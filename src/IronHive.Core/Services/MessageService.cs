using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using IronHive.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class MessageService : IMessageService
{
    private readonly IServiceProvider _services;
    private readonly IProviderRegistry _providers;
    private readonly IToolCollection _tools;
    private readonly IToolResultFilter? _resultFilter;

    public MessageService(IServiceProvider services, IProviderRegistry providers, IToolCollection tools)
    {
        _services = services;
        _providers = providers;
        _tools = tools;
        _resultFilter = services.GetService<IToolResultFilter>();
    }

    private static string BuildResponseModel(string provider, string model)
        => $"{provider}/{model}";
    private static string? BuildResponseId(string provider, string? responseId) 
        => !string.IsNullOrWhiteSpace(responseId)
            ? $"{provider}_{responseId}" : null;
    private static string? ExtractResponseId(string provider, string? responseId)
        => !string.IsNullOrWhiteSpace(responseId) 
            ? responseId.StartsWith($"{provider}_", StringComparison.Ordinal) 
            ? responseId[(provider.Length + 1)..] : responseId 
            : null;

    /// <inheritdoc />
    public async Task<MessageResponse> GenerateMessageAsync(
        MessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var generator = ResolveGenerator(request.Provider);

        string? responseId = null;
        MessageDoneReason? reason;
        MessageTokenUsage? usage;
        Message? message;

        var counter = new LimitedCounter(request.MaxLoopCount);
        var req = new MessageGenerationRequest
        {
            PreviousId = ExtractResponseId(request.Provider, request.PreviousId),
            Model = request.Model,
            ThinkingEffort = request.ThinkingEffort,
            Messages = request.Messages,
            System = request.System,
            Tools = _tools.FilterBy(request.Tools.Select(t => t.Name)),
            Output = request.Output,
            MaxTokens = request.MaxTokens,
            TopK = request.TopK,
            TopP = request.TopP,
            Temperature = request.Temperature,
            StopSequences = request.StopSequences,
        };

        var timer = Stopwatch.StartNew();
        do
        {
            if (req.Messages.LastOrDefault() is { Role: MessageRole.Assistant } last)
            {
                message = last;
                await foreach (var _ in ProcessToolContentAsync(message, request.Tools, cancellationToken).ConfigureAwait(false))
                { }
            }
            else
            {
                message = new Message { Role = MessageRole.Assistant };
            }

            var res = await generator.GenerateMessageAsync(req, cancellationToken).ConfigureAwait(false);
            responseId = res.ResponseId; // 마지막 루프의 ResponseId 채택

            foreach (var content in res.Message?.Content ?? [])
            {
                message.Content.Add(content);
            }
            if (req.Messages.LastOrDefault() is not { Role: MessageRole.Assistant })
            {
                req.Messages.Add(message);
            }
            reason = res.DoneReason;
            usage = res.TokenUsage;
        }
        while (counter.TryIncrement() && ShouldContinue(reason, message, cancellationToken));
        timer.Stop();

        return new MessageResponse
        {
            ResponseId = BuildResponseId(request.Provider, responseId),
            DoneReason = reason,
            Message = message,
            TokenUsage = usage,
            Model = BuildResponseModel(request.Provider, request.Model),
            Duration = timer.Elapsed,
            Timestamp = DateTime.UtcNow,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var generator = ResolveGenerator(request.Provider);

        string? responseId = null;
        MessageDoneReason? reason = null;
        MessageTokenUsage? usage = null;
        Message? message = null;

        bool beginSent = false;
        var counter = new LimitedCounter(request.MaxLoopCount);
        var req = new MessageGenerationRequest
        {
            PreviousId = request.PreviousId,
            Model = request.Model,
            ThinkingEffort = request.ThinkingEffort,
            Messages = request.Messages,
            System = request.System,
            Tools = _tools.FilterBy(request.Tools.Select(t => t.Name)),
            Output = request.Output,
            MaxTokens = request.MaxTokens,
            TopK = request.TopK,
            TopP = request.TopP,
            Temperature = request.Temperature,
            StopSequences = request.StopSequences,
        };

        var timer = Stopwatch.StartNew();
        do
        {
            if (req.Messages.LastOrDefault() is { Role: MessageRole.Assistant } last)
            {
                message = last;
                await foreach (var res in ProcessToolContentAsync(message, request.Tools, cancellationToken).ConfigureAwait(false))
                {
                    yield return res;
                }
            }
            else
            {
                message = new Message { Role = MessageRole.Assistant };
            }

            var stack = new List<MessageContent>();
            await foreach (var res in generator.GenerateStreamingMessageAsync(req, cancellationToken).ConfigureAwait(false))
            {
                // 에러가 아닌 첫 이벤트 수신 시 begin emit (제너레이터 정상 진입 확인)
                if (!beginSent && res is not StreamingMessageErrorResponse)
                {
                    yield return new StreamingMessageBeginResponse();
                    beginSent = true;
                }

                if (res is StreamingMessageBeginResponse)
                {
                    continue; // 제너레이터의 begin은 suppress
                }
                else if (res is StreamingMessageErrorResponse mer)
                {
                    yield return mer;
                }
                else if (res is StreamingContentAddedResponse car)
                {
                    car.Index = message.Content.Count + car.Index;
                    stack.Add(car.Content);
                    yield return car;
                }
                else if (res is StreamingContentDeltaResponse cdr)
                {
                    var content = stack.ElementAt(cdr.Index);
                    content.Merge(cdr.Delta);
                    cdr.Index = message.Content.Count + cdr.Index;
                    yield return cdr;
                }
                else if (res is StreamingContentUpdatedResponse cur)
                {
                    var content = stack.ElementAt(cur.Index);
                    content.Update(cur.Updated);
                    cur.Index = message.Content.Count + cur.Index;
                    yield return cur;
                }
                else if (res is StreamingContentCompletedResponse ccr)
                {
                    var stackItem = stack.ElementAt(ccr.Index);

                    // 툴은 suppress → ProcessToolContentAsync에서 inprogress/updated/completed 순서로 emit
                    if (stackItem is ToolMessageContent)
                        continue;

                    yield return new StreamingContentCompletedResponse
                    {
                        Index = message.Content.Count + ccr.Index,
                        Content = stackItem
                    };
                }
                else if (res is StreamingMessageDoneResponse mdr)
                {
                    reason = mdr.DoneReason;
                    usage = mdr.TokenUsage;
                    responseId = mdr.ResponseId; // 마지막 루프의 ResponseId 채택
                }
                else
                {
                    throw new InvalidOperationException("Unexpected response type.");
                }
            }

            foreach (var content in stack)
            {
                message.Content.Add(content);
            }
            if (req.Messages.LastOrDefault() is not { Role: MessageRole.Assistant })
            {
                req.Messages.Add(message);
            }
        }
        while (counter.TryIncrement() && ShouldContinue(reason, message, cancellationToken));
        timer.Stop();

        yield return new StreamingMessageDoneResponse
        {
            ResponseId = BuildResponseId(request.Provider, responseId),
            DoneReason = reason,
            Message = message,
            TokenUsage = usage,
            Model = BuildResponseModel(request.Provider, request.Model),
            Duration = timer.Elapsed,
            Timestamp = DateTime.UtcNow,
        };
    }

    private IMessageGenerator ResolveGenerator(string? provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            var entries = _providers.Entries<IMessageGenerator>().ToList();
            if (entries.Count == 0)
                throw new InvalidOperationException(
                    "No message generators are registered. Call AddMessageGenerator() during setup.");
            if (entries.Count > 1)
                throw new InvalidOperationException(
                    $"Multiple message generators are registered ({string.Join(", ", entries.Select(e => e.Key))}). " +
                    "Specify a provider via MessageRequest.Provider.");
            return entries[0].Value;
        }

        if (!_providers.TryGet<IMessageGenerator>(provider, out var generator))
            throw new KeyNotFoundException($"Message generator '{provider}' is not registered.");
        return generator;
    }

    /// <summary>
    /// 도구 컨텐츠를 처리합니다.
    /// </summary>
    private async IAsyncEnumerable<StreamingMessageResponse> ProcessToolContentAsync(
        Message message,
        IEnumerable<ToolItem> toolItems,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        const int maxConcurrent = 3;
        var semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
        var channel = Channel.CreateBounded<StreamingMessageResponse>(new BoundedChannelOptions(maxConcurrent * 4)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        var tasks = new List<Task>();
        foreach (var (item, idx) in message.Content.Select((x, i) => (x, i)))
        {
            if (item is not ToolMessageContent tmc)
                continue;
            if (tmc.IsCompleted)
                continue;
            if (!tmc.IsApproved)
                continue;

            var task = Task.Run(async () =>
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    await channel.Writer.WriteAsync(new StreamingContentInProgressResponse
                    {
                        Index = idx,
                    }, cancellationToken).ConfigureAwait(false);

                    var options = toolItems.FirstOrDefault(t => string.Equals(t.Name, tmc.Name, StringComparison.Ordinal))?.Options;
                    var input = new ToolInput(tmc.Input, options, _services);
                    tmc.Output = _tools.TryGet(tmc.Name, out var tool)
                        ? await tool.InvokeAsync(input, cancellationToken).ConfigureAwait(false)
                        : ToolOutput.Failure($"Could not find tool '{tmc.Name}', invocation failed.");

                    if (_resultFilter is not null && tmc.Output is { Result: not null })
                    {
                        tmc.Output = _resultFilter.Filter(tmc.Name, tmc.Output);
                    }

                    await channel.Writer.WriteAsync(new StreamingContentCompletedResponse
                    {
                        Index = idx,
                        Content = tmc
                    }, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error processing tool content for {tmc.Name} at index {idx}.", ex);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken);
            tasks.Add(task);
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, CancellationToken.None);

        await foreach (var res in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return res;
        }
    }

    private static bool ShouldContinue(MessageDoneReason? endReason, Message? message, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (endReason == MessageDoneReason.EndTurn)
            return false;
        if (endReason == MessageDoneReason.StopSequence)
            return false;
        if (message != null && message.Content.OfType<ToolMessageContent>().Any(t => !t.IsApproved))
            return false;

        return true;
    }
}

using System.Runtime.CompilerServices;
using System.Threading.Channels;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Utilities;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class MessageService : IMessageService
{
    private readonly IReadOnlyDictionary<string, IMessageGenerator> _generators;
    private readonly IReadOnlyList<IMessageMiddleware> _middlewares;

    internal MessageService(
        IReadOnlyDictionary<string, IMessageGenerator> generators,
        IReadOnlyList<IMessageMiddleware>? middlewares = null)
    {
        _generators = generators;
        _middlewares = middlewares ?? [];
    }

    /// <inheritdoc />
    public async Task<MessageResponse> GenerateMessageAsync(
        MessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var generator = GetRequiredGenerator(request.Provider);
        var pipeline = BuildPipeline(generator, _middlewares, cancellationToken);
        var context = new MessageContext(request, req => ConfigureGeneration(request, req));

        for (var turn = 0; turn < context.MaxTurns; turn++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            context.CurrentTurn = turn;
            context.BeginTurn();

            await ExecuteToolsAsync(context.CurrentMessage, request.Tools, request.ToolOptions, cancellationToken).ConfigureAwait(false);

            var res = await pipeline(context).ConfigureAwait(false);
            context.TrackedId = res.ResponseId;
            context.TurnReason = res.DoneReason;
            context.TokenUsage = res.TokenUsage;

            context.CurrentMessage ??= new Message { Role = MessageRole.Assistant };
            foreach (var content in res.Message?.Content ?? [])
            {
                context.CurrentMessage.Content.Add(content);
            }

            if (!context.ShouldContinue())
                break;
        }

        List<Suggestion>? suggestions = null;
        if (request.Suggestions != null && context.CurrentMessage != null)
        {
            var collector = new SuggestionCollector();
            foreach (var content in context.CurrentMessage.Content)
            {
                if (content is TextMessageContent tc)
                    tc.Value = collector.Feed(tc.Value);
            }
            suggestions = collector.Drain();
        }

        return new MessageResponse
        {
            ResponseId = BuildResponseId(request.Provider, context.TrackedId),
            DoneReason = context.TurnReason,
            Message = context.CurrentMessage,
            TokenUsage = context.TokenUsage,
            Model = request.Model,
            Duration = context.Elapsed,
            Timestamp = DateTime.UtcNow,
            Suggestions = suggestions,
            Items = context.Items,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var generator = GetRequiredGenerator(request.Provider);
        var pipeline = BuildStreamingPipeline(generator, _middlewares, cancellationToken);
        var context = new MessageContext(request, req => ConfigureGeneration(request, req));

        var beginSent = false;
        var parser = request.Suggestions != null ? new SuggestionCollector() : null;

        for (var turn = 0; turn < context.MaxTurns; turn++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            context.CurrentTurn = turn;
            context.BeginTurn();

            await foreach (var res in ExecuteStreamingToolsAsync(context.CurrentMessage, request.Tools, request.ToolOptions, cancellationToken).ConfigureAwait(false))
            {
                yield return res;
            }

            // 이번 턴의 상대 인덱스를 절대 인덱스로 바꾸기 위한 기준값. 턴 도중에는 CurrentMessage가 바뀌지 않으므로 한 번만 계산합니다.
            var baseIndex = context.CurrentMessage?.Content.Count ?? 0;
            var stack = new List<MessageContent>();

            await foreach (var res in pipeline(context).ConfigureAwait(false))
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
                    var absoluteIdx = baseIndex + car.Index;
                    MessageContent tracked;

                    if (parser != null && car.Content is TextMessageContent tmc)
                    {
                        tracked = new TextMessageContent
                        {
                            Signature = tmc.Signature,
                            Value = parser.Feed(tmc.Value),
                        };
                    }
                    else
                    {
                        tracked = car.Content;
                    }

                    stack.Add(tracked);
                    yield return new StreamingContentAddedResponse
                    {
                        Index = absoluteIdx,
                        Content = tracked
                    };
                }
                else if (res is StreamingContentDeltaResponse cdr)
                {
                    if (parser != null && cdr.Delta is TextDeltaContent tdc)
                    {
                        var text = parser.Feed(tdc.Value);
                        if (text.Length > 0)
                        {
                            stack.ElementAt(cdr.Index).Merge(new TextDeltaContent { Value = text });
                            yield return new StreamingContentDeltaResponse
                            {
                                Index = baseIndex + cdr.Index,
                                Delta = new TextDeltaContent { Value = text }
                            };
                        }
                    }
                    else
                    {
                        var content = stack.ElementAt(cdr.Index);
                        content.Merge(cdr.Delta);
                        cdr.Index = baseIndex + cdr.Index;
                        yield return cdr;
                    }
                }
                else if (res is StreamingContentUpdatedResponse cur)
                {
                    var content = stack.ElementAt(cur.Index);
                    content.Update(cur.Updated);
                    cur.Index = baseIndex + cur.Index;
                    yield return cur;
                }
                else if (res is StreamingContentCompletedResponse ccr)
                {
                    var stackItem = stack.ElementAt(ccr.Index);

                    // 툴은 suppress → ExecuteStreamingToolsAsync에서 inprogress/updated/completed 순서로 emit
                    if (stackItem is ToolMessageContent)
                        continue;

                    yield return new StreamingContentCompletedResponse
                    {
                        Index = baseIndex + ccr.Index,
                        Content = stackItem
                    };
                }
                else if (res is StreamingMessageDoneResponse mdr)
                {
                    context.TurnReason = mdr.DoneReason;
                    context.TokenUsage = mdr.TokenUsage;
                    context.TrackedId = mdr.ResponseId;
                }
                else
                {
                    // 알려지지 않은 타입은 미들웨어가 정의한 커스텀 시그널로 간주하고 그대로 투과시킨다.
                    // (인덱스 리베이싱 등 content 관련 가공 대상이 아니므로 별도 처리 없이 pass-through)
                    yield return res;
                }
            }

            context.CurrentMessage ??= new Message { Role = MessageRole.Assistant };
            foreach (var content in stack)
            {
                context.CurrentMessage.Content.Add(content);
            }

            if (!context.ShouldContinue())
                break;
        }

        yield return new StreamingMessageDoneResponse
        {
            ResponseId = BuildResponseId(request.Provider, context.TrackedId),
            DoneReason = context.TurnReason,
            Message = context.CurrentMessage,
            TokenUsage = context.TokenUsage,
            Model = request.Model,
            Duration = context.Elapsed,
            Timestamp = DateTime.UtcNow,
            Suggestions = parser?.Drain(),
            Items = context.Items,
        };
    }

    /// <inheritdoc />
    public Task<int> CountTokensAsync(
        MessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var generator = GetRequiredGenerator(request.Provider);
        var req = new MessageGenerationRequest
        {
            Model = request.Model,
            ThinkingEffort = request.ThinkingEffort,
            Messages = request.Messages,
            System = request.System,
            Tools = request.Tools,
            OutputFormat = request.OutputFormat,
            MaxTokens = request.MaxTokens,
        };
        return generator.CountTokensAsync(req, cancellationToken);
    }

    // ---- 제너레이터 조회 ----

    private IMessageGenerator GetRequiredGenerator(string? provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            var entries = _generators.ToList();
            if (entries.Count == 0)
                throw new InvalidOperationException(
                    "No message generators are registered. Call AddMessageGenerator() during setup.");
            if (entries.Count > 1)
                throw new InvalidOperationException(
                    $"Multiple message generators are registered ({string.Join(", ", entries.Select(e => e.Key))}). " +
                    "Specify a provider via MessageRequest.Provider.");
            return entries[0].Value;
        }

        if (!_generators.TryGetValue(provider, out var generator))
            throw new KeyNotFoundException($"Message generator '{provider}' is not registered.");
        return generator;
    }

    // ---- MessageContext 구성 ----

    /// <summary>
    /// MessageContext 생성 시 Provider 종속적인 보정(PreviousId 추출, 제안 프롬프트 병합)을 적용합니다.
    /// MessageContext(Abstractions)는 Provider 문자열이나 SuggestionCollector(Core 내부)를 알 필요가 없도록
    /// 이 보정을 MessageService(Core)에서 처리합니다.
    /// </summary>
    private static void ConfigureGeneration(MessageRequest request, MessageGenerationRequest generation)
    {
        generation.PreviousId = ExtractResponseId(request.Provider, request.PreviousId);
        if (request.Suggestions != null)
            generation.System = SuggestionCollector.Prompt(request.System, request.Suggestions);
    }

    // ---- 미들웨어 파이프라인 구성 ----

    private static Func<MessageContext, Task<MessageResponse>> BuildPipeline(
        IMessageGenerator generator,
        IReadOnlyList<IMessageMiddleware> middlewares,
        CancellationToken cancellationToken)
    {
        Func<MessageContext, Task<MessageResponse>> pipeline =
            ctx => generator.GenerateMessageAsync(ctx.Request, cancellationToken);

        for (var i = middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = middlewares[i];
            var next = pipeline;
            pipeline = ctx => middleware.GenerateAsync(ctx, next, cancellationToken);
        }

        return pipeline;
    }

    private static Func<MessageContext, IAsyncEnumerable<StreamingMessageResponse>> BuildStreamingPipeline(
        IMessageGenerator generator,
        IReadOnlyList<IMessageMiddleware> middlewares,
        CancellationToken cancellationToken)
    {
        Func<MessageContext, IAsyncEnumerable<StreamingMessageResponse>> pipeline =
            ctx => generator.GenerateStreamingMessageAsync(ctx.Request, cancellationToken);

        for (var i = middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = middlewares[i];
            var next = pipeline;
            pipeline = ctx => middleware.GenerateStreamingAsync(ctx, next, cancellationToken);
        }

        return pipeline;
    }

    // ---- 응답 id 포맷팅 ----

    private static string? BuildResponseId(string provider, string? responseId)
        => !string.IsNullOrWhiteSpace(responseId)
            ? $"{provider}_{responseId}" : null;

    private static string? ExtractResponseId(string provider, string? responseId)
        => !string.IsNullOrWhiteSpace(responseId)
            ? responseId.StartsWith($"{provider}_", StringComparison.Ordinal)
            ? responseId[(provider.Length + 1)..] : responseId
            : null;

    // ---- 도구 실행 루프 ----

    /// <summary>
    /// 도구 하나를 실행하고 Output을 채웁니다. OnBeforeInvoke/OnAfterInvoke 훅과 타임아웃 처리를 포함합니다.
    /// </summary>
    private static async Task ExecuteToolAsync(
        ToolMessageContent tmc,
        IToolCollection? tools,
        ToolOptions? toolOptions,
        CancellationToken cancellationToken)
    {
        if (toolOptions?.OnBeforeInvoke is not null)
        {
            await toolOptions.OnBeforeInvoke(tmc, cancellationToken).ConfigureAwait(false);
        }

        // OnBeforeInvoke에서 이미 Output을 채웠다면 실제 도구 실행을 스킵합니다.
        if (tmc.Output is null)
        {
            var input = new ToolInput(tmc.Input);

            if (tools?.TryGet(tmc.Name, out var tool) == true)
            {
                var timeout = toolOptions?.Timeout;
                using var timeoutCts = timeout.HasValue
                    ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                    : null;
                timeoutCts?.CancelAfter(timeout.GetValueOrDefault());
                var ct = timeoutCts?.Token ?? cancellationToken;
                try
                {
                    tmc.Output = await tool.InvokeAsync(input, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (timeoutCts?.IsCancellationRequested == true
                                                         && !cancellationToken.IsCancellationRequested)
                {
                    tmc.Output = ToolOutput.Failure($"Tool '{tmc.Name}' timed out after {timeout!.Value.TotalSeconds:0.#}s.");
                }
            }
            else
            {
                tmc.Output = ToolOutput.Failure($"Could not find tool '{tmc.Name}', invocation failed.");
            }
        }

        if (toolOptions?.OnAfterInvoke is not null)
        {
            await toolOptions.OnAfterInvoke(tmc, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 논스트리밍 경로용 도구 실행입니다. 진행 상황을 보고할 필요가 없으므로 채널 없이 바로 완료를 기다립니다.
    /// </summary>
    private static async Task ExecuteToolsAsync(
        Message? message,
        IToolCollection? tools,
        ToolOptions? toolOptions,
        CancellationToken cancellationToken)
    {
        var concurrent = toolOptions?.MaxParallel ?? 3;
        var semaphore = new SemaphoreSlim(concurrent, concurrent);

        var tasks = message?.Content
            .OfType<ToolMessageContent>()
            .Where(tmc => !tmc.IsCompleted && tmc.IsApproved)
            .Select(tmc => Task.Run(async () =>
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    await ExecuteToolAsync(tmc, tools, toolOptions, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error processing tool content for {tmc.Name}.", ex);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken)) ?? [];

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// 스트리밍 경로용 도구 실행입니다. 진행 상황(in_progress/completed)을 이벤트로 보고합니다.
    /// </summary>
    private static async IAsyncEnumerable<StreamingMessageResponse> ExecuteStreamingToolsAsync(
        Message? message,
        IToolCollection? tools,
        ToolOptions? toolOptions,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var concurrent = toolOptions?.MaxParallel ?? 3;
        var semaphore = new SemaphoreSlim(concurrent, concurrent);
        var channel = Channel.CreateBounded<StreamingMessageResponse>(new BoundedChannelOptions(concurrent * 4)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        var tasks = new List<Task>();
        foreach (var (tmc, idx) in message?.Content.Select((x, i) => (x, i)) ?? [])
        {
            if (tmc is not ToolMessageContent tool || tool.IsCompleted || !tool.IsApproved)
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

                    await ExecuteToolAsync(tool, tools, toolOptions, cancellationToken).ConfigureAwait(false);

                    await channel.Writer.WriteAsync(new StreamingContentCompletedResponse
                    {
                        Index = idx,
                        Content = tool
                    }, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error processing tool content for {tool.Name} at index {idx}.", ex);
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
}

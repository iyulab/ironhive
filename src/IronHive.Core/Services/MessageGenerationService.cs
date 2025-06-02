using System.Runtime.CompilerServices;
using IronHive.Abstractions.Message;
using IronHive.Abstractions.Message.Content;
using IronHive.Abstractions.Message.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core.Utilities;

namespace IronHive.Core.Services;

public class MessageGenerationService : IMessageGenerationService
{
    private readonly Dictionary<string, IMessageGenerationProvider> _providers;
    private readonly IToolPluginManager _plugins;

    public MessageGenerationService(IEnumerable<IMessageGenerationProvider> providers, IToolPluginManager plugins)
    {
        _providers = providers.ToDictionary(p => p.ProviderName, p => p);
        _plugins = plugins;
    }

    /// <inheritdoc />
    public async Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(request.Provider, out var provider))
            throw new KeyNotFoundException($"Service key '{request.Provider}' not found.");

        // 파라미터를 받아서 요청을 생성합니다.
        MessageDoneReason? reason = null;
        MessageTokenUsage? usage = null;
        AssistantMessage? message = null;

        // 루프 시작
        bool next;
        var counter = new LimitedCounter(request.MaxLoopCount);
        do
        {
            // 마지막 메시지가 어시스턴트 메시지인 경우, 툴 사용을 확인하고, 메시지를 이어갑니다.
            if (request.Messages.LastOrDefault() is AssistantMessage last)
            {
                message = last;
                await foreach (var _ in ProcessToolContentAsync(message, cancellationToken))
                { }
            }
            else
            {
                message = new AssistantMessage { Id = Guid.NewGuid().ToString() };
            }

            var res = await provider.GenerateMessageAsync(request, cancellationToken);

            foreach (var content in res.Message?.Content ?? [])
            {
                // 툴 컨텐츠 추가
                message.Content.Add(content is ToolMessageContent tool
                    ? PrepareToolContent(tool, request.Tools)
                    : content);
            }

            // 요청 메시지의 마지막이 어시스턴트가 아닌 경우, 메시지를 추가합니다.
            if (request.Messages.LastOrDefault() is not AssistantMessage)
            {
                request.Messages.Add(message);
            }
            reason = res.DoneReason; // 생성 종료 이유 업데이트
            usage = res.TokenUsage; // 토큰 사용량 업데이트

            counter.Increment();
            next = ShouldContinue(reason, message, counter, cancellationToken);
        }
        while (next);

        return new MessageResponse
        {
            DoneReason = reason,
            Message = message,
            TokenUsage = usage,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(request.Provider, out var provider))
            throw new KeyNotFoundException($"Service key '{request.Provider}' not found.");

        string messageId = string.Empty;
        MessageDoneReason? reason = null;
        MessageTokenUsage? usage = null;

        // 루프 시작
        bool next;
        var counter = new LimitedCounter(request.MaxLoopCount);
        do
        {
            // 마지막 메시지가 어시스턴트 메시지인 경우, 툴 사용을 확인하고, 메시지를 이어갑니다.
            if (request.Messages.LastOrDefault() is AssistantMessage message)
            {
                messageId = message.Id;
                await foreach (var res in ProcessToolContentAsync(message, cancellationToken))
                {
                    yield return res;
                }
            }
            // 마지막 메시지가 어시스턴트 메시지가 아닌 경우, 새로운 메시지를 생성합니다.
            else
            {
                messageId = Guid.NewGuid().ToString();
                message = new AssistantMessage { Id = messageId };
            }

            // 현재 루프에서 생성된 컨텐츠를 저장할 메시지 컨텐츠 스택 객체
            var stack = new List<MessageContent>();
            await foreach (var res in provider.GenerateStreamingMessageAsync(request, cancellationToken))
            {
                if (res is StreamingMessageBeginResponse mbr)
                {
                    mbr.Id = messageId;
                    yield return mbr;
                }
                else if (res is StreamingContentAddedResponse car)
                {
                    car.Index = message.Content.Count + car.Index;
                    stack.Add(car.Content is ToolMessageContent tool
                        ? PrepareToolContent(tool, request.Tools)
                        : car.Content);
                    yield return car;
                }
                else if (res is StreamingContentDeltaResponse cdr)
                {
                    cdr.Index = message.Content.Count + cdr.Index;
                    if (cdr.Delta is TextDeltaContent text)
                    {
                        var content = stack.ElementAt(cdr.Index) as TextMessageContent
                            ?? throw new InvalidOperationException("Expected TextMessageContent type for delta processing.");
                        content.Value += text.Value;
                        yield return cdr;
                    }
                    else if (cdr.Delta is ThinkingDeltaContent thinking)
                    {
                        var content = stack.ElementAt(cdr.Index) as ThinkingMessageContent
                            ?? throw new InvalidOperationException("Expected AssistantThinkingContent type for delta processing.");
                        content.Value += thinking.Data;
                        yield return cdr;
                    }
                    else if (cdr.Delta is ToolDeltaContent tool)
                    {
                        var content = stack.ElementAt(cdr.Index) as ToolMessageContent
                            ?? throw new InvalidOperationException("Expected ToolMessageContent type for delta processing.");
                        content.Input += tool.Input;
                        yield return cdr;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unexpected delta type.");
                    }
                }
                else if (res is StreamingContentUpdatedResponse cur)
                {
                    cur.Index = message.Content.Count + cur.Index;
                    if (cur.Updated is ThinkingUpdatedContent thinking)
                    {
                        var content = stack.ElementAt(cur.Index) as ThinkingMessageContent
                            ?? throw new InvalidOperationException("Expected ThinkingMessageContent type for updated content.");
                        content.Id = thinking.Id;
                        yield return cur;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unexpected updated content type.");
                    }
                }
                else if (res is StreamingContentCompletedResponse ccr)
                {
                    ccr.Index = message.Content.Count + ccr.Index;
                    yield return ccr;
                }
                else if (res is StreamingMessageDoneResponse mdr)
                {
                    // 스트리밍 메시지 종료 응답인 경우, 종료 이유와 토큰 사용량을 업데이트합니다.
                    reason = mdr.DoneReason;
                    usage = mdr.TokenUsage;
                }
                else
                {
                    throw new InvalidOperationException("Unexpected response type.");
                }
            }

            // 스택에 있는 컨텐츠를 메시지에 추가합니다.
            foreach (var content in stack)
            {
                message.Content.Add(content);
            }

            // 요청 메시지의 마지막이 어시스턴트가 아닌 경우, 메시지를 추가합니다.
            if (request.Messages.LastOrDefault() is not AssistantMessage)
            {
                request.Messages.Add(message);
            }

            counter.Increment();
            next = ShouldContinue(reason, message, counter, cancellationToken);
        }
        while (next);

        // 마지막 메시지를 전달 합니다.
        yield return new StreamingMessageDoneResponse
        {
            Id = messageId,
            DoneReason = reason,
            TokenUsage = usage,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 도구 컨텐츠를 준비 합니다.
    /// </summary>
    private static ToolMessageContent PrepareToolContent(ToolMessageContent content, IEnumerable<ToolDescriptor> tools)
    {
        // 도구의 아이디가 없는 경우 임의 생성합니다.
        if (string.IsNullOrEmpty(content.Id))
        {
            content.Id = $"tool_{Guid.NewGuid().ToShort()}";
        }

        // 도구가 승인을 필요로 하는 경우
        if (!string.IsNullOrEmpty(content.Name))
        {
            var required = tools.FirstOrDefault(t => t.Name == content.Name)?.RequiresApproval ?? false;
            content.ApprovalStatus = required
                ? ToolApprovalStatus.Requires
                : ToolApprovalStatus.NotRequired;
        }

        return content;
    }

    /// <summary>
    /// 도구 컨텐츠를 처리합니다.
    /// </summary>
    private async IAsyncEnumerable<StreamingMessageResponse> ProcessToolContentAsync(
        AssistantMessage message,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < message.Content.Count; i++)
        {
            var content = message.Content.ElementAt(i);
            if (content is not ToolMessageContent tool)
                continue;

            // 도구 이름이 없는 경우 예상치 못한 오류입니다.
            if (string.IsNullOrEmpty(tool.Name))
                throw new KeyNotFoundException($"Tool name is not found. Please check the tool name in the message content.");

            yield return new StreamingContentInProgressResponse
            {
                Index = i,
            };

            // 실행 거부된 경우 혹은 승인 확인이 안된 경우
            if (tool.ApprovalStatus == ToolApprovalStatus.Rejected ||
                tool.ApprovalStatus == ToolApprovalStatus.Requires)
            {
                tool.Output = ToolOutput.InvocationRejected();
            }
            // 도구 호출
            else
            {
                var input = new ToolInput(tool.Input);
                tool.Output = await _plugins.InvokeAsync(tool.Name, input, cancellationToken);
            }

            yield return new StreamingContentUpdatedResponse
            {
                Index = i,
                Updated = new ToolUpdatedContent
                {
                    Output = tool.Output,
                }
            };
        }
    }

    /// <summary>
    /// 루프를 계속 진행할지 여부를 결정합니다.
    /// </summary>
    private static bool ShouldContinue(MessageDoneReason? endReason, AssistantMessage message, LimitedCounter counter, CancellationToken token)
    {
        // 취소 요청이 있는 경우 종료 Throw
        if (token.IsCancellationRequested)
            throw new OperationCanceledException(token);
        // 최대 루프 카운트 초과할 경우 종료 Throw
        if (counter.ReachedMax)
            throw new InvalidOperationException("Max loop count exceeded.");

        // 도구 호출이 아닌 경우 종료
        if (endReason != MessageDoneReason.ToolCall)
            return false;
        // 도구 승인 요청이 있는 경우 종료
        if (message.RequiresApproval)
            return false;

        return true;
    }
}

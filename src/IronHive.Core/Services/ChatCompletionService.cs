using DocumentFormat.OpenXml.Bibliography;
using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;
using IronHive.Core.Utilities;
using ModelContextProtocol.Protocol.Types;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IronHive.Core.Services;

public class ChatCompletionService : IChatCompletionService
{
    private readonly IHiveServiceStore _store;

    public ChatCompletionService(IHiveServiceStore store)
    {
        _store = store;
    }

    /// <inheritdoc />
    public Task<ChatCompletionResponse<AssistantMessage>> GenerateMessageAsync(
        UserMessage message, 
        ChatCompletionOptions options, 
        CancellationToken cancellationToken = default)
    {
        var messages = new MessageCollection { message };
        return GenerateMessageAsync(messages, options, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ChatCompletionResponse<IAssistantContent>> GenerateStreamingMessageAsync(
        UserMessage message, 
        ChatCompletionOptions options, 
        CancellationToken cancellationToken = default)
    {
        var messages = new MessageCollection { message };
        return GenerateStreamingMessageAsync(messages, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResponse<AssistantMessage>> GenerateMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetService<IChatCompletionConnector>(options.Provider, out var conn))
            throw new KeyNotFoundException($"Service key '{options.Provider}' not found.");

        // 파라미터를 받아서 요청을 생성합니다.
        var req = CreateChatCompletionRequest(messages, options);
        EndReason? reason;
        TokenUsage? usage;
        AssistantMessage message;

        // 루프 시작
        bool next;
        var counter = new LoopCounter(options.MaxLoopCount);
        do
        {
            // 마지막 메시지가 어시스턴트 메시지인 경우, 툴 사용을 확인하고, 메시지를 이어갑니다.
            if (req.Messages.LastOrDefault() is AssistantMessage last)
            {
                await foreach (var _ in ProcessToolContentAsync(last, req.Tools, cancellationToken))
                { }
            }

            var res = await conn.GenerateMessageAsync(req, cancellationToken);

            message = req.Messages.LastOrCreate<AssistantMessage>();
            foreach (var content in res.Data?.Content ?? [])
            {
                // 툴 컨텐츠 추가
                message.Content.Add(content is AssistantToolContent tool
                    ? PrepareToolContent(tool, req.Tools)
                    : content);
            }
            reason = res.EndReason; // 생성 종료 이유 업데이트
            usage = res.TokenUsage; // 토큰 사용량 업데이트

            next = ShouldContinue(reason, message, counter, cancellationToken);
            counter.Increment();
        }
        while (next);

        return new ChatCompletionResponse<AssistantMessage>
        {
            EndReason = reason,
            Data = message,
            TokenUsage = usage,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionResponse<IAssistantContent>> GenerateStreamingMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetService<IChatCompletionConnector>(options.Provider, out var conn))
            throw new KeyNotFoundException($"Service key '{options.Provider}' not found.");

        // 커넥터에 전달할 요청 객체를 생성합니다.
        var req = CreateChatCompletionRequest(messages, options);
        EndReason? reason = null;
        TokenUsage? usage = null;

        // 루프 시작
        bool next;
        var counter = new LoopCounter(options.MaxLoopCount);
        do
        {
            // 마지막 메시지가 어시스턴트 메시지인 경우, 툴 사용을 확인하고, 메시지를 이어갑니다.
            if (req.Messages.LastOrDefault() is AssistantMessage message)
            {
                await foreach (var res in ProcessToolContentAsync(message, req.Tools, cancellationToken))
                {
                    yield return res;
                }
            }
            // 마지막 메시지가 어시스턴트 메시지가 아닌 경우, 새로운 메시지를 생성합니다.
            else
            {
                message = new AssistantMessage();
            }

            // 현재 루프에서 생성된 컨텐츠를 저장할 메시지 컨텐츠 스택 객체
            var stack = new AssistantContentCollection();
            await foreach (var res in conn.GenerateStreamingMessageAsync(req, cancellationToken))
            {
                // 생성 종료 이유
                if (res.EndReason != null)
                {
                    reason = res.EndReason;
                }

                // 토큰 사용량 업데이트
                if (res.TokenUsage != null)
                {
                    usage = res.TokenUsage;
                }

                // 컨텐츠 추가
                if (res.Data != null)
                {
                    // 텍스트 컨텐츠의 경우
                    if (res.Data is AssistantTextContent text)
                    {
                        // 마지막 컨텐츠가 텍스트인 경우 텍스트를 이어 붙입니다.
                        if (stack.LastOrDefault() is AssistantTextContent lastText)
                        {
                            lastText.Value ??= string.Empty;
                            lastText.Value += text.Value;
                        }
                        // 아닌 경우 새로운 텍스트 컨텐츠를 추가합니다.
                        else
                        {
                            stack.Add(text);
                        }

                        yield return new ChatCompletionResponse<IAssistantContent>
                        {
                            Data = new AssistantTextContent
                            {
                                // 실제 축적 인덱스 계산 (축적된 컨텐츠 + 현재 인덱스)
                                Index = (message.Content.Count + stack.Count) - 1,
                                Value = text.Value,
                            }
                        };
                    }
                    // 툴 컨텐츠의 경우
                    else if (res.Data is AssistantToolContent tool)
                    {
                        // 현재루프의 컨텐츠에서 해당하는 인덱스에 툴이 있는지 확인
                        var existing = stack.ElementAtOrDefault(tool.Index ?? 0);

                        // 있다면 스트리밍으로 나오는 Arguments를 이어 붙입니다.
                        if (existing is AssistantToolContent existingTool)
                        {
                            existingTool.Arguments ??= string.Empty;
                            existingTool.Arguments += tool.Arguments;
                            yield return new ChatCompletionResponse<IAssistantContent>
                            {
                                Data = existingTool
                            };
                        }
                        // 없다면 새로운 툴 컨텐츠를 추가합니다.
                        else
                        {
                            tool = PrepareToolContent(tool, req.Tools);
                            stack.Add(tool);
                            yield return new ChatCompletionResponse<IAssistantContent>
                            {
                                Data = new AssistantToolContent
                                {
                                    // 실제 인덱스 계산 (축적된 컨텐츠 + 현재 인덱스)
                                    Index = (message.Content.Count + stack.Count) - 1,
                                    ExecutionStatus = ToolExecutionStatus.Pending,
                                    ApprovalStatus = tool.ApprovalStatus,
                                    Id = tool.Id,
                                    Name = tool.Name,
                                }
                            };
                        }
                    }
                    // 추론 컨텐츠의 경우
                    else if (res.Data is AssistantThinkingContent thinking)
                    {
                        var existing = stack.ElementAtOrDefault(thinking.Index ?? 0);

                        // 현재루프의 컨텐츠에서 해당하는 인덱스에 추론이 있는지 확인
                        if (existing is AssistantThinkingContent existingThinking)
                        {
                            existingThinking.Value ??= string.Empty;
                            existingThinking.Value += thinking.Value;
                        }
                        // 없다면 새로운 컨텐츠를 스택에 추가합니다.
                        else
                        {
                            stack.Add(thinking);
                            existingThinking = thinking;
                        }

                        if (!string.IsNullOrEmpty(thinking.Id))
                        {
                            existingThinking.Id = thinking.Id;
                        }

                        yield return new ChatCompletionResponse<IAssistantContent>
                        {
                            Data = new AssistantThinkingContent
                            {
                                // 실제 인덱스 계산 (축적된 컨텐츠 + 현재 인덱스)
                                Index = (message.Content.Count + stack.Count) - 1,
                                Mode = existingThinking?.Mode ?? thinking.Mode,
                                Id = existingThinking?.Id ?? thinking.Id,
                                Value = thinking.Value,
                            }
                        };
                    }
                    // 이외의 컨텐츠는 지원하지 않습니다.
                    else
                    {
                        throw new InvalidOperationException("Unexpected content type.");
                    }
                }
            }

            // 스택에 있는 컨텐츠를 메시지에 추가합니다.
            foreach (var content in stack)
            {
                message.Content.Add(content);
            }

            // 요청 메시지의 마지막이 어시스턴트가 아닌 경우, 메시지를 추가합니다.
            if (req.Messages.LastOrDefault() is not AssistantMessage)
            {
                req.Messages.Add(message);
            }

            next = ShouldContinue(reason, message, counter, cancellationToken);
            counter.Increment();
        }
        while (next);

        // 마지막 메시지를 전달 합니다.
        yield return new ChatCompletionResponse<IAssistantContent>
        {
            EndReason = EndReason.EndTurn,
            TokenUsage = usage,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 채팅 요청 객체를 생성합니다.
    /// </summary>
    private static ChatCompletionRequest CreateChatCompletionRequest(
        MessageCollection messages, 
        ChatCompletionOptions options)
    {
        var request = new ChatCompletionRequest
        {
            Model = options.Model,
            System = options.Instructions,
            Tools = options.Tools ?? new ToolCollection(),
            Messages = messages.Clone(), // 메시지 복사
            MaxTokens = options.MaxTokens,
            Temperature = options.Temperature,
            TopK = options.TopK,
            TopP = options.TopP,
            StopSequences = options.StopSequences
        };

        return request;
    }

    /// <summary>
    /// 도구 컨텐츠를 준비 합니다.
    /// </summary>
    private static AssistantToolContent PrepareToolContent(
        AssistantToolContent content, 
        ToolCollection tools)
    {
        // 도구의 아이디가 없는 경우 임의 생성합니다.
        if (string.IsNullOrEmpty(content.Id))
        {
            content.Id = $"tool_{Guid.NewGuid().ToShort()}";
        }

        // 도구가 승인을 필요로 하는 경우
        if (!string.IsNullOrEmpty(content.Name) && tools.TryGetValue(content.Name, out var tool))
        {
            content.ApprovalStatus = tool.RequiresApproval 
                ? ToolApprovalStatus.Requires 
                : ToolApprovalStatus.NotRequired;
        }

        return content;
    }

    /// <summary>
    /// 도구 컨텐츠를 처리합니다.
    /// </summary>
    private static async IAsyncEnumerable<ChatCompletionResponse<IAssistantContent>> ProcessToolContentAsync(
        AssistantMessage message,
        ToolCollection tools,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var tc in message.Content.GetIncompleteToolContents())
        {
            // 도구 이름이 없는 경우 예상치 못한 오류입니다.
            if (string.IsNullOrEmpty(tc.Name))
                throw new KeyNotFoundException($"Tool name is not found. Please check the tool name in the message content.");

            // 실행 거부된 경우 혹은 승인 확인이 안된 경우
            if (tc.ApprovalStatus == ToolApprovalStatus.Rejected || 
                tc.ApprovalStatus == ToolApprovalStatus.Requires)
            {
                tc.Result = ToolResult.InvocationRejected();
            }
            // 도구 호출
            else if (tools.TryGetValue(tc.Name, out var tool))
            {
                tc.ExecutionStatus = ToolExecutionStatus.Running;
                yield return new ChatCompletionResponse<IAssistantContent> { Data = tc };
                tc.Result = await tool.InvokeAsync(tc.Arguments, cancellationToken);
            }
            // 도구를 찾을 수 없는 경우
            else
            {
                tc.Result = ToolResult.ToolNotFound(tc.Name);
            }

            tc.ExecutionStatus = ToolExecutionStatus.Completed;
            yield return new ChatCompletionResponse<IAssistantContent> { Data = tc };
        }
    }

    /// <summary>
    /// 루프를 계속 진행할지 여부를 결정합니다.
    /// </summary>
    private bool ShouldContinue(
        EndReason? endReason,
        AssistantMessage message,
        LoopCounter counter,
        CancellationToken token)
    {
        // 취소 요청이 있는 경우 종료 Throw
        if (token.IsCancellationRequested)
            throw new OperationCanceledException(token);
        // 최대 루프 카운트 초과할 경우 종료 Throw
        if (!counter.HasNext)
            throw new InvalidOperationException("Max loop count exceeded.");

        // 도구 호출이 아닌 경우 종료
        if (endReason != EndReason.ToolCall)
            return false;
        // 도구 승인 요청이 있는 경우 종료
        if (message.Content.RequiresToolApproval())
            return false;

        return true;
    }
}

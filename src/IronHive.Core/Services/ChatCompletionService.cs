using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;
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
        var request = CreateChatCompletionRequest(messages, options);

        // 생성 종료 이유
        EndReason? reason = null;

        // 루프를 돌면서 축적될 메시지 입니다.
        TokenUsage? usage = null;

        // 루프를 돌면서 축적될 메시지 입니다.
        AssistantMessage message;

        // 마지막 메시지가 어시스턴트 메시지인 경우
        // 툴 호출을 확인하고 결과를 생성합니다.
        // 유저가 툴사용을 허락했는지 확인하고 실행합니다.
        if (messages.LastOrDefault() is AssistantMessage assistantMessage)
        {
            message = assistantMessage;
            var toolContentGroup = message.Content.OfType<AssistantToolContent>() ?? [];
            foreach (var toolContent in toolContentGroup)
            {
                // 도구 이름이 없거나 결과가 이미 있는 경우 건너뜀
                if (string.IsNullOrEmpty(toolContent.Name) || toolContent.Result != null)
                {
                    continue;
                }
                // 도구 호출
                else if (request.Tools.TryGetValue(toolContent.Name, out var tool))
                {
                    if (tool.Permission == ToolPermission.Manual && !toolContent.IsAllowed)
                    {
                        // 도구가 승인이 요구되었고,
                        // 도구 사용을 확인하지 않은 경우 실패 처리
                        toolContent.DeniedToolInvoke();
                        continue;
                    }

                    var result = await tool.InvokeAsync(toolContent.Arguments, cancellationToken);
                    var data = JsonSerializer.Serialize(result.Data, JsonDefaultOptions.Options);

                    if (!result.IsSuccess)
                        toolContent.Failed(data);
                    else if (data.Length > 30_000)
                        toolContent.TooMuchResult();
                    else
                        toolContent.Completed(data);
                }
                // 도구를 찾을 수 없는 경우
                else
                {
                    toolContent.NotFoundTool();
                }
            }
        }
        // 마지막 메시지가 어시스턴트 메시지가 아닌 경우
        // 유저와의 대화를 시작합니다.
        else
        {
            message = new AssistantMessage();        
        }

        // 루프 시작
        while (!cancellationToken.IsCancellationRequested)
        {
            var res = await conn.GenerateMessageAsync(request, cancellationToken);

            // 생성 종료 이유
            if (res.EndReason != null)
            {
                reason = res.EndReason;
            }

            // 토큰 사용량 업데이트
            if (res.TokenUsage != null)
            {
                if (usage != null)
                {
                    // 두번째 이후
                    // 처음 입력토큰은 그대로 두고, 이후에 생성된 토큰만 추가
                    usage.OutputTokens += res.TokenUsage.OutputTokens;
                }
                else
                {
                    // 처음 첫번째 루프에서 토큰 사용량을 설정
                    usage = res.TokenUsage;
                }
            }

            // 메시지 컨텐츠 추가
            if (res.Data?.Content != null)
                message.Content.AddRange(res.Data.Content);

            // 메시지 추가(마지막 메시지가 유저가 아닌 경우 == 첫번째 루프인 경우)
            if (request.Messages.LastOrDefault() is not AssistantMessage)
                request.Messages.Add(message);

            // 도구 호출
            if (reason == EndReason.ToolCall)
            {
                bool approveRequired = false;
                var toolContentGroup = message.Content.OfType<AssistantToolContent>() ?? [];
                foreach (var toolContent in toolContentGroup)
                {
                    // 도구 이름이 없거나 결과가 이미 있는 경우 건너뜀
                    if (string.IsNullOrEmpty(toolContent.Name) || toolContent.Result != null)
                    {
                        continue;
                    }
                    // 도구 호출
                    else if (request.Tools.TryGetValue(toolContent.Name, out var tool))
                    {
                        if (tool.Permission == ToolPermission.Manual)
                        {
                            approveRequired = true; // 도구 사용을 확인하지 않은 경우
                            // 도구가 승인이 요구되는 겨우 건너뜀
                            continue;
                        }

                        var result = await tool.InvokeAsync(toolContent.Arguments, cancellationToken);
                        var data = JsonSerializer.Serialize(result.Data, JsonDefaultOptions.Options);

                        if (!result.IsSuccess)
                            toolContent.Failed(data);    // 실패 처리
                        else if (data.Length > 30_000)
                            toolContent.TooMuchResult(); // 결과가 너무 긴 경우
                        else
                            toolContent.Completed(data); // 성공 처리
                    }
                    // 도구를 찾을 수 없는 경우
                    else
                    {
                        toolContent.NotFoundTool();  // 도구를 찾을 수 없음
                    }
                }

                if (approveRequired)
                {
                    // 도구 사용을 확인하지 않은 경우
                    // 승인 요청을 위해 루프를 종료합니다.
                    break;
                }
            }
            // 도구 호출이 아닌 경우 종료
            else
            {
                break;
            }
        }

        return new ChatCompletionResponse<AssistantMessage>
        {
            EndReason = reason,
            TokenUsage = usage,
            Data = message,
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

        // 파라미터를 받아서 요청을 생성합니다.
        var request = CreateChatCompletionRequest(messages, options);

        // 생성 종료 이유
        EndReason? reason = null;

        // 루프를 돌면서 축적될 메시지 입니다.
        TokenUsage? usage = null;

        // 루프를 돌면서 축적될 메시지 입니다.
        AssistantMessage message;

        // 마지막 메시지가 어시스턴트 메시지인 경우
        // 툴 호출을 확인하고 결과를 생성합니다.
        // 유저가 툴사용을 허락했는지 확인하고 실행합니다.
        if (messages.LastOrDefault() is AssistantMessage assistantMessage)
        {
            message = assistantMessage;
            var toolContentGroup = message.Content.OfType<AssistantToolContent>();
            foreach (var content in toolContentGroup)
            {
                if (string.IsNullOrEmpty(content.Name) || content.Result != null)
                {
                    // 도구 이름이 없거나 결과가 이미 있는 경우 건너뜀
                    continue;
                }
                if (request.Tools.TryGetValue(content.Name, out var tool))
                {
                    if (tool.Permission == ToolPermission.Manual && !content.IsAllowed)
                    {
                        // 도구가 승인이 요구되었고,
                        // 도구 사용을 확인하지 않은 경우 실패 처리
                        content.DeniedToolInvoke();
                        yield return new ChatCompletionResponse<IAssistantContent>
                        {
                            Data = content
                        };
                    }
                    else
                    {
                        // 도구 호출
                        content.Status = ToolStatus.Running;
                        yield return new ChatCompletionResponse<IAssistantContent>
                        {
                            Data = content
                        };

                        var result = await tool.InvokeAsync(content.Arguments, cancellationToken);
                        var data = JsonSerializer.Serialize(result.Data, JsonDefaultOptions.Options);

                        if (!result.IsSuccess)
                            content.Failed(data);
                        else if (data.Length > 30_000)
                            content.TooMuchResult();
                        else
                            content.Completed(data);

                        yield return new ChatCompletionResponse<IAssistantContent>
                        {
                            Data = content
                        };
                    }
                }
                else
                {
                    content.NotFoundTool();
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = content
                    };
                }
            }
        }
        // 마지막 메시지가 어시스턴트 메시지가 아닌 경우
        // 채팅이 시작됩니다.
        else
        {
            message = new AssistantMessage();
        }

        // 루프 시작
        while (!cancellationToken.IsCancellationRequested)
        {
            // 현재 루프에서 생성된 컨텐츠를 저장할 스택
            var stack = new AssistantContentCollection();

            // 메시지 생성
            await foreach (var res in conn.GenerateStreamingMessageAsync(request, cancellationToken))
            {
                // 생성 종료 이유
                if (res.EndReason != null)
                {
                    reason = res.EndReason;
                }

                // 토큰 사용량 업데이트
                if (res.TokenUsage != null)
                {
                    if (usage != null)
                    {
                        // 두번째 이후
                        // 처음 입력토큰은 그대로 두고, 이후에 생성된 토큰만 추가
                        usage.OutputTokens += res.TokenUsage.OutputTokens;
                    }
                    else
                    {
                        // 처음 첫번째 루프에서 토큰 사용량을 설정
                        usage = res.TokenUsage;
                    }
                }

                // 컨텐츠 추가
                if (res.Data != null)
                {
                    // 텍스트 컨텐츠의 경우
                    if (res.Data is AssistantTextContent text)
                    {
                        var last = stack.LastOrDefault();
                        var value = text.Value;

                        // 마지막 컨텐츠가 텍스트인 경우
                        // 텍스트를 이어 붙입니다.
                        if (last is AssistantTextContent lastText)
                        {
                            lastText.Value ??= string.Empty;
                            lastText.Value += value;
                        }
                        // 마지막 컨텐츠가 텍스트가 아닌 경우
                        // 새로운 텍스트 컨텐츠를 추가합니다.
                        else
                        {
                            stack.Add(text);
                            last = stack.Last();
                        }

                        yield return new ChatCompletionResponse<IAssistantContent>
                        {
                            Data = new AssistantTextContent
                            {
                                // 실제 인덱스 계산 (축적된 컨텐츠 + 현재 인덱스)
                                Index = message.Content.Count + last.Index, 
                                Value = value,
                            }
                        };
                    }
                    // 툴 컨텐츠의 경우
                    else if (res.Data is AssistantToolContent tool)
                    {
                        // 현재루프의 컨텐츠에서 해당하는 인덱스에 툴이 있는지 확인
                        var exist = stack.ElementAtOrDefault(tool.Index ?? 0);

                        // 있다면 스트리밍으로 나오는 Arguments를 이어 붙입니다.
                        if (exist is AssistantToolContent existTool)
                        {
                            existTool.Arguments ??= string.Empty;
                            existTool.Arguments += tool.Arguments;
                        }
                        // 없다면 새로운 툴 컨텐츠를 추가합니다.
                        else
                        {
                            stack.Add(tool);
                            yield return new ChatCompletionResponse<IAssistantContent>
                            {
                                Data = new AssistantToolContent
                                {
                                    // 실제 인덱스 계산 (축적된 컨텐츠 + 현재 인덱스)
                                    Index = message.Content.Count + tool.Index,
                                    // 도구는 대기 상태로 시작합니다.
                                    Status = ToolStatus.Pending,
                                    // 호출한 도구 이름과 Provider에서 제공한 ID
                                    Id = tool.Id,
                                    Name = tool.Name,
                                }
                            };  
                        }
                    }
                    // 이외의 컨텐츠는 지원하지 않습니다.
                    else
                    {
                        throw new InvalidOperationException("Unexpected content type.");
                    }
                }
            }

            // 어시스턴트 메시지에 컨텐츠를 추가(현재 루프에서 생성된 컨텐츠를 축적)
            if (stack.Count > 0)
                message.Content.AddRange(stack);

            // 어시스턴트 메시지 추가 (마지막 메시지가 아닌 경우 == 첫번째 루프였을 경우)
            if (request.Messages.LastOrDefault() is not AssistantMessage)
                request.Messages.Add(message);

            // 도구 호출
            if (reason == EndReason.ToolCall)
            {
                bool approveRequired = false;
                var toolContentGroup = message.Content.OfType<AssistantToolContent>();
                foreach (var content in toolContentGroup)
                {
                    if (string.IsNullOrEmpty(content.Name) || content.Result != null)
                    {
                        // 도구 이름이 없거나 결과가 이미 있는 경우 건너뜀
                        continue;
                    }
                    if (request.Tools.TryGetValue(content.Name, out var tool))
                    {
                        if (tool.Permission == ToolPermission.Manual)
                        {
                            // 도구 사용이 요구되는 경우
                            approveRequired = true;
                        }
                        else
                        {
                            // 도구 호출
                            content.Status = ToolStatus.Running;
                            yield return new ChatCompletionResponse<IAssistantContent>
                            {
                                Data = content
                            };

                            var result = await tool.InvokeAsync(content.Arguments, cancellationToken);
                            var data = JsonSerializer.Serialize(result.Data, JsonDefaultOptions.Options);

                            if (!result.IsSuccess)
                                content.Failed(data);
                            else if (data.Length > 30_000)
                                content.TooMuchResult();
                            else
                                content.Completed(data);

                            yield return new ChatCompletionResponse<IAssistantContent>
                            {
                                Data = content
                            };
                        }
                    }
                    else
                    {
                        content.NotFoundTool();
                        yield return new ChatCompletionResponse<IAssistantContent> 
                        { 
                            Data = content 
                        };
                    }
                }

                if (approveRequired)
                {
                    // 도구 사용을 확인하지 않은 경우
                    // 승인 요청을 위해 루프를 종료합니다.
                    break;
                }
            }
            // 이외 루프 종료
            else
            {
                break;
            }
        }

        // 마지막 메시지를 줍니다.
        yield return new ChatCompletionResponse<IAssistantContent>
        {
            EndReason = EndReason.EndTurn,
            TokenUsage = usage,
            Timestamp = DateTime.UtcNow
        };
    }

    // 채팅 완성 요청을 생성합니다.
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
}

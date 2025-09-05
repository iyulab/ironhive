using System.Runtime.CompilerServices;
using IronHive.Abstractions.Message;
using IronHive.Providers.OpenAI.ChatCompletion;
using MessageGenerationRequest = IronHive.Abstractions.Messages.MessageGenerationRequest;
using TextMessageContent = IronHive.Abstractions.Messages.Content.TextMessageContent;
using AssistantMessage = IronHive.Abstractions.Messages.Roles.AssistantMessage;
using MessageContent = IronHive.Abstractions.Messages.MessageContent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;

namespace IronHive.Providers.OpenAI;

public class OpenAIMessageGenerator : IMessageGenerator
{
    private readonly OpenAIChatCompletionClient _client;

    public OpenAIMessageGenerator(OpenAIConfig config)
    {
        _client = new OpenAIChatCompletionClient(config);
    }

    public OpenAIMessageGenerator(string apiKey)
    {
        _client = new OpenAIChatCompletionClient(apiKey);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var req = request.ToOpenAI();
        var res = await _client.PostChatCompletionAsync(req, cancellationToken);
        var choice = res.Choices?.FirstOrDefault();
        var content = new List<MessageContent>();

        // 텍스트 생성
        var text = choice?.Message?.Content;
        if (!string.IsNullOrWhiteSpace(text))
        {
            content.Add(new TextMessageContent
            {
                Value = text
            });
        }

        // 툴 호출
        var tools = choice?.Message?.ToolCalls;
        if (tools != null && tools.Count > 0)
        {
            foreach (var tool in tools)
            {
                var tc = new ToolMessageContent
                { 
                    IsApproved = true,
                    Id = tool.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                    Name = string.Empty,
                };
                if (tool is OpenAIFunctionToolCall func)
                {
                    tc.IsApproved = !request.Tools!.TryGet(func.Function?.Name!, out var t) || !t.RequiresApproval;
                    tc.Name = func.Function?.Name ?? string.Empty;
                    tc.Input = func.Function?.Arguments ?? string.Empty;
                }
                else if (tool is OpenAICustomToolCall custom)
                {
                    tc.IsApproved = !request.Tools!.TryGet(custom.Custom?.Name!, out var t) || !t.RequiresApproval;
                    tc.Name = custom.Custom?.Name ?? string.Empty;
                    tc.Input = custom.Custom?.Input ?? string.Empty;
                }
                else
                {
                    throw new NotImplementedException("Unsupported tool call type.");
                }
                content.Add(tc);
            }
        }

        return new MessageResponse
        {
            DoneReason = choice?.FinishReason switch
            {
                ChatFinishReason.ToolCalls => MessageDoneReason.ToolCall,
                ChatFinishReason.Stop => MessageDoneReason.EndTurn,
                ChatFinishReason.Length => MessageDoneReason.MaxTokens,
                ChatFinishReason.ContentFilter => MessageDoneReason.ContentFilter,
                _ => null
            },
            Message = new AssistantMessage
            {
                Id = res.Id ?? Guid.NewGuid().ToString(),
                Name = null,
                Model = res.Model,
                Content = content,
            },
            TokenUsage = new MessageTokenUsage
            {
                InputTokens = res.Usage?.PromptTokens,
                OutputTokens = res.Usage?.CompletionTokens
            }
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = request.ToOpenAI();

        // 인덱스 추적 관리용
        (int, MessageContent)? current = null;
        int? currentToolIndex = null;

        string id = string.Empty;
        string model = string.Empty;
        MessageDoneReason? reason = null;
        MessageTokenUsage? usage = null;
        await foreach (var res in _client.PostStreamingChatCompletionAsync(req, cancellationToken))
        {
            // 메시지 시작
            if (current == null)
            {
                id = res.Id ?? Guid.NewGuid().ToString();
                model = res.Model ?? req.Model;
                yield return new StreamingMessageBeginResponse
                {
                    Id = id
                };
            }

            // 토큰 사용량(FinishReason 다음 호출)
            if (res.Usage != null)
            {
                usage = new MessageTokenUsage
                {
                    InputTokens = res.Usage.PromptTokens,
                    OutputTokens = res.Usage.CompletionTokens
                };
            }

            // 메시지 확인 및 건너뛰기
            var choice = res.Choices?.FirstOrDefault();
            if (choice == null)
                continue;

            // 종료 메시지
            if (choice.FinishReason != null)
            {
                reason = choice.FinishReason switch
                {
                    ChatFinishReason.ToolCalls => MessageDoneReason.ToolCall,
                    ChatFinishReason.Stop => MessageDoneReason.EndTurn,
                    ChatFinishReason.Length => MessageDoneReason.MaxTokens,
                    ChatFinishReason.ContentFilter => MessageDoneReason.ContentFilter,
                    _ => null
                };
            }

            // 메시지 확인 및 건너뛰기
            var delta = choice.Delta;
            if (delta == null)
                continue;

            // 툴 사용
            var tools = delta.ToolCalls;
            if (tools != null)
            {
                for (var i = 0; i < tools.Count; i++)
                {
                    var func = tools.ElementAt(i);
                    var funcIndex = func.Index ?? i;

                    // 이어서 생성되는 툴 메시지의 경우
                    if (current.HasValue)
                    {
                        var (index, content) = current.Value;
                        if (content is ToolMessageContent toolContent)
                        {
                            // 현재 컨텐츠가 ToolMessageContent이고, 인덱스가 동일한 경우 업데이트
                            if (funcIndex == currentToolIndex)
                            {
                                yield return new StreamingContentDeltaResponse
                                {
                                    Index = index,
                                    Delta = new ToolDeltaContent
                                    { 
                                        Input = func.Function?.Arguments ?? string.Empty 
                                    }
                                };
                            }
                            // 현재 컨텐츠가 ToolMessageContent이지만, 인덱스가 다른 경우
                            // 이전 컨텐츠 종료 및 새 컨텐츠 시작
                            else
                            {
                                yield return new StreamingContentCompletedResponse
                                {
                                    Index = index,
                                };
                                current = (index + 1, new ToolMessageContent
                                {
                                    IsApproved = !request.Tools!.TryGet(func.Function?.Name!, out var t) || !t.RequiresApproval,
                                    Id = func.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                                    Name = func.Function?.Name ?? string.Empty,
                                    Input = func.Function?.Arguments,
                                });
                                currentToolIndex = funcIndex;
                                yield return new StreamingContentAddedResponse
                                {
                                    Index = current.Value.Item1,
                                    Content = current.Value.Item2
                                };
                            }
                        }
                        // 현재 컨텐츠가 ToolMessageContent가 아닌 경우
                        // 이전 컨텐츠 종료 및 새 컨텐츠 시작
                        else
                        {
                            yield return new StreamingContentCompletedResponse
                            {
                                Index = index,
                            };
                            current = (index + 1, new ToolMessageContent
                            {
                                IsApproved = !request.Tools!.TryGet(func.Function?.Name!, out var t) || !t.RequiresApproval,
                                Id = func.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                                Name = func.Function?.Name ?? string.Empty,
                                Input = func.Function?.Arguments,
                            });
                            currentToolIndex = funcIndex;
                            yield return new StreamingContentAddedResponse
                            {
                                Index = current.Value.Item1,
                                Content = current.Value.Item2
                            };
                        }
                    }
                    // 처음 생성되는 툴 메시지의 경우
                    else
                    {
                        current = (0, new ToolMessageContent
                        {
                            IsApproved = !request.Tools!.TryGet(func.Function?.Name!, out var t) || !t.RequiresApproval,
                            Id = func.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                            Name = func.Function?.Name ?? string.Empty,
                            Input = func.Function?.Arguments,
                        });
                        currentToolIndex = funcIndex;
                        yield return new StreamingContentAddedResponse
                        {
                            Index = current.Value.Item1,
                            Content = current.Value.Item2
                        };
                    }
                }
            }

            // 텍스트 생성
            var text = delta.Content;
            if (text != null)
            {
                // 이어서 생성되는 텍스트 메시지의 경우
                if (current.HasValue)
                {
                    var (index, content) = current.Value;
                    // 현재 컨텐츠가 TextMessageContent인 경우
                    if (content is TextMessageContent textContent)
                    {
                        yield return new StreamingContentDeltaResponse
                        {
                            Index = index,
                            Delta = new TextDeltaContent
                            {
                                Value = text
                            }
                        };
                    }
                    // 현재 컨텐츠가 TextMessageContent가 아닌 경우
                    // 이전 컨텐츠 종료 및 새 컨텐츠 시작
                    else
                    {
                        yield return new StreamingContentCompletedResponse
                        {
                            Index = index,
                        };
                        current = (index + 1, new TextMessageContent { Value = text });
                        yield return new StreamingContentAddedResponse
                        {
                            Index = current.Value.Item1,
                            Content = current.Value.Item2
                        };
                    }
                }
                // 처음 생성되는 텍스트 메시지의 경우
                else
                {
                    current = (0, new TextMessageContent { Value = text });
                    yield return new StreamingContentAddedResponse
                    {
                        Index = current.Value.Item1,
                        Content = current.Value.Item2
                    };
                }
            }
        }

        // 남아 있는 컨텐츠 처리
        if (current.HasValue)
        {
            yield return new StreamingContentCompletedResponse
            {
                Index = current.Value.Item1,
            };
            //current = null;
        }

        // 종료
        yield return new StreamingMessageDoneResponse
        {
            DoneReason = reason,
            TokenUsage = usage,
            Id = id,
            Model = model,
            Timestamp = DateTime.UtcNow,
        };
    }
}

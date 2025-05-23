using System.Text.Json;
using System.Runtime.CompilerServices;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Connectors.Anthropic.Clients;
using IronHive.Connectors.Anthropic.ChatCompletion;

namespace IronHive.Connectors.Anthropic;

public class AnthropicChatCompletionConnector : IChatCompletionConnector
{
    private readonly AnthropicChatCompletionClient _client;

    public AnthropicChatCompletionConnector(AnthropicConfig config)
    {
        _client = new AnthropicChatCompletionClient(config);
    }

    public AnthropicChatCompletionConnector(string apiKey)
    {
        _client = new AnthropicChatCompletionClient(apiKey);
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResponse<AssistantMessage>> GenerateMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var req = ConvertRequest(request);
        var res = await _client.PostMessagesAsync(req, cancellationToken);

        var message = new AssistantMessage();
        foreach (var item in res.Content ?? [])
        {
            // 추론 생성
            if (item is ThinkingMessageContent thinking)
            {
                message.Content.Add(new AssistantThinkingContent
                {
                    Mode = ThinkingMode.Detailed,
                    Id = thinking.Signature,
                    Value = thinking.Thinking,
                });
            }
            // 보안 추론 생성
            else if (item is RedactedThinkingMessageContent redacted)
            {
                message.Content.Add(new AssistantThinkingContent
                {
                    Mode = ThinkingMode.Secure,
                    Id = null,
                    Value = redacted.Data,
                });
            }
            // 텍스트 생성
            else if (item is TextMessageContent text)
            {
                message.Content.Add(new AssistantTextContent
                {
                    Value = text.Text
                });
            }
            // 툴 사용
            else if (item is ToolUseMessageContent tool)
            {
                message.Content.Add(new AssistantToolContent
                {
                    Id = tool.Id,
                    Name = tool.Name,
                    Arguments = JsonSerializer.Serialize(tool.Input)
                });
            }
            else
            {
                throw new NotSupportedException($"Unexpected content type [{item.GetType()}] not supported yet");
            }
        }

        return new ChatCompletionResponse<AssistantMessage>
        {
            EndReason = res.StopReason switch
            {
                StopReason.ToolUse => EndReason.ToolCall,
                StopReason.EndTurn => EndReason.EndTurn,
                StopReason.MaxTokens => EndReason.MaxTokens,
                StopReason.StopSequence => EndReason.StopSequence,
                _ => null
            },
            TokenUsage = new TokenUsage
            {
                InputTokens = res.Usage.InputTokens,
                OutputTokens = res.Usage.OutputTokens
            },
            Data = message
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionResponse<IAssistantContent>> GenerateStreamingMessageAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = ConvertRequest(request);

        int index = 0;
        EndReason? reason = null;
        TokenUsage usage = new TokenUsage();
        await foreach (var res in _client.PostStreamingMessagesAsync(req, cancellationToken))
        {
            // 1. 메시지 시작 이벤트
            if (res is MessageStartEvent mse)
            {
                usage.InputTokens = mse.Message?.Usage.InputTokens;
            }
            // 2. 컨텐츠 생성 시작 이벤트
            else if (res is ContentStartEvent cse)
            {
                // 추론 생성
                if (cse.ContentBlock is ThinkingMessageContent thinking)
                {
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantThinkingContent
                        {
                            Mode = ThinkingMode.Detailed,
                            Index = cse.Index ?? index,
                            Value = thinking.Thinking
                        }
                    };
                }
                // 보안 추론 생성
                else if (cse.ContentBlock is RedactedThinkingMessageContent redacted)
                {
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantThinkingContent
                        {
                            Mode = ThinkingMode.Secure,
                            Index = cse.Index ?? index,
                            Value = redacted.Data
                        }
                    };
                }
                // 텍스트 생성
                else if (cse.ContentBlock is TextMessageContent text)
                {
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantTextContent
                        {
                            Index = cse.Index ?? index,
                            Value = text.Text
                        }
                    };
                }
                // 툴 사용
                else if (cse.ContentBlock is ToolUseMessageContent tool)
                {
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantToolContent
                        {
                            Index = cse.Index ?? index,
                            Id = tool.Id,
                            Name = tool.Name
                        }
                    };
                }
            }
            // 3. 컨텐츠 생성 이벤트
            else if (res is ContentDeltaEvent cde)
            {
                // 추론 생성
                if (cde.Delta is ThinkingDeltaMessageContent thinking)
                {
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantThinkingContent
                        {
                            Index = cde.Index ?? index,
                            Value = thinking.Thinking
                        }
                    };
                }
                // 추론 ID 전달
                else if (cde.Delta is SignatureDeltaMessageContent signature)
                {
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantThinkingContent
                        {
                            Index = cde.Index ?? index,
                            Id = signature.Signature
                        }
                    };
                }
                // 텍스트 생성
                else if (cde.Delta is TextDeltaMessageContent text)
                {
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantTextContent
                        {
                            Index = cde.Index ?? index,
                            Value = text.Text
                        }
                    };
                }
                // 툴 사용
                else if (cde.Delta is ToolUseDeltaMessageContent tool)
                {
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantToolContent
                        {
                            Index = cde.Index ?? index,
                            Arguments = tool.PartialJson,
                        }
                    };
                }
            }
            // 4. 컨텐츠 생성 종료 이벤트
            else if (res is ContentStopEvent)
            {
                index++;
            }
            // 5. 메시지 메타 데이터 이벤트
            else if (res is MessageDeltaEvent mde)
            {
                reason = mde.Delta?.StopReason switch
                {
                    StopReason.ToolUse => EndReason.ToolCall,
                    StopReason.EndTurn => EndReason.EndTurn,
                    StopReason.StopSequence => EndReason.StopSequence,
                    StopReason.MaxTokens => EndReason.MaxTokens,
                    _ => null
                };
                usage.OutputTokens = mde.Usage?.OutputTokens;
            }
            // 6. 메시지 종료 이벤트
            else if (res is MessageStopEvent)
            {
                continue;
            }
            // 핑 이벤트
            else if (res is PingEvent)
            {
                continue;
            }
            // 에러 이벤트
            else if (res is ErrorEvent error)
            {
                throw new Exception(JsonSerializer.Serialize(error.Error));
            }
            // 알수 없는 이벤트
            else
            {
                throw new NotSupportedException($"Unexpected event type: {res.GetType()}");
            }
        }

        yield return new ChatCompletionResponse<IAssistantContent>
        {
            EndReason = reason,
            TokenUsage = usage
        };
    }

    private static MessagesRequest ConvertRequest(ChatCompletionRequest request)
    {
        // 모델에 따라 추론 사용 여부 결정
        var isThinking = IsThinkingModel(request.Model);

        var _req = new MessagesRequest
        {
            Model = request.Model,
            System = request.System,
            Messages = request.Messages.ToAnthropic(isThinking),
            // MaxToken이 필수요청사항으로 "8192"값을 기본으로 함
            MaxTokens = request.MaxTokens ?? (isThinking ? 64_000 : 8_192),
            Temperature = request.Temperature,
            TopK = request.TopK,
            TopP = request.TopP,
            StopSequences = request.StopSequences,
            // 추론 모델일 경우 기본 사용
            Thinking = isThinking
            ? new EnabledThinking { BudgetTokens = 32_000 }
            : null,
            Tools = request.Tools.Select(t => new CustomTool
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = t.Parameters ?? new ObjectJsonSchema()
            })
        };

        return _req;
    }

    private static bool IsThinkingModel(string model)
    {
        return model.StartsWith("claude-3-7-sonnet", StringComparison.OrdinalIgnoreCase)
            || model.StartsWith("claude-sonnet-4", StringComparison.OrdinalIgnoreCase)
            || model.StartsWith("claude-opus-4", StringComparison.OrdinalIgnoreCase);
    }
}

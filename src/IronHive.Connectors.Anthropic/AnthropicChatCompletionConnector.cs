using System.Text.Json;
using System.Runtime.CompilerServices;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Messages;
using IronHive.Connectors.Anthropic.ChatCompletion;
using IronHive.Connectors.Anthropic.Clients;
using TokenUsage = IronHive.Abstractions.ChatCompletion.TokenUsage;

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
            // 텍스트 생성
            if (item is TextMessageContent text)
            {
                message.Content.AddText(text.Text);
            }
            // 툴 사용
            else if (item is ToolUseMessageContent tool)
            {
                message.Content.AddTool(tool.Id, tool.Name, JsonSerializer.Serialize(tool.Input), null);
            }
            // 추론 사용
            else if (item is ThinkingMessageContent thinking)
            {
                message.Content.AddThinking(thinking.Signature, thinking.Thinking);
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

        EndReason? reason = null;
        TokenUsage usage = new TokenUsage();
        await foreach (var res in _client.PostStreamingMessagesAsync(req, cancellationToken))
        {
            if (res is MessageStartEvent mse)
            {
                // 1. 메시지 시작 이벤트
                usage.InputTokens = mse.Message?.Usage.InputTokens;
            }
            else if (res is ContentStartEvent cse)
            {
                // 2-1. 컨텐츠 블록 시작 이벤트

                // 텍스트 생성
                if (cse.ContentBlock is TextMessageContent text)
                {
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantTextContent
                        {
                            Index = cse.Index,
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
                            Index = cse.Index,
                            Id = tool.Id,
                            Name = tool.Name
                        }
                    };
                }
                // 추론 사용
                else if (cse.ContentBlock is ThinkingMessageContent thinking)
                {
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantThinkingContent
                        {
                            Index = cse.Index,
                            Id = thinking.Signature,
                            Value = thinking.Thinking
                        }
                    };
                }
            }
            else if (res is ContentDeltaEvent cde)
            {
                // 2-2. 컨텐츠 블록 생성 진행 이벤트

                // 텍스트 생성
                if (cde.Delta is TextDeltaMessageContent text)
                {
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantTextContent
                        {
                            Index = cde.Index,
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
                            Index = cde.Index,
                            Arguments = tool.PartialJson,
                        }
                    };
                }
                // 추론 사용
                else if (cde.Delta is ThinkingDeltaMessageContent thinking)
                {
                    yield return new ChatCompletionResponse<IAssistantContent>
                    {
                        Data = new AssistantThinkingContent
                        {
                            Index = cde.Index,
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
                            Index = cde.Index,
                            Id = signature.Signature
                        }
                    };
                }
            }
            else if (res is ContentStopEvent)
            {
                // 2-3. 컨텐츠 블록 생성 종료 이벤트
            }
            else if (res is MessageDeltaEvent mde)
            {
                // 3. 메시지 정보 이벤트
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
            else if (res is MessageStopEvent)
            {
                // 4. 메시지 종료 이벤트
            }
            else if (res is PingEvent)
            {
                // 핑 이벤트
            }
            else if (res is ErrorEvent error)
            {
                // 에러 이벤트
                throw new Exception(JsonSerializer.Serialize(error.Error));
            }
            else
            {
                // Unknown
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
        var _req = new MessagesRequest
        {
            Model = request.Model,
            System = request.System,
            Messages = request.Messages.ToAnthropic(),
            // MaxToken이 필수요청사항으로 "8192"값을 기본으로 함
            MaxTokens = request.MaxTokens ?? (IsThinkingModel(request.Model) ? 64_000 :  8_192),
            Temperature = request.Temperature,
            TopK = request.TopK,
            TopP = request.TopP,
            StopSequences = request.StopSequences,
            // 추론 모델일 경우 기본 사용
            Thinking = IsThinkingModel(request.Model) 
            ? new EnabledThinking { BudgetTokens = 32_000 }
            : null,
        };

        _req.Tools = request.Tools.Select(t => new Tool
        {
            Name = t.Name,
            Description = t.Description,
            InputSchema = t.InputSchema ?? new ObjectJsonSchema()
        });

        return _req;
    }

    private static bool IsThinkingModel(string model)
    {
        return model.StartsWith("claude-3-7-sonnet", StringComparison.OrdinalIgnoreCase);
    }
}

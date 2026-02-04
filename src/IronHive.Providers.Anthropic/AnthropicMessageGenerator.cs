using System.Text.Json;
using System.Runtime.CompilerServices;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using MessageContent = IronHive.Abstractions.Messages.MessageContent;
using TextMessageContent = IronHive.Abstractions.Messages.Content.TextMessageContent;
using AnthropicTextMessageContent = IronHive.Providers.Anthropic.Payloads.Messages.TextMessageContent;
using ThinkingMessageContent = IronHive.Abstractions.Messages.Content.ThinkingMessageContent;
using AnthropicThinkingMessageContent = IronHive.Providers.Anthropic.Payloads.Messages.ThinkingMessageContent;
using IronHive.Abstractions.Messages;
using IronHive.Providers.Anthropic.Payloads.Messages;
using IronHive.Providers.Anthropic.Clients;

namespace IronHive.Providers.Anthropic;

/// <inheritdoc />
public class AnthropicMessageGenerator : IMessageGenerator
{
    private readonly AnthropicMessagesClient _client;

    public AnthropicMessageGenerator(string apiKey)
        : this(new AnthropicConfig { ApiKey = apiKey })
    { }

    public AnthropicMessageGenerator(AnthropicConfig config)
    {
        _client = new AnthropicMessagesClient(config);
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
        var req = request.ToAnthropic();
        var res = await _client.PostMessagesAsync(req, cancellationToken);

        var content = new List<MessageContent>();
        foreach (var item in res.Content ?? [])
        {
            // 추론 생성
            if (item is AnthropicThinkingMessageContent thinking)
            {
                content.Add(new ThinkingMessageContent
                {
                    Format = ThinkingFormat.Detailed,
                    Signature = thinking.Signature,
                    Value = thinking.Thinking,
                });
            }
            // 보안 추론 생성
            else if (item is RedactedThinkingMessageContent redacted)
            {
                content.Add(new ThinkingMessageContent
                {
                    Format = ThinkingFormat.Secure,
                    Signature = Guid.NewGuid().ToShort(), // 보안 추론은 ID가 없으므로 임의로 생성
                    Value = redacted.Data,
                });
            }
            // 텍스트 생성
            else if (item is AnthropicTextMessageContent text)
            {
                content.Add(new TextMessageContent
                {
                    Value = text.Text
                });
            }
            // 툴 사용
            else if (item is ToolUseMessageContent tool)
            {
                content.Add(new ToolMessageContent
                {
                    IsApproved = request.Tools?.TryGet(tool.Name!, out var t) != true || !t.RequiresApproval,
                    Id = tool.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                    Name = tool.Name ?? string.Empty,
                    Input = JsonSerializer.Serialize(tool.Input)
                });
            }
            else
            {
                throw new NotSupportedException($"Unexpected content type [{item.GetType()}] not supported yet");
            }
        }

        return new MessageResponse
        {
            Id = res.Id,
            DoneReason = res.StopReason switch
            {
                StopReason.ToolUse => MessageDoneReason.ToolCall,
                StopReason.EndTurn => MessageDoneReason.EndTurn,
                StopReason.MaxTokens => MessageDoneReason.MaxTokens,
                StopReason.StopSequence => MessageDoneReason.StopSequence,
                _ => null
            },
            Message = new AssistantMessage
            {
                Id = res.Id,
                Model = res.Model,
                Content = content,
            },
            TokenUsage = new MessageTokenUsage
            {
                InputTokens = res.Usage.InputTokens,
                OutputTokens = res.Usage.OutputTokens
            },
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = request.ToAnthropic();

        var id = string.Empty;
        var model = string.Empty;
        int index = 0;
        var usage = new MessageTokenUsage();
        await foreach (var res in _client.PostStreamingMessagesAsync(req, cancellationToken))
        {
            // 1. 메시지 시작 이벤트
            if (res is MessageStartEvent mse)
            {
                id = mse.Message.Id;
                model = mse.Message.Model;
                usage.InputTokens = mse.Message.Usage.InputTokens;
                yield return new StreamingMessageBeginResponse
                {
                    Id = id
                };
            }
            // 2. 컨텐츠 생성 시작 이벤트
            else if (res is ContentStartEvent cse)
            {
                // 추론 생성
                if (cse.ContentBlock is AnthropicThinkingMessageContent thinking)
                {
                    yield return new StreamingContentAddedResponse
                    {
                        Index = index,
                        Content = new ThinkingMessageContent
                        {
                            Signature = thinking.Signature,
                            Format = ThinkingFormat.Detailed,
                            Value = thinking.Thinking
                        }
                    };
                }
                // 보안 추론 생성
                else if (cse.ContentBlock is RedactedThinkingMessageContent redacted)
                {
                    yield return new StreamingContentAddedResponse
                    {
                        Index = index,
                        Content = new ThinkingMessageContent
                        {
                            Format = ThinkingFormat.Secure,
                            Value = redacted.Data
                        }
                    };
                }
                // 텍스트 생성
                else if (cse.ContentBlock is AnthropicTextMessageContent text)
                {
                    yield return new StreamingContentAddedResponse
                    {
                        Index = index,
                        Content = new TextMessageContent
                        {
                            Value = text.Text,
                        }
                    };
                }
                // 툴 사용
                else if (cse.ContentBlock is ToolUseMessageContent tool)
                {
                    yield return new StreamingContentAddedResponse
                    {
                        Index = index,
                        Content = new ToolMessageContent
                        {
                            IsApproved = request.Tools?.TryGet(tool.Name!, out var t) != true || !t.RequiresApproval,
                            Id = tool.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                            Name = tool.Name ?? string.Empty,
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
                    yield return new StreamingContentDeltaResponse
                    {
                        Index = index,
                        Delta = new ThinkingDeltaContent
                        {
                            Data = thinking.Thinking,
                        },
                    };
                }
                // 추론 ID 전달
                else if (cde.Delta is SignatureDeltaMessageContent signature)
                {
                    yield return new StreamingContentUpdatedResponse
                    {
                        Index = index,
                        Updated = new ThinkingUpdatedContent
                        {
                            Signature = signature.Signature,
                        }
                    };
                }
                // 텍스트 생성
                else if (cde.Delta is TextDeltaMessageContent text)
                {
                    yield return new StreamingContentDeltaResponse
                    {
                        Index = index,
                        Delta = new TextDeltaContent
                        {
                            Value = text.Text,
                        }
                    };
                }
                // 툴 사용
                else if (cde.Delta is ToolUseDeltaMessageContent tool)
                {
                    yield return new StreamingContentDeltaResponse
                    {
                        Index = index,
                        Delta = new ToolDeltaContent
                        {
                            Input = tool.PartialJson
                        }
                    };
                }
            }
            // 4. 컨텐츠 생성 종료 이벤트
            else if (res is ContentStopEvent)
            {
                yield return new StreamingContentCompletedResponse
                {
                    Index = index
                };
                index++;
            }
            // 5. 메시지 메타 데이터 이벤트
            else if (res is MessageDeltaEvent mde)
            {
                usage.OutputTokens = mde.Usage?.OutputTokens ?? usage.OutputTokens;

                yield return new StreamingMessageDoneResponse
                {
                    Id = id,
                    Model = model,
                    DoneReason = mde.Delta.StopReason switch
                    {
                        StopReason.ToolUse => MessageDoneReason.ToolCall,
                        StopReason.EndTurn => MessageDoneReason.EndTurn,
                        StopReason.StopSequence => MessageDoneReason.StopSequence,
                        StopReason.MaxTokens => MessageDoneReason.MaxTokens,
                        _ => null
                    },
                    TokenUsage = usage,
                    Timestamp = DateTime.UtcNow
                };
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
                var json = error.Error.ConvertTo<JsonElement>();
                var message = json.GetProperty("message").GetString() ?? "Unknown error";
                throw new Exception(message);
            }
            // 알수 없는 이벤트
            else
            {
                throw new NotSupportedException($"Unexpected event type: {res.GetType()}");
            }
        }
    }
}

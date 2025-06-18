using System.Text.Json;
using System.Runtime.CompilerServices;
using IronHive.Abstractions.Message;
using IronHive.Abstractions.Message.Content;
using IronHive.Abstractions.Message.Roles;
using IronHive.Providers.Anthropic.Messages;
using TextMessageContent = IronHive.Abstractions.Message.Content.TextMessageContent;
using AnthropicTextMessageContent = IronHive.Providers.Anthropic.Messages.TextMessageContent;
using ThinkingMessageContent = IronHive.Abstractions.Message.Content.ThinkingMessageContent;
using AnthropicThinkingMessageContent = IronHive.Providers.Anthropic.Messages.ThinkingMessageContent;

namespace IronHive.Providers.Anthropic;

public class AnthropicMessageGenerationProvider : IMessageGenerationProvider
{
    private readonly AnthropicMessagesClient _client;

    public AnthropicMessageGenerationProvider(AnthropicConfig config)
    {
        _client = new AnthropicMessagesClient(config);
    }

    public AnthropicMessageGenerationProvider(string apiKey)
    {
        _client = new AnthropicMessagesClient(apiKey);
    }

    /// <inheritdoc />
    public required string ProviderName { get; init; }

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
                    Id = thinking.Signature,
                    Value = thinking.Thinking,
                });
            }
            // 보안 추론 생성
            else if (item is RedactedThinkingMessageContent redacted)
            {
                content.Add(new ThinkingMessageContent
                {
                    Format = ThinkingFormat.Secure,
                    Id = Guid.NewGuid().ToShort(), // 보안 추론은 ID가 없으므로 임의로 생성
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
                Timestamp = DateTime.UtcNow
            },
            TokenUsage = new MessageTokenUsage
            {
                InputTokens = res.Usage.InputTokens,
                OutputTokens = res.Usage.OutputTokens
            }
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = request.ToAnthropic();

        #region 인덱스 수동 관리
        // ┌───────────────────────────────────────
        // │ xAI Messages API: index 직접 관리
        // └───────────────────────────────────────
        // xAI의 Messages API에서 index 응답을 받지 못하므로
        // index를 직접 관리합니다.
        int index = 0;
        #endregion

        var id = string.Empty;
        var model = string.Empty;
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
                            Id = thinking.Signature,
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
                            Id = signature.Signature,
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
                usage.OutputTokens = mde.Usage?.OutputTokens;

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
                throw new Exception(error.Error.Message);
            }
            // 알수 없는 이벤트
            else
            {
                throw new NotSupportedException($"Unexpected event type: {res.GetType()}");
            }
        }
    }
}

using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Providers.GoogleAI.Clients;
using IronHive.Providers.GoogleAI.Payloads.GenerateContent;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IronHive.Providers.GoogleAI;

/// <inheritdoc />
public class GoogleAIMessageGenerator : IMessageGenerator
{
    private readonly GoogleAIGenerateContentClient _client;

    public GoogleAIMessageGenerator(string apiKey)
        : this(new GoogleAIConfig { ApiKey = apiKey })
    { }

    public GoogleAIMessageGenerator(GoogleAIConfig config)
    {
        _client = new GoogleAIGenerateContentClient(config);
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
        var req = request.ToGoogleAI();
        var response = await _client.GenerateContentAsync(req, cancellationToken);

        MessageDoneReason? reason = null;
        var usage = new MessageTokenUsage();
        var message = new AssistantMessage
        {
            Model = response.ModelVersion
        };
        var first = response.Candidates?.FirstOrDefault()
            ?? throw new InvalidOperationException("No candidates in response.");

        // 응답 메시지 구성
        foreach (var part in first.Content?.Parts ?? [])
        {
            if (!string.IsNullOrWhiteSpace(part.Text))
            {
                // 생각(Thought) 메시지 처리
                if (part.Thought == true)
                {
                    message.Content.Add(new ThinkingMessageContent
                    {
                        Format = ThinkingFormat.Summary,
                        Signature = part.ThoughtSignature,
                        Value = part.Text
                    });
                }
                // 일반 텍스트 메시지 처리
                else
                {
                    message.Content.Add(new TextMessageContent
                    {
                        Value = part.Text
                    });
                }
            }

            // 생각(Thought) 시그니처 업데이트
            if (!string.IsNullOrWhiteSpace(part.ThoughtSignature))
            {
                if (message.Content.LastOrDefault(c => c is ThinkingMessageContent) is ThinkingMessageContent last)
                {
                    last.Signature = part.ThoughtSignature;
                }
            }

            // 함수 호출 메시지 처리
            if (part.FunctionCall != null)
            {
                reason ??= MessageDoneReason.ToolCall;
                message.Content.Add(new ToolMessageContent
                {
                    Id = part.FunctionCall.Id ?? Guid.NewGuid().ToShort(),
                    Name = part.FunctionCall.Name,
                    Input = JsonSerializer.Serialize(part.FunctionCall.Args),
                    IsApproved = !request.Tools!.TryGet(part.FunctionCall.Name, out var t) || !t.RequiresApproval
                });
            }
            
            // 이하 생략 (blob 등)
        }

        // 완료 이유 매핑
        reason ??= ResolveReason(first.FinishReason);

        // 토큰 사용량 집계
        if (response.UsageMetadata != null)
        {
            var meta = response.UsageMetadata;
            if (meta.PromptTokenCount.HasValue)
                usage.InputTokens += meta.PromptTokenCount.Value;

            if (meta.CandidatesTokenCount.HasValue)
                usage.OutputTokens += meta.CandidatesTokenCount.Value;

            if (meta.ThoughtsTokenCount.HasValue)
                usage.OutputTokens += meta.ThoughtsTokenCount.Value;
        }

        return new MessageResponse
        {
            Id = response.ResponseId ?? Guid.NewGuid().ToShort(),
            DoneReason = reason,
            Message = message,
            TokenUsage = usage,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = request.ToGoogleAI();

        // 인덱스 추적 관리용
        (int, MessageContent)? current = null;

        string id = string.Empty;
        string model = string.Empty;
        MessageDoneReason? reason = null;
        MessageTokenUsage? usage = null;

        await foreach (var res in _client.StreamGenerateContentAsync(req, cancellationToken))
        {
            // 메시지 시작
            if (current == null)
            {
                id = res.ResponseId ?? Guid.NewGuid().ToString();
                model = res.ModelVersion ?? req.Model;
                yield return new StreamingMessageBeginResponse
                {
                    Id = id
                };
            }

            // 토큰 사용량(FinishReason 다음 호출)
            if (res.UsageMetadata != null)
            {
                usage = new MessageTokenUsage
                {
                    InputTokens = res.UsageMetadata.PromptTokenCount ?? 0,
                    OutputTokens = res.UsageMetadata.CandidatesTokenCount ?? 0
                        + res.UsageMetadata.ThoughtsTokenCount ?? 0
                };
            }

            // 메시지 확인 및 건너뛰기
            var msg = res.Candidates?.FirstOrDefault();
            if (msg == null)
                continue;

            // 종료 메시지
            if (msg.FinishReason != null)
            {
                reason = ResolveReason(msg.FinishReason);
            }

            // 메시지 컨텐츠 확인 및 건너뛰기
            var parts = msg.Content?.Parts;
            if (parts == null)
                continue;

            foreach (var part in parts)
            {
                // 첫 업데이트 인 경우
                if (!current.HasValue)
                {
                    // 생각(Thought) or 일반 텍스트 처리
                    if (!string.IsNullOrWhiteSpace(part.Text))
                    {
                        if (part.Thought == true)
                        {
                            current = (0, new ThinkingMessageContent
                            {
                                Format = ThinkingFormat.Summary,
                                Signature = part.ThoughtSignature,
                                Value = part.Text
                            });
                        }
                        else
                        {
                            current = (0, new TextMessageContent
                            {
                                Value = part.Text
                            });
                        }

                        yield return new StreamingContentAddedResponse
                        {
                            Index = current.Value.Item1,
                            Content = current.Value.Item2
                        };
                    }
                    else if (part.FunctionCall != null)
                    {
                        current = (0, new ToolMessageContent
                        {
                            Id = part.FunctionCall.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                            Name = part.FunctionCall.Name ?? string.Empty,
                            Input = JsonSerializer.Serialize(part.FunctionCall.Args),
                            IsApproved = !request.Tools!.TryGet(part.FunctionCall.Name!, out var t) || !t.RequiresApproval,
                        });
                        yield return new StreamingContentAddedResponse
                        {
                            Index = current.Value.Item1,
                            Content = current.Value.Item2
                        };
                    }
                    else
                    {
                        // 아무 것도 하지 않음
                    }
                }
                // 이전 업데이트 이후 추가시
                else
                {
                    (int index, MessageContent content) = current.Value;

                    if (!string.IsNullOrWhiteSpace(part.ThoughtSignature))
                    {
                        // 생각(Thought) 시그니처 업데이트 및 이전 생각 완료
                        if (content is ThinkingMessageContent thinkingContent)
                        {
                            yield return new StreamingContentUpdatedResponse
                            {
                                Index = index,
                                Updated = new ThinkingUpdatedContent
                                {
                                    Signature = part.ThoughtSignature
                                }
                            };
                            yield return new StreamingContentCompletedResponse
                            {
                                Index = index,
                            };
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(part.Text))
                    {
                        // 생각(Thought) 텍스트 처리
                        if (part.Thought == true)
                        {
                            // 이전 생각 완료
                            yield return new StreamingContentCompletedResponse
                            {
                                Index = index,
                            };

                            // 새 생각 추가
                            current = (index + 1, new ThinkingMessageContent
                            {
                                Format = ThinkingFormat.Summary,
                                Signature = part.ThoughtSignature,
                                Value = part.Text
                            });
                            yield return new StreamingContentAddedResponse
                            {
                                Index = current.Value.Item1,
                                Content = current.Value.Item2
                            };
                        }
                        else
                        {
                            // 동일 텍스트 블록인 경우, 델타 추가
                            if (content is TextMessageContent textContent)
                            {
                                textContent.Value += part.Text;
                                yield return new StreamingContentDeltaResponse
                                {
                                    Index = index,
                                    Delta = new TextDeltaContent
                                    {
                                        Value = part.Text
                                    }
                                };
                            }
                            // 다른 블록인 경우, 이전 블록 완료 후 새 블록 추가
                            else
                            {
                                yield return new StreamingContentCompletedResponse
                                {
                                    Index = index,
                                };
                                current = (index + 1, new TextMessageContent
                                {
                                    Value = part.Text
                                });
                                yield return new StreamingContentAddedResponse
                                {
                                    Index = current.Value.Item1,
                                    Content = current.Value.Item2
                                };
                            }
                        }
                    }
                    else if (part.FunctionCall != null)
                    {
                        // 이전 블록 완료
                        yield return new StreamingContentCompletedResponse
                        {
                            Index = index,
                        };

                        // 새 툴 호출 추가
                        current = (index + 1, new ToolMessageContent
                        {
                            Id = part.FunctionCall.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                            Name = part.FunctionCall.Name ?? string.Empty,
                            Input = JsonSerializer.Serialize(part.FunctionCall.Args),
                            IsApproved = !request.Tools!.TryGet(part.FunctionCall.Name!, out var t) || !t.RequiresApproval,
                        });
                        yield return new StreamingContentAddedResponse
                        {
                            Index = current.Value.Item1,
                            Content = current.Value.Item2
                        };
                    }
                    else
                    {
                        // 아무 것도 하지 않음
                    }
                }
            }
        }

        // 남아 있는 컨텐츠 처리
        if (current.HasValue)
        {
            // 툴 호출인 경우, 완료 이유 재설정
            if (current.Value.Item2 is ToolMessageContent)
            {
                reason = MessageDoneReason.ToolCall;
            }

            yield return new StreamingContentCompletedResponse
            {
                Index = current.Value.Item1,
            };
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

    /// <summary> Google AI의 FinishReason을 MessageDoneReason으로 매핑합니다. </summary>
    private static MessageDoneReason ResolveReason(FinishReason? reason)
    {
        return reason switch
        {
            FinishReason.STOP => MessageDoneReason.EndTurn,

            FinishReason.MAX_TOKENS => MessageDoneReason.MaxTokens,
            
            FinishReason.SAFETY or FinishReason.IMAGE_SAFETY or 
            FinishReason.PROHIBITED_CONTENT or FinishReason.SPII => MessageDoneReason.ContentFilter,
            
            _ => MessageDoneReason.Unknown
        };
    }
}
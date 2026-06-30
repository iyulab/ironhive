using Google.GenAI;
using Google.GenAI.Types;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IronHive.Providers.GoogleAI;

/// <inheritdoc />
public class GoogleAIMessageGenerator : IMessageGenerator
{
    private readonly Client _client;
    private readonly bool _isVertex;

    public GoogleAIMessageGenerator(string apiKey)
        : this(new GoogleAIConfig { ApiKey = apiKey })
    { }

    public GoogleAIMessageGenerator(GoogleAIConfig config)
    {
        _client = GoogleAIClientFactory.Create(config);
        _isVertex = false;
    }

    public GoogleAIMessageGenerator(VertexAIConfig config)
    {
        _client = GoogleAIClientFactory.Create(config);
        _isVertex = true;
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
        var (contents, config) = ToGoogleAIParams(request);
        var response = await _client.Models.GenerateContentAsync(
            request.Model, contents, config, cancellationToken);

        MessageDoneReason? reason = null;
        var usage = new MessageTokenUsage();
        var message = new Message { Role = MessageRole.Assistant };
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

            // 함수 호출 메시지 처리
            if (part.FunctionCall != null)
            {
                reason ??= MessageDoneReason.ToolCall;
                message.Content.Add(new ToolMessageContent
                {
                    Id = part.FunctionCall.Id ?? Guid.NewGuid().ToShort(),
                    Name = part.FunctionCall.Name ?? string.Empty,
                    Input = JsonSerializer.Serialize(part.FunctionCall.Args),
                    IsApproved = request.Tools?.TryGet(part.FunctionCall.Name!, out var t) != true || t?.RequiresApproval == false,
                    Signature = part.ThoughtSignature is { Length: > 0 }
                        ? Convert.ToBase64String(part.ThoughtSignature)
                        : null
                });
            }
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
            ResponseId = response.ResponseId,
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
        var (contents, config) = ToGoogleAIParams(request);

        // 인덱스 추적 관리용
        (int, MessageContent)? current = null;

        string? id = null;
        MessageDoneReason? reason = null;
        MessageTokenUsage? usage = null;

        await foreach (var res in _client.Models.GenerateContentStreamAsync(
            request.Model, contents, config, cancellationToken))
        {
            // 메시지 시작
            if (current == null)
            {
                id = res.ResponseId;
                yield return new StreamingMessageBeginResponse();
            }

            // 토큰 사용량(FinishReason 다음 호출)
            if (res.UsageMetadata != null)
            {
                usage = new MessageTokenUsage
                {
                    InputTokens = res.UsageMetadata.PromptTokenCount ?? 0,
                    OutputTokens = (res.UsageMetadata.CandidatesTokenCount ?? 0)
                        + (res.UsageMetadata.ThoughtsTokenCount ?? 0)
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
                            IsApproved = request.Tools?.TryGet(part.FunctionCall.Name!, out var t) != true || t?.RequiresApproval == false,
                            Signature = part.ThoughtSignature is { Length: > 0 }
                                ? Convert.ToBase64String(part.ThoughtSignature)
                                : null
                        });
                        yield return new StreamingContentAddedResponse
                        {
                            Index = current.Value.Item1,
                            Content = current.Value.Item2
                        };
                    }
                }
                // 이전 업데이트 이후 추가시
                else
                {
                    (int index, MessageContent content) = current.Value;

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
                            IsApproved = request.Tools?.TryGet(part.FunctionCall.Name!, out var t) != true || t?.RequiresApproval == false,
                            Signature = part.ThoughtSignature is { Length: > 0 }
                                ? Convert.ToBase64String(part.ThoughtSignature)
                                : null
                        });
                        yield return new StreamingContentAddedResponse
                        {
                            Index = current.Value.Item1,
                            Content = current.Value.Item2
                        };
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
            ResponseId = id,
            DoneReason = reason,
            TokenUsage = usage,
        };
    }

    /// <inheritdoc />
    public async Task<int> CountTokensAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var (contents, config) = ToGoogleAIParams(request);
        // Developer API는 CountTokens에서 systemInstruction/tools를 지원하지 않아
        // contents(메시지)만 카운팅 가능합니다. Vertex AI는 풀 카운팅을 지원합니다.
        var countConfig = _isVertex
            ? new CountTokensConfig { SystemInstruction = config.SystemInstruction, Tools = config.Tools }
            : null;
        var result = await _client.Models.CountTokensAsync(request.Model, contents, countConfig, cancellationToken);
        return result.TotalTokens ?? 0;
    }

    /// <summary> Google AI의 FinishReason을 MessageDoneReason으로 매핑합니다. </summary>
    private static MessageDoneReason ResolveReason(FinishReason? reason)
    {
        if (reason is null)
        {
            return MessageDoneReason.Unknown;
        }

        if (reason == FinishReason.Stop)
        {
            return MessageDoneReason.EndTurn;
        }

        if (reason == FinishReason.MaxTokens)
        {
            return MessageDoneReason.MaxTokens;
        }

        if (reason == FinishReason.Safety || reason == FinishReason.ImageSafety ||
            reason == FinishReason.ProhibitedContent || reason == FinishReason.Spii)
        {
            return MessageDoneReason.ContentFilter;
        }

        return MessageDoneReason.Unknown;
    }

    /// <summary>
    /// IronHive의 MessageGenerationRequest를 Google GenAI SDK의 타입들로 변환합니다.
    /// </summary>
    private static (List<Content> contents, GenerateContentConfig config) ToGoogleAIParams(
        MessageGenerationRequest request)
    {
        var contents = new List<Content>();
        foreach (var msg in request.Messages)
        {
            // 사용자 메시지
            if (msg is { Role: MessageRole.User } user)
            {
                var parts = new List<Part>();
                foreach (var item in user.Content)
                {
                    // 텍스트 메시지
                    if (item is TextMessageContent text)
                    {
                        parts.Add(new Part
                        {
                            Text = text.Value ?? string.Empty
                        });
                    }
                    // 이미지 메시지
                    else if (item is ImageMessageContent image)
                    {
                        parts.Add(new Part
                        {
                            InlineData = new Blob
                            {
                                MimeType = image.Format switch
                                {
                                    ImageFormat.Png => "image/png",
                                    ImageFormat.Jpeg => "image/jpeg",
                                    ImageFormat.Webp => "image/webp",
                                    _ => throw new NotImplementedException("not supported yet")
                                },
                                Data = Convert.FromBase64String(image.Base64 ?? string.Empty)
                            }
                        });
                    }
                    else
                    {
                        throw new NotImplementedException("not supported yet");
                    }
                }
                contents.Add(new Content
                {
                    Role = "user",
                    Parts = parts
                });
            }
            // AI 메시지
            else if (msg is { Role: MessageRole.Assistant } assistant)
            {
                var modelParts = new List<Part>();
                var userParts = new List<Part>();

                foreach (var item in assistant.Content)
                {
                    // 사고 메시지
                    if (item is ThinkingMessageContent thinking)
                    {
                        modelParts.Add(new Part
                        {
                            Thought = true,
                            Text = thinking.Value ?? string.Empty,
                        });
                    }
                    // 텍스트 메시지
                    else if (item is TextMessageContent text)
                    {
                        modelParts.Add(new Part
                        {
                            Text = text.Value ?? string.Empty,
                        });
                    }
                    // 도구 메시지
                    else if (item is ToolMessageContent tool)
                    {
                        // 도구 호출 메시지
                        var part = new Part
                        {
                            FunctionCall = new FunctionCall
                            {
                                Id = tool.Id,
                                Name = tool.Name,
                                Args = JsonSerializer.Deserialize<Dictionary<string, object>>(tool.Input ?? "{}")
                            },
                            ThoughtSignature = !string.IsNullOrWhiteSpace(tool.Signature)
                                ? Convert.FromBase64String(tool.Signature)
                                : null
                        };
                        modelParts.Add(part);

                        // 도구 결과 메시지
                        userParts.Add(new Part
                        {
                            FunctionResponse = new FunctionResponse
                            {
                                Id = tool.Id,
                                Name = tool.Name,
                                Response = tool.Output != null
                                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(
                                        JsonSerializer.Serialize(tool.Output))
                                    : new Dictionary<string, object>()
                            }
                        });
                    }
                    else
                    {
                        throw new NotImplementedException("not supported yet");
                    }
                }

                contents.Add(new Content
                {
                    Role = "model",
                    Parts = modelParts
                });
                if (userParts.Count > 0)
                {
                    contents.Add(new Content
                    {
                        Role = "user",
                        Parts = userParts
                    });
                }
            }
            else
            {
                throw new NotImplementedException("not supported yet");
            }
        }

        // 도구 변환
        List<Tool>? tools = null;
        if (request.Tools?.Any() == true)
        {
            tools =
            [
                new Tool
                {
                    FunctionDeclarations = request.Tools.Select(t => new FunctionDeclaration
                    {
                        Name = t.UniqueName,
                        Description = t.Description ?? string.Empty,
                        ParametersJsonSchema = t.Parameters is not null
                            ? JsonSerializer.Deserialize<Dictionary<string, object>>(
                                JsonSerializer.Serialize(t.Parameters))
                            : null
                    }).ToList()
                }
            ];
        }

        // Thinking 설정, Gemini 3 이전 모델은 ThinkingLevel을 지원하지 않습니다.
        // https://ai.google.dev/api/generate-content?hl=ko#ThinkingConfig
        ThinkingConfig? thinkingConfig = null;
        if (request.ThinkingEffort is not null and not MessageThinkingEffort.None)
        {
            thinkingConfig = new ThinkingConfig
            {
                IncludeThoughts = true,
                ThinkingLevel = request.ThinkingEffort switch
                {
                    MessageThinkingEffort.Minimal => ThinkingLevel.Minimal,
                    MessageThinkingEffort.Low => ThinkingLevel.Low,
                    MessageThinkingEffort.Medium => ThinkingLevel.Medium,
                    MessageThinkingEffort.High => ThinkingLevel.High,
                    MessageThinkingEffort.XHigh => ThinkingLevel.High,
                    _ => ThinkingLevel.ThinkingLevelUnspecified
                }
            };
        }

        var config = new GenerateContentConfig
        {
            SystemInstruction = string.IsNullOrWhiteSpace(request.System) ? null : new Content
            {
                Parts = [new Part { Text = request.System }]
            },
            Tools = tools,
            CandidateCount = 1,
            MaxOutputTokens = request.MaxTokens,
            ThinkingConfig = thinkingConfig,
            ResponseMimeType = request.Output != null ? "application/json" : null,
            ResponseJsonSchema = request.Output switch
            {
                { Type: { } t } => JsonSchemaFactory.Build(t),
                { Schema: { } s } => JsonNode.Parse(s),
                _ => null
            },
        };

        return (contents, config);
    }
}

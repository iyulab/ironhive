using Google.GenAI;
using Google.GenAI.Types;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using GoogleContent = Google.GenAI.Types.Content;
using GooglePart = Google.GenAI.Types.Part;
using GoogleTool = Google.GenAI.Types.Tool;
using GoogleFunctionDeclaration = Google.GenAI.Types.FunctionDeclaration;
using GoogleThinkingConfig = Google.GenAI.Types.ThinkingConfig;
using GoogleGenerateContentConfig = Google.GenAI.Types.GenerateContentConfig;

namespace IronHive.Providers.GoogleAI;

/// <inheritdoc />
public class GoogleAIMessageGenerator : IMessageGenerator
{
    private readonly Client _client;

    public GoogleAIMessageGenerator(string apiKey)
        : this(new GoogleAIConfig { ApiKey = apiKey })
    { }

    public GoogleAIMessageGenerator(GoogleAIConfig config)
    {
        _client = GoogleAIClientFactory.CreateClient(config);
    }

    public GoogleAIMessageGenerator(VertexAIConfig config)
    {
        _client = GoogleAIClientFactory.CreateClient(config);
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
        var (model, contents, config) = ToGoogleAIParams(request);
        var response = await _client.Models.GenerateContentAsync(
            model, contents, config, cancellationToken);

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
        var (model, contents, config) = ToGoogleAIParams(request);

        // 인덱스 추적 관리용
        (int, MessageContent)? current = null;

        string id = string.Empty;
        string modelVersion = string.Empty;
        MessageDoneReason? reason = null;
        MessageTokenUsage? usage = null;

        await foreach (var res in _client.Models.GenerateContentStreamAsync(
            model, contents, config, cancellationToken))
        {
            Console.WriteLine($"Received response chunk: {JsonSerializer.Serialize(res)}");

            // 메시지 시작
            if (current == null)
            {
                id = res.ResponseId ?? Guid.NewGuid().ToString();
                modelVersion = res.ModelVersion ?? model;
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
            DoneReason = reason,
            TokenUsage = usage,
            Id = id,
            Model = modelVersion,
            Timestamp = DateTime.UtcNow,
        };
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
    private static (string model, List<GoogleContent> contents, GoogleGenerateContentConfig config) ToGoogleAIParams(
        MessageGenerationRequest request)
    {
        var contents = new List<GoogleContent>();
        foreach (var msg in request.Messages)
        {
            // 사용자 메시지
            if (msg is UserMessage user)
            {
                var parts = new List<GooglePart>();
                foreach (var item in user.Content)
                {
                    // 텍스트 메시지
                    if (item is TextMessageContent text)
                    {
                        parts.Add(new GooglePart
                        {
                            Text = text.Value ?? string.Empty
                        });
                    }
                    // 이미지 메시지
                    else if (item is ImageMessageContent image)
                    {
                        parts.Add(new GooglePart
                        {
                            InlineData = new Google.GenAI.Types.Blob
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
                contents.Add(new GoogleContent
                {
                    Role = "user",
                    Parts = parts
                });
            }
            // AI 메시지
            else if (msg is AssistantMessage assistant)
            {
                var modelParts = new List<GooglePart>();
                var userParts = new List<GooglePart>();

                foreach (var item in assistant.Content)
                {
                    // 사고 메시지
                    if (item is ThinkingMessageContent thinking)
                    {
                        modelParts.Add(new GooglePart
                        {
                            Thought = true,
                            Text = thinking.Value ?? string.Empty,
                        });
                    }
                    // 텍스트 메시지
                    else if (item is TextMessageContent text)
                    {
                        modelParts.Add(new GooglePart
                        {
                            Text = text.Value ?? string.Empty,
                        });
                    }
                    // 도구 메시지
                    else if (item is ToolMessageContent tool)
                    {
                        // 도구 호출 메시지
                        var part = new GooglePart
                        {
                            FunctionCall = new Google.GenAI.Types.FunctionCall
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
                        userParts.Add(new GooglePart
                        {
                            FunctionResponse = new Google.GenAI.Types.FunctionResponse
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

                contents.Add(new GoogleContent
                {
                    Role = "model",
                    Parts = modelParts
                });
                if (userParts.Count > 0)
                {
                    contents.Add(new GoogleContent
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
        List<GoogleTool>? tools = null;
        if (request.Tools?.Any() == true)
        {
            tools =
            [
                new GoogleTool
                {
                    FunctionDeclarations = request.Tools.Select(t => new GoogleFunctionDeclaration
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

        // Thinking 설정
        GoogleThinkingConfig? thinkingConfig = null;
        if (request.ThinkingEffort is not null and not MessageThinkingEffort.None)
        {
            thinkingConfig = new GoogleThinkingConfig
            {
                IncludeThoughts = true,
                ThinkingBudget = CalculateThinkingTokens(request)
            };
        }

        var config = new GoogleGenerateContentConfig
        {
            SystemInstruction = string.IsNullOrWhiteSpace(request.System) ? null : new GoogleContent
            {
                Parts = [new GooglePart { Text = request.System }]
            },
            Tools = tools,
            CandidateCount = 1,
            MaxOutputTokens = request.MaxTokens,
            StopSequences = request.StopSequences?.ToList(),
            TopP = request.TopP,
            TopK = request.TopK,
            Temperature = request.Temperature,
            ThinkingConfig = thinkingConfig,
            ResponseMimeType = request.Output != null ? "application/json" : null,
            ResponseJsonSchema = request.Output != null
                ? JsonSchemaFactory.Build(request.Output)
                : null,
        };

        return (request.Model, contents, config);
    }

    /// <summary>
    /// 추론(Thinking) 사용 시 토큰 예산을 계산합니다.
    /// <see href="https://ai.google.dev/gemini-api/docs/thinking#set-budget">모델별 토큰 범위</see>
    /// </summary>
    private static int CalculateThinkingTokens(MessageGenerationRequest request)
    {
        string model = request.Model.ToLowerInvariant();

        // Gemini 2.5 Pro (128 ~ 32768)
        if (model.Contains("gemini-2.5-pro"))
        {
            return request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => 1024,
                MessageThinkingEffort.Medium => 8192,
                MessageThinkingEffort.High => 32768,
                _ => -1 // dynamic
            };
        }
        // Gemini 2.5 Flash (0 ~ 24576)
        else if (model.Contains("gemini-2.5-flash"))
        {
            return request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => 1024,
                MessageThinkingEffort.Medium => 8192,
                MessageThinkingEffort.High => 24576,
                _ => -1 // dynamic
            };
        }
        // Gemini 2.5 Flash Lite(512 ~ 24576)
        else if (model.Contains("gemini-2.5-flash-lite"))
        {
            return request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => 512,
                MessageThinkingEffort.Medium => 8192,
                MessageThinkingEffort.High => 24576,
                _ => 0
            };
        }
        // Robotics-ER 1.5 Preview(512 ~ 24576)
        else if (model.Contains("robotics-er-1.5"))
        {
            return request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => 512,
                MessageThinkingEffort.Medium => 8192,
                MessageThinkingEffort.High => 24576,
                _ => 0
            };
        }
        // 이외 모델(512 ~ 8192)
        else
        {
            return request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => 512,
                MessageThinkingEffort.Medium => 2048,
                MessageThinkingEffort.High => 8192,
                _ => 0
            };
        }
    }
}

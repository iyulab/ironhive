using Google.GenAI;
using Google.GenAI.Types;
using IronHive.Abstractions.Extensions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IronHive.Providers.GoogleAI;

/// <inheritdoc />
public class GoogleAIMessageGenerator : IMessageGenerator
{
    private readonly Client _client;
    private readonly bool _isVertex;
    private readonly IReadOnlyDictionary<string, GoogleAIModelCapabilities>? _capabilityOverrides;

    /// <summary>
    /// Whether the configured key is in the format Gemini retired in 2026-09. Only the answer is kept —
    /// never the key — so the diagnostic in <see cref="GoogleAIExceptionMapper"/> can be raised without
    /// this type holding a credential.
    /// </summary>
    private readonly bool _keyUsesRetiredFormat;

    public GoogleAIMessageGenerator(string apiKey)
        : this(new GoogleAIConfig { ApiKey = apiKey })
    { }

    public GoogleAIMessageGenerator(GoogleAIConfig config)
    {
        _client = GoogleAIClientFactory.Create(config);
        _isVertex = false;
        _capabilityOverrides = CopyOverrides(config.ModelCapabilities);
        _keyUsesRetiredFormat = config.ApiKey?.StartsWith("AIza", StringComparison.Ordinal) == true;
    }

    public GoogleAIMessageGenerator(VertexAIConfig config)
    {
        _client = GoogleAIClientFactory.Create(config);
        _isVertex = true;
        // Vertex authenticates with application default credentials, not a Gemini API key.
        _capabilityOverrides = CopyOverrides(config.ModelCapabilities);
    }

    private static Dictionary<string, GoogleAIModelCapabilities>? CopyOverrides(
        IDictionary<string, GoogleAIModelCapabilities>? overrides)
        => overrides is { Count: > 0 }
            ? new Dictionary<string, GoogleAIModelCapabilities>(overrides, StringComparer.Ordinal)
            : null;

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
            request.Model, contents, config, cancellationToken)
            .MapException(ex => GoogleAIExceptionMapper.Map(ex, _keyUsesRetiredFormat, cancellationToken));

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
                    // Same minted shape as the streaming path ("tool_<short guid>"): the agent loop
                    // matches results back to calls by this id on both paths, and a consumer must
                    // not be able to tell the halves apart by it (GoogleAIEquivalenceTests).
                    Id = part.FunctionCall.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
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
            Model = response.ModelVersion,
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
        string? model = null;
        MessageDoneReason? reason = null;
        MessageTokenUsage? usage = null;
        // The buffered path reports ToolCall whenever a function call is present, wherever it sits
        // among the parts. The stream used to decide by whichever part arrived last, so a call
        // followed by narration came back as EndTurn on this path only (GoogleAIEquivalenceTests).
        var sawToolCall = false;

        await foreach (var res in _client.Models.GenerateContentStreamAsync(
            request.Model, contents, config, cancellationToken)
            .MapException(ex => GoogleAIExceptionMapper.Map(ex, _keyUsesRetiredFormat, cancellationToken), cancellationToken))
        {
            // 메시지 시작
            if (current == null)
            {
                id = res.ResponseId;
                model ??= res.ModelVersion;
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
                        sawToolCall = true;
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
                            // 동일 텍스트 블록인 경우, 델타 추가. 이미 yield한 Added 프레임의 객체를
                            // 여기서 변형하지 않는다 — 소비자는 그 객체에 델타를 누적하므로, 생성기가
                            // 같은 객체에 `Value += part.Text` 를 하면 텍스트가 두 번 들어간다
                            // (GoogleAIEquivalenceTests가 «Hello worldworld» 로 잡았다).
                            if (content is TextMessageContent)
                            {
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
                        sawToolCall = true;

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
            yield return new StreamingContentCompletedResponse
            {
                Index = current.Value.Item1,
            };
        }

        // 툴 호출이 하나라도 있었으면 완료 이유는 ToolCall — 버퍼드 경로와 같은 규칙
        if (sawToolCall)
        {
            reason = MessageDoneReason.ToolCall;
        }

        // 종료
        yield return new StreamingMessageDoneResponse
        {
            ResponseId = id,
            DoneReason = reason,
            Model = model,
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
    /// 모델 세대가 받지 못하는 비텍스트 도구 결과 블록의 텍스트 자리표시자 — 무엇이 빠졌는지(미디어 타입 · 크기)를 말한다.
    /// Chat Completions 경로가 도구 메시지에서 쓰는 것과 같은 규약.
    /// </summary>
    private static string OmittedBlockPlaceholder(string mimeType, string? base64)
    {
        var bytes = string.IsNullOrEmpty(base64) ? 0 : (int)Math.Round(base64.Length * 3.0 / 4.0);
        return $"[{mimeType} content omitted - {bytes} bytes; this model does not accept multimodal function responses]";
    }

    /// <summary>
    /// IronHive의 MessageGenerationRequest를 Google GenAI SDK의 타입들로 변환합니다.
    /// </summary>
    // Internal so that the request translation can be asserted without a network: the model-generation
    // policy (GoogleAIModelCapabilities) decides what reaches the wire, and that is what the facts check.
    internal (List<Content> contents, GenerateContentConfig config) ToGoogleAIParams(
        MessageGenerationRequest request)
    {
        var capabilities = GoogleAIModelCapabilities.Resolve(request.Model, _capabilityOverrides);
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
                                MimeType = ToMimeType(image.Format),
                                Data = Convert.FromBase64String(image.Base64 ?? string.Empty)
                            }
                        });
                    }
                    // 오디오 메시지
                    else if (item is AudioMessageContent audio)
                    {
                        parts.Add(new Part
                        {
                            InlineData = new Blob
                            {
                                MimeType = ToMimeType(audio.Format),
                                Data = Convert.FromBase64String(audio.Base64 ?? string.Empty)
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
                        // 텍스트 콘텐츠는 합쳐져 구조화 Response에 담기고, 이미지/오디오 콘텐츠는
                        // Parts에 InlineData로 추가됩니다(네이티브 멀티모달 지원).
                        var functionResponse = new FunctionResponse { Id = tool.Id, Name = tool.Name };
                        if (tool.Output is null)
                        {
                            functionResponse.Response = new Dictionary<string, object>();
                        }
                        else
                        {
                            var texts = new List<string>();
                            List<FunctionResponsePart>? responseParts = null;
                            foreach (var resultContent in tool.Output.Content)
                            {
                                switch (resultContent)
                                {
                                    case TextMessageContent resultText:
                                        texts.Add(resultText.Value ?? string.Empty);
                                        break;
                                    // A model generation that rejects inlineData in functionResponse.parts
                                    // (Gemini 2.5: 400 "Multimodal function responses are not supported") gets
                                    // the block named in the text result instead of a call that fails (#327).
                                    case ImageMessageContent resultImage when !capabilities.SupportsMultimodalFunctionResponse:
                                        texts.Add(OmittedBlockPlaceholder(ToMimeType(resultImage.Format), resultImage.Base64));
                                        break;
                                    case AudioMessageContent resultAudio when !capabilities.SupportsMultimodalFunctionResponse:
                                        texts.Add(OmittedBlockPlaceholder(ToMimeType(resultAudio.Format), resultAudio.Base64));
                                        break;
                                    case ImageMessageContent resultImage:
                                        (responseParts ??= []).Add(new FunctionResponsePart
                                        {
                                            InlineData = new FunctionResponseBlob
                                            {
                                                MimeType = ToMimeType(resultImage.Format),
                                                Data = Convert.FromBase64String(resultImage.Base64 ?? string.Empty)
                                            }
                                        });
                                        break;
                                    case AudioMessageContent resultAudio:
                                        (responseParts ??= []).Add(new FunctionResponsePart
                                        {
                                            InlineData = new FunctionResponseBlob
                                            {
                                                MimeType = ToMimeType(resultAudio.Format),
                                                Data = Convert.FromBase64String(resultAudio.Base64 ?? string.Empty)
                                            }
                                        });
                                        break;
                                }
                            }
                            functionResponse.Response = new Dictionary<string, object>
                            {
                                ["success"] = tool.Output.IsSuccess,
                                ["result"] = string.Join("\n", texts)
                            };
                            functionResponse.Parts = responseParts;
                        }

                        userParts.Add(new Part
                        {
                            FunctionResponse = functionResponse
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

        // Thinking 설정 — 세대별 제어 형태는 GoogleAIModelCapabilities 가 정합니다.
        // Gemini 3: thinkingLevel · Gemini 2.5: thinkingBudget · 그 이전: 없음.
        // https://ai.google.dev/api/generate-content?hl=ko#ThinkingConfig
        ThinkingConfig? thinkingConfig = null;
        if (request.ThinkingEffort is MessageThinkingEffort.None)
        {
            // None 은 「보내지 않음」이 아니라 「꺼 달라」입니다 — 기본으로 생각하는 모델은 thinking 토큰이 출력 예산을
            // 먹어 짧은 응답이 빈 문자열로 돌아옵니다. 끌 수 없는 모델에는 가장 낮은 단계를 보냅니다.
            thinkingConfig = capabilities.ThinkingControl switch
            {
                GoogleAIThinkingControl.None => null,
                _ when capabilities.SupportsZeroThinkingBudget => new ThinkingConfig { ThinkingBudget = 0 },
                GoogleAIThinkingControl.Budget => new ThinkingConfig { ThinkingBudget = 128 },
                _ => new ThinkingConfig
                {
                    ThinkingLevel = capabilities.SupportsMinimalThinking ? ThinkingLevel.Minimal : ThinkingLevel.Low
                },
            };
        }
        else if (request.ThinkingEffort is not null)
        {
            thinkingConfig = capabilities.ThinkingControl switch
            {
                GoogleAIThinkingControl.Level => new ThinkingConfig
                {
                    IncludeThoughts = true,
                    ThinkingLevel = request.ThinkingEffort switch
                    {
                        // minimal 을 받지 않는 모델(Gemini 3.8 Flash)은 지원되는 최저 단계로 강등합니다.
                        MessageThinkingEffort.Minimal => capabilities.SupportsMinimalThinking ? ThinkingLevel.Minimal : ThinkingLevel.Low,
                        MessageThinkingEffort.Low => ThinkingLevel.Low,
                        MessageThinkingEffort.Medium => ThinkingLevel.Medium,
                        MessageThinkingEffort.High => ThinkingLevel.High,
                        MessageThinkingEffort.XHigh => ThinkingLevel.High,
                        _ => ThinkingLevel.ThinkingLevelUnspecified
                    }
                },
                GoogleAIThinkingControl.Budget => new ThinkingConfig
                {
                    IncludeThoughts = true,
                    // 토큰 예산은 Anthropic budget 매핑과 같은 커뮤니티 기준; 상한은 Gemini 2.5 Flash 의 최대(24,576).
                    ThinkingBudget = request.ThinkingEffort switch
                    {
                        MessageThinkingEffort.Minimal => 1_024,
                        MessageThinkingEffort.Low => 4_000,
                        MessageThinkingEffort.Medium => 10_000,
                        MessageThinkingEffort.High => 20_000,
                        MessageThinkingEffort.XHigh => 24_576,
                        _ => null
                    }
                },
                _ => null
            };
        }

        // ThinkingOutput 은 생각을 보여 줄지만 정합니다(includeThoughts) — 노력도를 주지 않았어도 기본으로 생각하는 모델의
        // 요약을 받거나 숨길 수 있습니다. 생각을 끈 요청(None)에는 보여 줄 것이 없어 건드리지 않습니다.
        if (request.ThinkingOutput is { } output
            && capabilities.ThinkingControl != GoogleAIThinkingControl.None
            && request.ThinkingEffort is not MessageThinkingEffort.None)
        {
            thinkingConfig ??= new ThinkingConfig();
            thinkingConfig.IncludeThoughts = output != MessageThinkingOutput.None;
        }

        var config = new GenerateContentConfig
        {
            SystemInstruction = string.IsNullOrWhiteSpace(request.System) ? null : new Content
            {
                Parts = [new Part { Text = request.System }]
            },
            Tools = tools,
            // FunctionCallingConfig.AllowedFunctionNames가 여러 함수 이름을 그대로 지원하므로,
            // 다른 프로바이더와 달리 도구 목록을 별도로 필터링할 필요가 없습니다.
            ToolConfig = request.ToolChoice switch
            {
                null or AutoToolChoice => null,
                NoneToolChoice => new ToolConfig
                {
                    FunctionCallingConfig = new FunctionCallingConfig { Mode = FunctionCallingConfigMode.None }
                },
                RequiredToolChoice => new ToolConfig
                {
                    FunctionCallingConfig = new FunctionCallingConfig { Mode = FunctionCallingConfigMode.Any }
                },
                FunctionToolChoice f => new ToolConfig
                {
                    FunctionCallingConfig = new FunctionCallingConfig
                    {
                        Mode = FunctionCallingConfigMode.Any,
                        AllowedFunctionNames = f.Names.ToList()
                    }
                },
                _ => null
            },
            CandidateCount = 1,
            MaxOutputTokens = request.MaxTokens,
            // 샘플링 파라미터를 받지 않는 모델(Gemini 3.8 Flash)에는 전달하지 않습니다.
            Temperature = capabilities.SupportsSamplingParameters ? request.Temperature : null,
            TopP = capabilities.SupportsSamplingParameters ? request.TopP : null,
            TopK = capabilities.SupportsSamplingParameters ? request.TopK : null,
            StopSequences = request.StopSequences?.ToList(),
            ThinkingConfig = thinkingConfig,
            ResponseMimeType = request.OutputFormat != null ? "application/json" : null,
            ResponseJsonSchema = request.OutputFormat?.Schema,
        };

        return (contents, config);
    }

    private static string ToMimeType(ImageFormat format) => format switch
    {
        ImageFormat.Png => "image/png",
        ImageFormat.Jpeg => "image/jpeg",
        ImageFormat.Webp => "image/webp",
        _ => throw new NotImplementedException("not supported yet")
    };

    private static string ToMimeType(AudioFormat format) => format switch
    {
        AudioFormat.Wav => "audio/wav",
        AudioFormat.Mp3 => "audio/mp3",
        AudioFormat.Flac => "audio/flac",
        AudioFormat.Aac => "audio/aac",
        AudioFormat.Ogg => "audio/ogg",
        AudioFormat.Aiff => "audio/aiff",
        _ => throw new NotImplementedException("not supported yet")
    };

}

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Runtime.CompilerServices;
using Anthropic;
using Anthropic.Models.Messages;
using IronHiveMessage = IronHive.Abstractions.Messages.Message;
using IronHiveMessageRole = IronHive.Abstractions.Messages.MessageRole;
using IronHive.Abstractions.Extensions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using MessageContent = IronHive.Abstractions.Messages.MessageContent;
using TextMessageContent = IronHive.Abstractions.Messages.Content.TextMessageContent;
using ImageMessageContent = IronHive.Abstractions.Messages.Content.ImageMessageContent;
using ThinkingMessageContent = IronHive.Abstractions.Messages.Content.ThinkingMessageContent;

namespace IronHive.Providers.Anthropic;

/// <inheritdoc />
public class AnthropicMessageGenerator : IMessageGenerator
{
    // Extended thinking's minimum budget_tokens (vendor docs, "budget_tokens ... minimum is 1,024").
    private const int AnthropicMinThinkingBudget = 1_024;

    private readonly IAnthropicClient _client;
    private readonly IReadOnlyDictionary<string, AnthropicModelCapabilities>? _capabilityOverrides;

    public AnthropicMessageGenerator(string apiKey)
        : this(new AnthropicConfig { ApiKey = apiKey })
    { }

    public AnthropicMessageGenerator(AnthropicConfig config)
    {
        _client = AnthropicClientFactory.Create(config);
        _capabilityOverrides = config.ModelCapabilities is { Count: > 0 } overrides
            ? new Dictionary<string, AnthropicModelCapabilities>(overrides, StringComparer.Ordinal)
            : null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var req = ToMessageCreateParams(request);
        var res = await _client.Messages.Create(req, cancellationToken)
            .MapException(ex => AnthropicExceptionMapper.Map(ex, cancellationToken));

        var content = new List<MessageContent>();
        foreach (var block in res.Content)
        {
            // 추론 생성
            if (block.TryPickThinking(out var thinking))
            {
                content.Add(new ThinkingMessageContent
                {
                    Format = ThinkingFormat.Detailed,
                    Signature = thinking.Signature,
                    Value = thinking.Thinking,
                });
            }
            // 보안 추론 생성
            else if (block.TryPickRedactedThinking(out var redacted))
            {
                content.Add(new ThinkingMessageContent
                {
                    Format = ThinkingFormat.Secure,
                    Signature = Guid.NewGuid().ToShort(), // 보안 추론은 ID가 없으므로 임의로 생성
                    Value = redacted.Data,
                });
            }
            // 텍스트 생성
            else if (block.TryPickText(out var text))
            {
                content.Add(new TextMessageContent
                {
                    Value = text.Text
                });
            }
            // 툴 사용
            else if (block.TryPickToolUse(out var tool))
            {
                content.Add(new ToolMessageContent
                {
                    IsApproved = request.Tools?.TryGet(tool.Name, out var t) != true || t?.RequiresApproval == false,
                    Id = tool.ID ?? $"tool_{Guid.NewGuid().ToShort()}",
                    Name = tool.Name ?? string.Empty,
                    Input = JsonSerializer.Serialize(tool.Input)
                });
            }
        }

        return new MessageResponse
        {
            ResponseId = res.ID,
            DoneReason = res.StopReason?.Value() switch
            {
                StopReason.ToolUse => MessageDoneReason.ToolCall,
                StopReason.EndTurn or StopReason.PauseTurn => MessageDoneReason.EndTurn,
                StopReason.MaxTokens => MessageDoneReason.MaxTokens,
                StopReason.StopSequence or StopReason.Refusal => MessageDoneReason.StopSequence,
                _ => null
            },
            Message = new IronHiveMessage 
            { 
                Role = IronHiveMessageRole.Assistant,
                Content = content,
            },
            TokenUsage = new MessageTokenUsage
            {
                InputTokens = (int)res.Usage.InputTokens,
                OutputTokens = (int)res.Usage.OutputTokens
            },
            // Same envelope on both paths — the streaming done frame carries the model from
            // message_start (AnthropicMessagesEquivalenceTests compares them).
            Model = res.Model.Raw(),
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = ToMessageCreateParams(request);

        string? id = null;
        string? model = null;
        int index = 0;
        var usage = new MessageTokenUsage();

        await foreach (var evt in _client.Messages.CreateStreaming(req, cancellationToken)
            .MapException(ex => AnthropicExceptionMapper.Map(ex, cancellationToken), cancellationToken))
        {
            // 1. 메시지 시작 이벤트
            if (evt.TryPickStart(out var mse))
            {
                id = mse.Message.ID;
                model = mse.Message.Model.Raw();
                usage.InputTokens = (int)mse.Message.Usage.InputTokens;
                yield return new StreamingMessageBeginResponse();
            }
            // 2. 컨텐츠 생성 시작 이벤트
            else if (evt.TryPickContentBlockStart(out var cse))
            {
                // 추론 생성
                if (cse.ContentBlock.TryPickThinking(out var thinking))
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
                else if (cse.ContentBlock.TryPickRedactedThinking(out var redacted))
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
                else if (cse.ContentBlock.TryPickText(out var text))
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
                else if (cse.ContentBlock.TryPickToolUse(out var tool))
                {
                    yield return new StreamingContentAddedResponse
                    {
                        Index = index,
                        Content = new ToolMessageContent
                        {
                            IsApproved = request.Tools?.TryGet(tool.Name!, out var t) != true || t?.RequiresApproval == false,
                            Id = tool.ID ?? $"tool_{Guid.NewGuid().ToShort()}",
                            Name = tool.Name ?? string.Empty,
                        }
                    };
                }
            }
            // 3. 컨텐츠 델타 이벤트
            else if (evt.TryPickContentBlockDelta(out var cde))
            {
                // 추론 생성
                if (cde.Delta.TryPickThinking(out var thinkingDelta))
                {
                    yield return new StreamingContentDeltaResponse
                    {
                        Index = index,
                        Delta = new ThinkingDeltaContent
                        {
                            Data = thinkingDelta.Thinking,
                        },
                    };
                }
                // 추론 서명 전달
                else if (cde.Delta.TryPickSignature(out var signature))
                {
                    yield return new StreamingContentUpdatedResponse
                    {
                        Index = index,
                        Updated = new SignatureUpdatedContent
                        {
                            Signature = signature.Signature,
                        }
                    };
                }
                // 텍스트 생성
                else if (cde.Delta.TryPickText(out var textDelta))
                {
                    yield return new StreamingContentDeltaResponse
                    {
                        Index = index,
                        Delta = new TextDeltaContent
                        {
                            Value = textDelta.Text,
                        }
                    };
                }
                // 툴 사용
                else if (cde.Delta.TryPickInputJson(out var toolDelta))
                {
                    yield return new StreamingContentDeltaResponse
                    {
                        Index = index,
                        Delta = new ToolDeltaContent
                        {
                            Input = toolDelta.PartialJson
                        }
                    };
                }
            }
            // 4. 컨텐츠 생성 종료 이벤트
            else if (evt.TryPickContentBlockStop(out _))
            {
                yield return new StreamingContentCompletedResponse
                {
                    Index = index
                };
                index++;
            }
            // 5. 메시지 메타 데이터 이벤트
            else if (evt.TryPickDelta(out var mde))
            {
                usage.OutputTokens = (int)(mde.Usage?.OutputTokens ?? usage.OutputTokens);

                yield return new StreamingMessageDoneResponse
                {
                    ResponseId = id,
                    Model = model,
                    DoneReason = mde.Delta.StopReason?.Value() switch
                    {
                        StopReason.ToolUse => MessageDoneReason.ToolCall,
                        StopReason.EndTurn or StopReason.PauseTurn => MessageDoneReason.EndTurn,
                        StopReason.MaxTokens => MessageDoneReason.MaxTokens,
                        StopReason.StopSequence or StopReason.Refusal => MessageDoneReason.StopSequence,
                        _ => null
                    },
                    TokenUsage = usage
                };
            }
        }
    }

    /// <inheritdoc />
    public async Task<int> CountTokensAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var createParams = ToMessageCreateParams(request);
        var countParams = new MessageCountTokensParams
        {
            Model = createParams.Model,
            Messages = createParams.Messages,
            System = new MessageCountTokensParamsSystem(request.System ?? string.Empty),
            Tools = createParams.Tools?.Select(t =>
            {
                t.TryPickTool(out var tool);
                return (MessageCountTokensTool)tool!;
            }).ToList(),
            Thinking = createParams.Thinking,
            OutputConfig = createParams.OutputConfig,
        };
        var result = await _client.Messages.CountTokens(countParams, cancellationToken);
        return (int)result.InputTokens;
    }

    /// <summary>
    /// IronHive의 MessageGenerationRequest를 Anthropic SDK의 MessageCreateParams로 변환합니다.
    /// </summary>
    // Internal so that the request translation can be asserted without a network: the model-generation
    // policy (AnthropicModelCapabilities) decides what reaches the wire, and that is what the facts check.
    internal MessageCreateParams ToMessageCreateParams(MessageGenerationRequest request)
    {
        var capabilities = AnthropicModelCapabilities.Resolve(request.Model, _capabilityOverrides);

        var messages = new List<MessageParam>();
        foreach (var message in request.Messages)
        {
            // 사용자 메시지
            if (message is { Role: IronHiveMessageRole.User } user)
            {
                var blocks = new List<ContentBlockParam>();
                foreach (var item in user.Content)
                {
                    // 텍스트 메시지
                    if (item is TextMessageContent text)
                    {
                        blocks.Add(new TextBlockParam
                        {
                            Text = text.Value ?? string.Empty
                        });
                    }
                    // 이미지 메시지
                    else if (item is ImageMessageContent image)
                    {
                        blocks.Add(new ImageBlockParam
                        {
                            Source = new Base64ImageSource
                            {
                                MediaType = ToMediaType(image.Format),
                                Data = image.Base64 ?? string.Empty
                            }
                        });
                    }
                    else
                    {
                        throw new NotSupportedException($"not supported type {item.GetType()}");
                    }
                }

                messages.Add(new MessageParam
                {
                    Role = Role.User,
                    Content = blocks
                });
            }
            // AI 메시지
            else if (message is { Role: IronHiveMessageRole.Assistant } assistant)
            {
                var groups = assistant.GroupContentByToolBoundary();
                foreach (var group in groups)
                {
                    var assistantBlocks = new List<ContentBlockParam>();
                    var userBlocks = new List<ContentBlockParam>();

                    // 추론 모델은 툴 사용 시나리오에서
                    // ToolUseMessageContent 이전에 반드시 추론 메시지가 존재해야 합니다.
                    var isLastMessage = message == request.Messages.Last();
                    var isFirstGroup = group == groups.First();
                    if (isLastMessage && isFirstGroup && request.ThinkingEffort != MessageThinkingEffort.None)
                    {
                        foreach (var thinkingItem in group.OfType<ThinkingMessageContent>())
                        {
                            if (thinkingItem.Format == ThinkingFormat.Secure)
                            {
                                assistantBlocks.Add(new RedactedThinkingBlockParam
                                {
                                    Data = thinkingItem.Value ?? string.Empty,
                                });
                            }
                            else
                            {
                                assistantBlocks.Add(new ThinkingBlockParam
                                {
                                    Signature = thinkingItem.Signature ?? string.Empty,
                                    Thinking = thinkingItem.Value ?? string.Empty
                                });
                            }
                        }
                    }

                    foreach (var content in group)
                    {
                        // 추론 메시지
                        if (content is ThinkingMessageContent)
                        {
                            continue;
                        }
                        // 텍스트 메시지
                        else if (content is TextMessageContent text)
                        {
                            assistantBlocks.Add(new TextBlockParam
                            {
                                Text = text.Value ?? string.Empty
                            });
                        }
                        // 도구 메시지
                        else if (content is ToolMessageContent tool)
                        {
                            assistantBlocks.Add(new ToolUseBlockParam
                            {
                                ID = tool.Id,
                                Name = tool.Name,
                                Input = !string.IsNullOrWhiteSpace(tool.Input)
                                    ? JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(tool.Input) ?? new()
                                    : new Dictionary<string, JsonElement>()
                            });
                            // 텍스트/이미지는 각각 TextBlockParam/ImageBlockParam으로 그대로 매핑하고,
                            // Anthropic이 블록으로 지원하지 않는 콘텐츠(오디오 등)는 설명 텍스트로 대체합니다.
                            ToolResultBlockParamContent resultContent = string.Empty;
                            if (tool.Output is { Content.Count: > 0 } toolOutput)
                            {
                                var resultBlocks = new List<Block>();
                                foreach (var c in toolOutput.Content)
                                {
                                    if (c is TextMessageContent resultText)
                                    {
                                        resultBlocks.Add(new TextBlockParam { Text = resultText.Value ?? string.Empty });
                                    }
                                    else if (c is ImageMessageContent resultImage)
                                    {
                                        resultBlocks.Add(new ImageBlockParam
                                        {
                                            Source = new Base64ImageSource
                                            {
                                                MediaType = ToMediaType(resultImage.Format),
                                                Data = resultImage.Base64 ?? string.Empty
                                            }
                                        });
                                    }
                                    else
                                    {
                                        resultBlocks.Add(new TextBlockParam { Text = "[unsupported content omitted — not supported in Anthropic tool_result blocks]" });
                                    }
                                }
                                resultContent = resultBlocks;
                            }

                            userBlocks.Add(new ToolResultBlockParam
                            {
                                ToolUseID = tool.Id,
                                IsError = tool.Output != null && !tool.Output.IsSuccess,
                                Content = resultContent,
                            });
                        }
                        else
                        {
                            throw new NotImplementedException("not supported yet");
                        }
                    }

                    messages.Add(new MessageParam
                    {
                        Role = Role.Assistant,
                        Content = assistantBlocks
                    });
                    if (userBlocks.Count > 0)
                    {
                        messages.Add(new MessageParam
                        {
                            Role = Role.User,
                            Content = userBlocks
                        });
                    }
                }
            }
            else
            {
                throw new NotImplementedException("not supported yet");
            }
        }

        // 도구 변환
        // 다중 함수명 강제(FunctionToolChoice.Names.Count > 1)는 Anthropic wire에 표현할 수 없어
        // ToolChoiceAny로 근사하는 대신, 도구 목록 자체를 해당 이름들로 필터링합니다.
        var toolSource = request.ToolChoice is FunctionToolChoice { Names.Count: > 1 } multiChoice
            ? request.Tools?.FilterBy(multiChoice.Names)
            : request.Tools;
        var tools = toolSource?.Select(t =>
        {
            ToolUnion toolUnion = new Tool
            {
                Name = t.UniqueName,
                Description = t.Description,
                // Every ITool.Parameters shape must reach the wire — a JsonObject (FunctionTool), a
                // JsonElement (AIFunction / MCP tool schemas), a JsonNode, a JSON string or a POCO.
                // Gating on JsonObject alone sent every bridge and MCP tool with an empty schema.
                InputSchema = InputSchema.FromRawUnchecked(AnthropicHelper.ToInputSchema(t.Parameters))
            };
            return toolUnion;
        })?.ToList();

        // 출력 구성 지원
        JsonOutputFormat? format = null;
        if (request.OutputFormat is { } outputFormat)
        {
            var schemaDict = AnthropicHelper.ToAnthropicCompatibleSchema(outputFormat.Schema)
                .Deserialize<IReadOnlyDictionary<string, JsonElement>>()!;
            format = JsonOutputFormat.FromRawUnchecked(schemaDict);
        }

        // 추론 구성 지원
        ThinkingConfigParam? thinking = null;
        Effort? effort = null;
        if (request.ThinkingEffort is MessageThinkingEffort.None && capabilities.ThinkingStyle == AnthropicThinkingStyle.Adaptive)
        {
            // None 은 「보내지 않음」이 아니라 「꺼 달라」입니다 — Claude Sonnet 5 · Opus 5 · Fable 은 thinking 을
            // 생략해도 adaptive 로 생각합니다. 끌 수 있는 모델에는 disabled 를, 끌 수 없는 모델(Fable: disabled 는 400)과
            // vendor 가 끄기 대신 낮은 effort 를 권하는 모델(Opus 5)에는 가장 낮은 effort 를 보냅니다.
            // Budget 세대는 생략이 곧 off 라 아무것도 보내지 않습니다.
            if (capabilities.SupportsDisabledThinking)
            {
                thinking = new ThinkingConfigDisabled();
            }
            else
            {
                effort = Effort.Low;
            }
        }
        else if (request.ThinkingEffort is not null and not MessageThinkingEffort.None)
        {
            if (capabilities.ThinkingStyle == AnthropicThinkingStyle.Budget)
            {
                // 구버전 모델들은 ThinkingConfigEnabled 방식으로 추론을 설정합니다.
                // 토큰은 OpenAI o-series, Gemini thinking_budget 커뮤니티 기준을 참고.
                long? budget = request.ThinkingEffort switch
                {
                    MessageThinkingEffort.Minimal => 1_024,
                    MessageThinkingEffort.Low => 4_000,
                    MessageThinkingEffort.Medium => 10_000,
                    MessageThinkingEffort.High => 20_000,
                    MessageThinkingEffort.XHigh => 32_000,
                    _ => null
                };

                // budget_tokens 는 max_tokens 보다 작아야 합니다(아니면 400 «max_tokens must be greater than
                // thinking.budget_tokens»). 호출자가 작은 출력 한도를 주면 예산을 그 절반으로 줄여 답변 자리를
                // 남기고, 그것이 vendor 최소 예산(1,024)에도 못 미치면 thinking 을 켜지 않습니다 — 그 한도 안에서는
                // 생각과 답을 함께 담을 수 없습니다.
                if (budget is not null && request.MaxTokens is { } maxTokens && budget >= maxTokens)
                {
                    budget = maxTokens / 2 >= AnthropicMinThinkingBudget ? maxTokens / 2 : null;
                }

                if (budget is { } b)
                {
                    thinking = new ThinkingConfigEnabled(b);
                }
            }
            else
            {
                // 최신 모델들은 Adaptive 추론 전략을 사용합니다.
                // https://platform.claude.com/docs/en/build-with-claude/adaptive-thinking
                thinking = new ThinkingConfigAdaptive { };
                // adaptive 의 깊이는 output_config.effort 가 정합니다(생략 = high). 요청의 단계를 옮기지 않으면
                // Minimal 과 XHigh 가 같은 요청이 됩니다. Minimal 은 vendor 최저 low 로, xhigh 를 받지 않는 세대는 high 로.
                effort = request.ThinkingEffort switch
                {
                    MessageThinkingEffort.Minimal or MessageThinkingEffort.Low => Effort.Low,
                    MessageThinkingEffort.Medium => Effort.Medium,
                    MessageThinkingEffort.High => Effort.High,
                    MessageThinkingEffort.XHigh => capabilities.SupportsXHighEffort ? Effort.Xhigh : Effort.High,
                    _ => null
                };
            }
        }

        OutputConfig? outputConfig = format is null && effort is null
            ? null
            : new OutputConfig { Format = format, Effort = effort };

        // 도구 호출 강제(tool_choice any/tool)를 받지 않는 모델(Claude 5.1 계열)에서는 vendor 처방대로
        // auto 로 강등하고 시스템 프롬프트 끝에 명시 지시를 덧붙입니다. 400 을 그대로 흘리지도, 아무 말 없이
        // auto 로 바꾸지도 않습니다 — 강등은 wire 의 지시문으로 보입니다.
        var system = request.System ?? string.Empty;
        var toolChoice = request.ToolChoice;
        if (!capabilities.SupportsForcedToolChoice && toolChoice is RequiredToolChoice or FunctionToolChoice)
        {
            system = AnthropicHelper.AppendForcedToolChoiceInstruction(system, toolChoice);
            toolChoice = new AutoToolChoice();
        }

        return new MessageCreateParams
        {
            Model = request.Model,
            System = new MessageCreateParamsSystem(system),
            Messages = messages,
            // 필수요청사항으로 64K로 기본값을 설정합니다.
            MaxTokens = request.MaxTokens ?? 64000,
            // Temperature/TopP/TopK are deliberately NOT forwarded: Anthropic deprecated them and
            // models released after Claude Opus 4.6 reject any value with a 400. Mapping them would
            // turn a silent no-op into a hard request failure, which is worse than ignoring them.
            StopSequences = request.StopSequences?.ToList(),
            Tools = tools?.Count > 0 ? tools : null,
            // 다중 함수명(FunctionToolChoice.Names.Count > 1)은 Anthropic wire에 "이 N개 중 하나 강제"에
            // 해당하는 값이 없어 ToolChoiceAny로 근사합니다 — 대신 위에서 도구 목록 자체를 필터링합니다.
            ToolChoice = toolChoice switch
            {
                null or AutoToolChoice => null,
                NoneToolChoice => new ToolChoiceNone(),
                RequiredToolChoice => new ToolChoiceAny(),
                FunctionToolChoice { Names.Count: 1 } f => new ToolChoiceTool(f.Names.First()),
                FunctionToolChoice => new ToolChoiceAny(),
                _ => null
            },
            Thinking = thinking,
            OutputConfig = outputConfig,
        };
    }

    private static string ToMediaType(ImageFormat format) => format switch
    {
        ImageFormat.Png => "image/png",
        ImageFormat.Jpeg => "image/jpeg",
        ImageFormat.Gif => "image/gif",
        ImageFormat.Webp => "image/webp",
        _ => throw new NotSupportedException($"not supported image format {format}")
    };

}

public static class AnthropicHelper
{
    /// <summary>
    /// budget 방식 thinking 을 쓰는 Claude 4.x 모델 id — <see cref="AnthropicModelCapabilities.BuiltIn"/>의 재료입니다.
    /// 세대 판정은 이 목록이 아니라 <see cref="AnthropicModelCapabilities.Resolve"/>를 통해 합니다.
    /// </summary>
    public static readonly string[] LegacyModels =
    [
        "claude-haiku-4-5",
        "claude-haiku-4-5-20251001",
        "claude-sonnet-4",
        "claude-sonnet-4-20250514",
        "claude-sonnet-4-5",
        "claude-sonnet-4-5-20250929",
        "claude-opus-4",
        "claude-opus-4-20250514",
        "claude-opus-4-1",
        "claude-opus-4-1-20250805",
        "claude-opus-4-5",
        "claude-opus-4-5-20251101",
    ];

    /// <summary>
    /// 도구 호출 강제를 받지 않는 모델을 위해 시스템 프롬프트 끝에 명시 지시를 덧붙입니다
    /// (vendor 마이그레이션 가이드: <c>tool_choice: auto</c> + 명시 지시). 원문은 변경하지 않고 빈 줄로 잇습니다.
    /// </summary>
    public static string AppendForcedToolChoiceInstruction(string system, IronHive.Abstractions.Messages.ToolChoice? toolChoice)
    {
        var instruction = toolChoice switch
        {
            FunctionToolChoice { Names.Count: 1 } single =>
                $"You must respond by calling the tool named `{single.Names.First()}`. Do not answer in plain text.",
            FunctionToolChoice multiple =>
                $"You must respond by calling one of these tools: {string.Join(", ", multiple.Names.Select(n => $"`{n}`"))}. Do not answer in plain text.",
            _ => "You must respond by calling one of the provided tools. Do not answer in plain text.",
        };
        return string.IsNullOrWhiteSpace(system) ? instruction : $"{system.TrimEnd()}\n\n{instruction}";
    }

    /// <summary>
    /// Anthropic 구조화 출력이 지원하지 않는 스키마 구성을 Anthropic 호환 형태로 변환합니다.
    /// Anthropic은 nullable 유니온 타입(`type: [X, "null"]`)을 지원하지 않아 단일 타입으로 평탄화하고,
    /// 모든 object 스키마에 `additionalProperties: false`를 강제합니다. 원본은 변경하지 않습니다.
    /// </summary>
    public static JsonNode ToAnthropicCompatibleSchema(JsonNode schema)
    {
        var clone = schema.DeepClone();
        FlattenNullableAndRestrictProperties(clone);
        return clone;
    }

    /// <summary>
    /// <see cref="ITool.Parameters"/>를 Anthropic <c>input_schema</c> 사전으로 정규화합니다.
    /// 파라미터는 도구 출처에 따라 <see cref="JsonObject"/>(FunctionTool), <see cref="JsonElement"/>
    /// (AIFunction·MCP 도구의 <c>JsonSchema</c>), 다른 <see cref="JsonNode"/>, JSON 문자열, POCO 중
    /// 무엇이든 될 수 있고, 어느 형태든 같은 스키마로 나가야 합니다. Messages API는
    /// <c>input_schema.type == "object"</c>를 요구하므로 <c>type</c>이 없으면 채우고, 파라미터가 없는
    /// 도구는 <c>{"type":"object","properties":{}}</c>로 보냅니다.
    /// </summary>
    internal static Dictionary<string, JsonElement> ToInputSchema(object? parameters)
    {
        var node = parameters switch
        {
            null => null,
            JsonObject obj => obj,
            JsonNode other => other as JsonObject,
            JsonElement { ValueKind: JsonValueKind.Object } element => JsonObject.Create(element),
            JsonElement => null,
            string json => TryParseObject(json),
            _ => JsonSerializer.SerializeToNode(parameters) as JsonObject,
        };

        var dictionary = node?.Deserialize<Dictionary<string, JsonElement>>() ?? new Dictionary<string, JsonElement>();
        if (!dictionary.ContainsKey("type"))
            dictionary["type"] = JsonSerializer.SerializeToElement("object");
        if (!dictionary.ContainsKey("properties"))
            dictionary["properties"] = JsonSerializer.SerializeToElement(new JsonObject());
        return dictionary;

        static JsonObject? TryParseObject(string json)
        {
            try { return JsonNode.Parse(json) as JsonObject; }
            catch (JsonException) { return null; }
        }
    }

    // ToAnthropicCompatibleSchema가 최초 1회 호출하고, 아래에서 각 object/array 자식으로
    // 재귀 호출됩니다(중첩 object의 properties, array의 items 등). 이 재귀 특성 때문에
    // 하나의 지역 함수로 접었을 때 진입점(1회성 clone)과 순회 로직(재귀)이 뒤섞여 읽기 어려워
    // 별도 private 메서드로 분리했습니다.
    private static void FlattenNullableAndRestrictProperties(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                if (obj["properties"] is JsonObject properties)
                {
                    var required = new HashSet<string>();
                    if (obj["required"] is JsonArray requiredArray)
                    {
                        foreach (var item in requiredArray)
                        {
                            if (item?.GetValue<string>() is { } name)
                                required.Add(name);
                        }
                    }

                    var newRequired = new JsonArray();
                    foreach (var (key, value) in properties.ToList())
                    {
                        var isNullable = false;
                        if (value is JsonObject propertySchema && propertySchema["type"] is JsonArray typeArray)
                        {
                            string? keepType = null;
                            foreach (var typeEntry in typeArray)
                            {
                                var typeName = typeEntry?.GetValue<string>();
                                if (typeName == "null")
                                    isNullable = true;
                                else if (typeName != null && keepType is null)
                                    keepType = typeName;
                            }
                            if (keepType != null)
                                propertySchema["type"] = keepType;
                        }

                        if (!isNullable || required.Contains(key))
                            newRequired.Add(JsonValue.Create(key));
                    }

                    if (newRequired.Count > 0)
                        obj["required"] = newRequired;
                    else
                        obj.Remove("required");

                    obj["additionalProperties"] = false;
                }

                foreach (var (_, value) in obj.ToList())
                    FlattenNullableAndRestrictProperties(value);
                break;

            case JsonArray arr:
                foreach (var item in arr.ToList())
                    FlattenNullableAndRestrictProperties(item);
                break;
        }
    }
}
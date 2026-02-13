using System.Text.Json;
using System.Text.Json.Nodes;
using System.Runtime.CompilerServices;
using Anthropic;
using Anthropic.Models.Messages;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using MessageContent = IronHive.Abstractions.Messages.MessageContent;
using TextMessageContent = IronHive.Abstractions.Messages.Content.TextMessageContent;
using ImageMessageContent = IronHive.Abstractions.Messages.Content.ImageMessageContent;
using DocumentMessageContent = IronHive.Abstractions.Messages.Content.DocumentMessageContent;
using ThinkingMessageContent = IronHive.Abstractions.Messages.Content.ThinkingMessageContent;

namespace IronHive.Providers.Anthropic;

/// <inheritdoc />
public class AnthropicMessageGenerator : IMessageGenerator
{
    private readonly IAnthropicClient _client;

    public AnthropicMessageGenerator(string apiKey)
        : this(new AnthropicConfig { ApiKey = apiKey })
    { }

    public AnthropicMessageGenerator(AnthropicConfig config)
    {
        _client = AnthropicClientFactory.CreateClient(config);
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
        var res = await _client.Messages.Create(req, cancellationToken);

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
            Id = res.ID,
            DoneReason = res.StopReason?.ToString() switch
            {
                "tool_use" => MessageDoneReason.ToolCall,
                "end_turn" => MessageDoneReason.EndTurn,
                "max_tokens" => MessageDoneReason.MaxTokens,
                "stop_sequence" => MessageDoneReason.StopSequence,
                _ => null
            },
            Message = new AssistantMessage
            {
                Id = res.ID,
                Model = res.Model,
                Content = content,
            },
            TokenUsage = new MessageTokenUsage
            {
                InputTokens = (int)(res.Usage.InputTokens),
                OutputTokens = (int)(res.Usage.OutputTokens)
            },
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = ToMessageCreateParams(request);

        var id = string.Empty;
        var model = string.Empty;
        int index = 0;
        var usage = new MessageTokenUsage();

        await foreach (var evt in _client.Messages.CreateStreaming(req, cancellationToken))
        {
            // 1. 메시지 시작 이벤트
            if (evt.TryPickStart(out var mse))
            {
                id = mse.Message.ID;
                model = mse.Message.Model;
                usage.InputTokens = (int)(mse.Message.Usage.InputTokens);
                yield return new StreamingMessageBeginResponse
                {
                    Id = id
                };
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
                        Updated = new ThinkingUpdatedContent
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
                    Id = id,
                    Model = model,
                    DoneReason = mde.Delta.StopReason?.ToString() switch
                    {
                        "tool_use" => MessageDoneReason.ToolCall,
                        "end_turn" => MessageDoneReason.EndTurn,
                        "stop_sequence" => MessageDoneReason.StopSequence,
                        "max_tokens" => MessageDoneReason.MaxTokens,
                        _ => null
                    },
                    TokenUsage = usage,
                    Timestamp = DateTime.UtcNow
                };
            }
        }
    }

    /// <summary>
    /// IronHive의 MessageGenerationRequest를 Anthropic SDK의 MessageCreateParams로 변환합니다.
    /// </summary>
    private static MessageCreateParams ToMessageCreateParams(MessageGenerationRequest request)
    {
        var messages = new List<MessageParam>();
        foreach (var message in request.Messages)
        {
            // 사용자 메시지
            if (message is UserMessage user)
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
                    // 파일 문서 메시지
                    else if (item is DocumentMessageContent document)
                    {
                        blocks.Add(new TextBlockParam
                        {
                            Text = $"Document Content:\n{document.Data}"
                        });
                    }
                    // 이미지 메시지
                    else if (item is ImageMessageContent image)
                    {
                        blocks.Add(new ImageBlockParam
                        {
                            Source = new Base64ImageSource
                            {
                                MediaType = image.Format switch
                                {
                                    ImageFormat.Png => "image/png",
                                    ImageFormat.Jpeg => "image/jpeg",
                                    ImageFormat.Gif => "image/gif",
                                    ImageFormat.Webp => "image/webp",
                                    _ => throw new NotSupportedException($"not supported image format {image.Format}")
                                },
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
            else if (message is AssistantMessage assistant)
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
                            userBlocks.Add(new ToolResultBlockParam
                            {
                                ToolUseID = tool.Id,
                                IsError = tool.Output != null && !tool.Output.IsSuccess,
                                Content = tool.Output?.Result ?? string.Empty,
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

        // 필수요청사항인 토큰을 계산합니다.
        var (maxTokens, thinking) = CalculateTokens(request);

        // 도구 변환
        var tools = request.Tools?.Select(t =>
        {
            ToolUnion toolUnion = new Tool
            {
                Name = t.UniqueName,
                Description = t.Description,
                InputSchema = t.Parameters is JsonObject jsonObj
                    ? InputSchema.FromRawUnchecked(
                        jsonObj.Deserialize<Dictionary<string, JsonElement>>() ?? new())
                    : InputSchema.FromRawUnchecked(new Dictionary<string, JsonElement>())
            };
            return toolUnion;
        })?.ToList();

        return new MessageCreateParams
        {
            Model = request.Model,
            System = new MessageCreateParamsSystem(request.System ?? string.Empty),
            Messages = messages,
            MaxTokens = maxTokens,
            StopSequences = request.StopSequences?.ToList(),
            Tools = tools?.Count > 0 ? tools : null,
            Thinking = thinking,
            // 추론 모델의 경우 토큰샘플링 방식을 임의로 설정할 수 없습니다.
            Temperature = thinking != null ? null : request.Temperature,
            TopP = thinking != null ? null : request.TopP,
            TopK = thinking != null ? null : (long?)request.TopK,
        };
    }

    /// <summary>
    /// 최대 토큰 수를 계산하고 추론 예산을 설정합니다.
    /// </summary>
    private static (int, ThinkingConfigParam?) CalculateTokens(MessageGenerationRequest request)
    {
        // MaxToken이 필수요청사항으로 sonnet은 "64000", opus는 "32000" 이외 "8192"값을 기본으로 함
        var maxTokens = request.MaxTokens ??
        (
            request.Model.Contains("sonnet") ? 64_000
            : request.Model.Contains("opus") ? 32_000
            : 8192
        );

        // 추론을 사용할 경우 예산 토큰을 설정합니다.
        if (request.ThinkingEffort is not null and not MessageThinkingEffort.None)
        {
            // 절대값 기반 + 비례값 기반 혼합 전략
            int budgetTokens = (int)(request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => Math.Min(maxTokens * 0.25, 12_000),    // 25%, 12K
                MessageThinkingEffort.Medium => Math.Min(maxTokens * 0.50, 32_000), // 50%, 32K
                MessageThinkingEffort.High => Math.Min(maxTokens * 0.75, 48_000),   // 75%, 48K
                _ => 1024 // 최소값
            });
            // 최소 "1024"토큰이상 사용하도록 강제
            budgetTokens = budgetTokens >= 1024 ? budgetTokens : 1024;
            ThinkingConfigParam thinkingConfig = new ThinkingConfigEnabled
            {
                BudgetTokens = budgetTokens
            };
            return (maxTokens, thinkingConfig);
        }

        return (maxTokens, null);
    }
}

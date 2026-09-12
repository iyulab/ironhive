using System.ClientModel;
using System.ClientModel.Primitives;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using IronHive.Abstractions.Extensions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using OpenAI.Responses;
using IronHiveMessage = IronHive.Abstractions.Messages.Message;
using IronHiveMessageRole = IronHive.Abstractions.Messages.MessageRole;

using TextMessageContent = IronHive.Abstractions.Messages.Content.TextMessageContent;
using ImageMessageContent = IronHive.Abstractions.Messages.Content.ImageMessageContent;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// Message generator targeting the OpenAI <b>Responses</b> API (<c>POST /v1/responses</c>), the first-party
/// OpenAI surface. Supports reasoning summaries and <c>reasoning.encrypted_content</c>. Not implemented by
/// OpenAI-compatible / self-hosted servers — those are served by <c>IronHive.Providers.OpenAI.Compatible</c>'s
/// Chat Completions generator instead.
/// </summary>
public class OpenAIMessageGenerator : IMessageGenerator
{
    private readonly ResponsesClient _client;

    public OpenAIMessageGenerator(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIMessageGenerator(OpenAIConfig config)
    {
        _client = OpenAIClientFactory.Create(config).GetResponsesClient();
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
        var options = BuildOptions(request);
        var result = await _client.CreateResponseAsync(options, cancellationToken)
            .MapException(ex => OpenAIExceptionMapper.Map(ex, cancellationToken));
        var response = result.Value;
        if (response.Error != null)
        {
            throw new InvalidOperationException(
                $"OpenAI API Error: {response.Error.Code} - {response.Error.Message}");
        }

        var content = new List<MessageContent>();
        foreach (var item in response.OutputItems)
        {
            if (item is MessageResponseItem mi)
            {
                foreach (var part in mi.Content)
                {
                    if (part.Text != null)
                    {
                        content.Add(new TextMessageContent { Value = part.Text });
                    }
                }
            }
            else if (item is ReasoningResponseItem ri)
            {
                content.Add(new ThinkingMessageContent
                {
                    Format = ThinkingFormat.Summary,
                    Signature = ri.EncryptedContent,
                    Value = ri.SummaryParts?.Count > 0
                        ? string.Join("\n---\n", ri.SummaryParts
                            .OfType<ReasoningSummaryTextPart>()
                            .Select(s => s.Text?.Trim() ?? string.Empty))
                        : string.Empty
                });
            }
            else if (item is FunctionCallResponseItem fci)
            {
                content.Add(new ToolMessageContent
                {
                    Id = fci.CallId,
                    Name = fci.FunctionName,
                    Input = fci.FunctionArguments?.ToString() ?? string.Empty,
                    IsApproved = request.Tools?.TryGet(fci.FunctionName, out var t) != true || t?.RequiresApproval == false
                });
            }
        }

        var reason = MessageDoneReason.EndTurn;
        var incompleteReason = response.IncompleteStatusDetails?.Reason?.ToString();
        if (!string.IsNullOrWhiteSpace(incompleteReason))
        {
            reason = incompleteReason switch
            {
                "max_output_tokens" => MessageDoneReason.MaxTokens,
                "content_filter" => MessageDoneReason.ContentFilter,
                _ => MessageDoneReason.Unknown,
            };
        }
        if (content.OfType<ToolMessageContent>().Any())
        {
            reason = MessageDoneReason.ToolCall;
        }

        return new MessageResponse
        {
            ResponseId = response.Id,
            DoneReason = reason,
            Message = new IronHiveMessage
            {
                Role = IronHiveMessageRole.Assistant,
                Content = content,
            },
            TokenUsage = new MessageTokenUsage
            {
                InputTokens = response.Usage?.InputTokenCount ?? 0,
                OutputTokens = response.Usage?.OutputTokenCount ?? 0
            },
            // Same envelope on every path: the streaming done frame carries these too (see below),
            // and the equivalence test compares them.
            Model = response.Model,
            Timestamp = response.CreatedAt.UtcDateTime,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var options = BuildOptions(request);
        options.StreamingEnabled = true;

        int pIndex = 0;
        var reason = MessageDoneReason.EndTurn;
        await foreach (var update in _client.CreateResponseStreamingAsync(options, cancellationToken)
            .MapException(ex => OpenAIExceptionMapper.Map(ex, cancellationToken), cancellationToken))
        {
            if (update is StreamingResponseCreatedUpdate)
            {
                yield return new StreamingMessageBeginResponse();
            }
            else if (update is StreamingResponseFailedUpdate failed)
            {
                yield return new StreamingMessageErrorResponse
                {
                    Code = failed.Response.Error?.Code.ToString() ?? "Unknown",
                    Message = failed.Response.Error?.Message ?? "An unknown error occurred."
                };
            }
            else if (update is StreamingResponseOutputItemAddedUpdate outputAdded)
            {
                if (outputAdded.Item is ReasoningResponseItem ri)
                {
                    yield return new StreamingContentAddedResponse
                    {
                        Index = outputAdded.OutputIndex,
                        Content = new ThinkingMessageContent
                        {
                            Format = ThinkingFormat.Summary,
                            Signature = ri.EncryptedContent,
                            Value = ri.SummaryParts?.Count > 0
                                ? string.Join("\n---\n", ri.SummaryParts
                                    .OfType<ReasoningSummaryTextPart>()
                                    .Select(s => s.Text?.Trim() ?? string.Empty))
                                : string.Empty
                        }
                    };
                }
                else if (outputAdded.Item is FunctionCallResponseItem fci)
                {
                    reason = MessageDoneReason.ToolCall;
                    yield return new StreamingContentAddedResponse
                    {
                        Index = outputAdded.OutputIndex,
                        Content = new ToolMessageContent
                        {
                            Id = fci.CallId,
                            Name = fci.FunctionName,
                            Input = fci.FunctionArguments?.ToString() ?? string.Empty,
                            IsApproved = request.Tools?.TryGet(fci.FunctionName, out var t) != true || t?.RequiresApproval == false
                        }
                    };
                }
            }
            else if (update is StreamingResponseContentPartAddedUpdate contentPartAdded)
            {
                if (contentPartAdded.Part?.Text != null)
                {
                    yield return new StreamingContentAddedResponse
                    {
                        Index = contentPartAdded.OutputIndex,
                        Content = new TextMessageContent
                        {
                            Value = contentPartAdded.Part.Text
                        }
                    };
                }
            }
            else if (update is StreamingResponseOutputTextDeltaUpdate textDelta)
            {
                var isAdded = pIndex != textDelta.ContentIndex;
                if (isAdded) pIndex = textDelta.ContentIndex;

                yield return new StreamingContentDeltaResponse
                {
                    Index = textDelta.OutputIndex,
                    Delta = new TextDeltaContent
                    {
                        Value = isAdded ? $"\n---\n{textDelta.Delta}" : textDelta.Delta
                    },
                };
            }
            else if (update is StreamingResponseReasoningSummaryTextDeltaUpdate reasoningDelta)
            {
                var isAdded = pIndex != reasoningDelta.SummaryIndex;
                if (isAdded) pIndex = reasoningDelta.SummaryIndex;

                yield return new StreamingContentDeltaResponse
                {
                    Index = reasoningDelta.OutputIndex,
                    Delta = new ThinkingDeltaContent
                    {
                        Data = isAdded ? $"\n---\n{reasoningDelta.Delta}" : reasoningDelta.Delta
                    },
                };
            }
            else if (update is StreamingResponseFunctionCallArgumentsDeltaUpdate toolDelta)
            {
                yield return new StreamingContentDeltaResponse
                {
                    Index = toolDelta.OutputIndex,
                    Delta = new ToolDeltaContent
                    {
                        Input = toolDelta.Delta?.ToString() ?? string.Empty
                    },
                };
            }
            else if (update is StreamingResponseOutputItemDoneUpdate outputDone)
            {
                if (outputDone.Item is ReasoningResponseItem ri)
                {
                    yield return new StreamingContentUpdatedResponse
                    {
                        Index = outputDone.OutputIndex,
                        Updated = new SignatureUpdatedContent
                        {
                            Signature = ri.EncryptedContent ?? string.Empty
                        }
                    };
                }

                pIndex = 0;
                yield return new StreamingContentCompletedResponse
                {
                    Index = outputDone.OutputIndex
                };
            }
            else if (update is StreamingResponseIncompleteUpdate incomplete)
            {
                reason = incomplete.Response.IncompleteStatusDetails?.Reason?.ToString() switch
                {
                    "max_output_tokens" => MessageDoneReason.MaxTokens,
                    "content_filter" => MessageDoneReason.ContentFilter,
                    _ => MessageDoneReason.Unknown,
                };
                yield return DoneFrame(incomplete.Response, reason);
            }
            else if (update is StreamingResponseCompletedUpdate completed)
            {
                yield return DoneFrame(completed.Response, reason);
            }
        }
    }

    /// <summary>
    /// The one place a streaming done frame is built from a Responses API result, whichever event
    /// carried it (completed or incomplete). Two copies of this once disagreed: the incomplete copy
    /// prefixed the id with "openai_" — which MessageService then prefixed again — and the completed
    /// copy carried no model or timestamp (0.26.1, OpenAIResponsesEquivalenceTests).
    /// </summary>
    private static StreamingMessageDoneResponse DoneFrame(ResponseResult response, MessageDoneReason reason) => new()
    {
        ResponseId = response.Id,
        DoneReason = reason,
        Model = response.Model,
        TokenUsage = new MessageTokenUsage
        {
            InputTokens = response.Usage?.InputTokenCount ?? 0,
            OutputTokens = response.Usage?.OutputTokenCount ?? 0
        },
        Timestamp = response.CreatedAt.UtcDateTime,
    };

    /// <inheritdoc />
    public async Task<int> CountTokensAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = BuildOptions(request);
        var serialized = ModelReaderWriter.Write(options);

        // token count endpoint rejects fields like 'include', 'stream', 'store', 'background'
        var body = JsonSerializer.Deserialize<JsonObject>(serialized)!;
        body.Remove("include");
        body.Remove("stream");
        var content = BinaryContent.Create(BinaryData.FromString(body.ToJsonString()));
        var result = await _client.GetInputTokenCountAsync(
            content, "application/json",
            new RequestOptions { CancellationToken = cancellationToken });
        using var doc = JsonDocument.Parse(result.GetRawResponse().Content);
        return doc.RootElement.GetProperty("input_tokens").GetInt32();
    }

    private static CreateResponseOptions BuildOptions(MessageGenerationRequest request)
    {
        var options = new CreateResponseOptions
        {
            Model = request.Model,
            Instructions = request.System,
        };

        if (request.MaxTokens.HasValue)
            options.MaxOutputTokenCount = request.MaxTokens.Value;

        if (request.Temperature.HasValue)
            options.Temperature = request.Temperature.Value;

        if (request.TopP.HasValue)
            options.TopP = request.TopP.Value;

        if (request.ThinkingEffort is not null and not MessageThinkingEffort.None)
        {
            options.IncludedProperties.Add(
                new IncludedResponseProperty("reasoning.encrypted_content"));
            options.ReasoningOptions = new ResponseReasoningOptions
            {
                ReasoningEffortLevel = request.ThinkingEffort switch
                {
                    MessageThinkingEffort.Minimal => ResponseReasoningEffortLevel.Minimal,
                    MessageThinkingEffort.Low => ResponseReasoningEffortLevel.Low,
                    MessageThinkingEffort.Medium => ResponseReasoningEffortLevel.Medium,
                    MessageThinkingEffort.High => ResponseReasoningEffortLevel.High,
                    MessageThinkingEffort.XHigh => new ResponseReasoningEffortLevel("xhigh"),
                    _ => ResponseReasoningEffortLevel.None
                },
                ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Auto
            };
        }

        if (request.OutputFormat is { } outputFormat)
        {
            options.TextOptions = new ResponseTextOptions
            {
                TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(
                    "output",
                    BinaryData.FromObjectAsJson(outputFormat.Schema),
                    jsonSchemaIsStrict: false)
            };
        }

        if (request.Tools != null)
        {
            // 다중 함수명은 CreateRequiredChoice() + 도구 목록 필터링으로 근사합니다. Responses API는
            // 정확히 이 시맨틱을 위한 tool_choice:"allowed_tools"를 지원하지만 이 SDK 버전엔 아직
            // 없음 — SDK가 추가하거나 CreateResponseOptions.Patch로 직접 넣을 때 이 부분을 바꾸면 됩니다.
            var toolSource = request.ToolChoice is FunctionToolChoice { Names.Count: > 1 } multiChoice
                ? request.Tools.FilterBy(multiChoice.Names)
                : request.Tools;
            foreach (var t in toolSource)
            {
                var parameters = t.Parameters ?? new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject()
                };
                options.Tools.Add(ResponseTool.CreateFunctionTool(
                    t.UniqueName,
                    BinaryData.FromString(JsonSerializer.Serialize(parameters)),
                    null,
                    t.Description));
            }
        }

        options.ToolChoice = request.ToolChoice switch
        {
            null or AutoToolChoice => null,
            NoneToolChoice => ResponseToolChoice.CreateNoneChoice(),
            RequiredToolChoice => ResponseToolChoice.CreateRequiredChoice(),
            FunctionToolChoice { Names.Count: 1 } f => ResponseToolChoice.CreateFunctionChoice(f.Names.First()),
            FunctionToolChoice => ResponseToolChoice.CreateRequiredChoice(),
            _ => null
        };

        foreach (var msg in request.Messages)
        {
            if (msg is { Role: IronHiveMessageRole.User } user)
            {
                var parts = new List<ResponseContentPart>();
                foreach (var item in user.Content)
                {
                    if (item is TextMessageContent text)
                    {
                        parts.Add(ResponseContentPart.CreateInputTextPart(
                            text.Value ?? string.Empty));
                    }
                    else if (item is ImageMessageContent image)
                    {
                        parts.Add(ResponseContentPart.CreateInputImagePart(
                            new Uri(EnsureBase64Url(image)),
                            ResponseImageDetailLevel.Auto));
                    }
                    else
                    {
                        throw new NotImplementedException("not supported yet");
                    }
                }
                options.InputItems.Add(
                    ResponseItem.CreateUserMessageItem(parts));
            }
            else if (msg is { Role: IronHiveMessageRole.Assistant } assistant)
            {
                foreach (var group in assistant.GroupContentByToolBoundary())
                {
                    foreach (var content in group)
                    {
                        if (content is ThinkingMessageContent thinking)
                        {
                            options.InputItems.Add(ResponseItem.CreateReasoningItem(
                                thinking.Value ?? string.Empty));
                        }
                        else if (content is TextMessageContent text)
                        {
                            options.InputItems.Add(ResponseItem.CreateAssistantMessageItem(
                                text.Value ?? string.Empty));
                        }
                        else if (content is ToolMessageContent tool)
                        {
                            options.InputItems.Add(ResponseItem.CreateFunctionCallItem(
                                tool.Id ?? string.Empty,
                                tool.Name,
                                BinaryData.FromString(tool.Input ?? string.Empty)));

                            // Responses API의 function_call_output.output은 이 SDK 버전에서 plain string만
                            // 노출한다(구조화 콘텐츠 없음) — 텍스트는 그대로 이어붙이고, 비텍스트 콘텐츠는
                            // 설명 텍스트로 대체한다.
                            options.InputItems.Add(ResponseItem.CreateFunctionCallOutputItem(
                                tool.Id ?? string.Empty,
                                tool.Output is null || tool.Output.Content.Count == 0
                                    ? string.Empty
                                    : string.Join("\n", tool.Output.Content.Select(c => c switch
                                    {
                                        TextMessageContent text => text.Value ?? string.Empty,
                                        _ => "[unsupported content omitted — not supported in this provider's tool-result format]"
                                    }))));
                        }
                        else
                        {
                            throw new NotImplementedException("not supported yet");
                        }
                    }
                }
            }
            else
            {
                throw new NotImplementedException("not supported yet");
            }
        }

        return options;
    }

    private static string EnsureBase64Url(ImageMessageContent image)
    {
        if (image.Base64.StartsWith("data:", StringComparison.Ordinal))
            return image.Base64;

        var format = image.Format switch
        {
            ImageFormat.Png => "image/png",
            ImageFormat.Jpeg => "image/jpeg",
            ImageFormat.Gif => "image/gif",
            ImageFormat.Webp => "image/webp",
            _ => throw new NotSupportedException($"Unsupported image format: {image.Format}")
        };
        return $"data:{format};base64,{image.Base64}";
    }
}
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using IronHive.Abstractions.Extensions;
using IronHive.Abstractions.Http;
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
    private readonly IReadOnlyDictionary<string, OpenAIModelCapabilities>? _capabilityOverrides;
    private readonly TimeSpan _streamIdleTimeout = System.Threading.Timeout.InfiniteTimeSpan;
    private readonly IReadOnlyCollection<string> _credentialHeaders;

    public OpenAIMessageGenerator(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIMessageGenerator(OpenAIConfig config)
    {
        ProviderStreams.ThrowIfInvalid(config.StreamIdleTimeout, $"{nameof(OpenAIConfig)}.{nameof(OpenAIConfig.StreamIdleTimeout)}");
        _streamIdleTimeout = config.StreamIdleTimeout;
        _client = OpenAIClientFactory.Create(config).GetResponsesClient();
        _credentialHeaders = (config.ApiKeyPlacement ?? CredentialPlacement.Bearer).ReservedHeaderNames;
        _capabilityOverrides = config.ModelCapabilities is { Count: > 0 } overrides
            ? new Dictionary<string, OpenAIModelCapabilities>(overrides, StringComparer.Ordinal)
            : null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private IReadOnlyDictionary<string, string>? ResolveRequestHeaders(MessageGenerationRequest request)
        => ProviderRequestHeaders.ResolveRequest(nameof(OpenAIConfig), nameof(OpenAIConfig.ApiKey), _credentialHeaders, request.Headers);

    // Request features this provider does not carry. The official SDK builds its request body, which this library does
    // not extend, and this provider does not return token log probabilities; dropping either silently would answer a
    // request other than the one asked for.
    private static void RejectUnsupported(MessageGenerationRequest request)
    {
        if (request.ExtraBody is { Count: > 0 })
            throw new NotSupportedException(
                "MessageGenerationRequest.ExtraBody is not supported by the OpenAI provider; the OpenAI-compatible provider honours it.");

        // Answering without them would hand the caller a response it cannot tell from one that has none.
        if (request.LogProbabilities is not null)
            throw new NotSupportedException(
                "MessageGenerationRequest.LogProbabilities is not supported by the OpenAI provider; the OpenAI-compatible provider returns them.");
    }

    /// <inheritdoc />
    public async Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        RejectUnsupported(request);
        var options = BuildOptions(request, _capabilityOverrides);
        using var headers = RequestHeadersScope.Begin(ResolveRequestHeaders(request));
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

        var stop = StopSequenceFilter.For(request.StopSequences);
        if (stop is not null)
            content = CutAtStop(content, stop);

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
        if (stop?.Stopped == true)
        {
            reason = MessageDoneReason.StopSequence;
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
            TokenUsage = UsageOf(response),
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
        RejectUnsupported(request);
        var options = BuildOptions(request, _capabilityOverrides);
        options.StreamingEnabled = true;
        // The SDK sends the request on the first read below, inside this iteration — the scope covers it.
        using var headers = RequestHeadersScope.Begin(ResolveRequestHeaders(request));

        int pIndex = 0;
        var reason = MessageDoneReason.EndTurn;
        // StopSequences, kept on the client (StopSequenceFilter): text is held back until it cannot start a stop
        // sequence; once one is seen, nothing after it is emitted — the rest of the stream is still read so the done
        // frame carries the response's id, model and usage, as the buffered call does.
        var stop = StopSequenceFilter.For(request.StopSequences);
        int? heldIndex = null;
        int? stoppedIndex = null;
        await foreach (var update in ProviderStreams.WithIdleTimeout(
                ct => _client.CreateResponseStreamingAsync(options, ct), _streamIdleTimeout, cancellationToken)
            .MapException(ex => OpenAIExceptionMapper.Map(ex, cancellationToken), cancellationToken))
        {
            if (stoppedIndex is { } cut && update is not (StreamingResponseCompletedUpdate or StreamingResponseIncompleteUpdate
                    or StreamingResponseFailedUpdate)
                && !(update is StreamingResponseOutputItemDoneUpdate done && done.OutputIndex == cut))
            {
                continue;
            }

            if (heldIndex is { } held && update is StreamingResponseOutputItemDoneUpdate or StreamingResponseCompletedUpdate
                    or StreamingResponseIncompleteUpdate)
            {
                heldIndex = null;
                var tail = stop!.Flush();
                if (tail.Length > 0)
                    yield return new StreamingContentDeltaResponse { Index = held, Delta = new TextDeltaContent { Value = tail } };
            }

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

                var value = isAdded ? $"\n---\n{textDelta.Delta}" : textDelta.Delta;
                if (stop is not null)
                {
                    value = stop.Push(value);
                    heldIndex = textDelta.OutputIndex;
                    if (stop.Stopped)
                    {
                        heldIndex = null;
                        stoppedIndex = textDelta.OutputIndex;
                        if (reason != MessageDoneReason.ToolCall)
                            reason = MessageDoneReason.StopSequence;
                    }
                }

                if (value.Length > 0)
                {
                    yield return new StreamingContentDeltaResponse
                    {
                        Index = textDelta.OutputIndex,
                        Delta = new TextDeltaContent
                        {
                            Value = value
                        },
                    };
                }
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
                // Output that ended at a stop sequence ended there, whatever cut the rest of the stream short.
                if (stoppedIndex is null)
                {
                    reason = incomplete.Response.IncompleteStatusDetails?.Reason?.ToString() switch
                    {
                        "max_output_tokens" => MessageDoneReason.MaxTokens,
                        "content_filter" => MessageDoneReason.ContentFilter,
                        _ => MessageDoneReason.Unknown,
                    };
                }
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
    /// <summary>
    /// The buffered half of <see cref="StopSequenceFilter"/>: content up to the first stop sequence, in order — the
    /// text before it, and nothing after it.
    /// </summary>
    private static List<MessageContent> CutAtStop(List<MessageContent> content, StopSequenceFilter stop)
    {
        var kept = new List<MessageContent>(content.Count);
        foreach (var item in content)
        {
            if (item is TextMessageContent text && stop.Cut(text.Value) is { } before)
            {
                if (before.Length > 0)
                    kept.Add(new TextMessageContent { Value = before });
                break;
            }
            kept.Add(item);
        }
        return kept;
    }

    private static StreamingMessageDoneResponse DoneFrame(ResponseResult response, MessageDoneReason reason) => new()
    {
        ResponseId = response.Id,
        DoneReason = reason,
        Model = response.Model,
        TokenUsage = UsageOf(response),
        Timestamp = response.CreatedAt.UtcDateTime,
    };

    // One reading of the usage for the buffered response and the streaming done frame alike.
    private static MessageTokenUsage UsageOf(ResponseResult response) => new()
    {
        InputTokens = response.Usage?.InputTokenCount ?? 0,
        OutputTokens = response.Usage?.OutputTokenCount ?? 0,
        CachedInputTokens = response.Usage?.InputTokenDetails?.CachedTokenCount
    };

    /// <inheritdoc />
    public async Task<int> CountTokensAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = BuildOptions(request, _capabilityOverrides);
        var serialized = ModelReaderWriter.Write(options);

        // token count endpoint rejects fields like 'include', 'stream', 'store', 'background'
        var body = JsonSerializer.Deserialize<JsonObject>(serialized)!;
        body.Remove("include");
        body.Remove("stream");
        var content = BinaryContent.Create(BinaryData.FromString(body.ToJsonString()));
        using var headers = RequestHeadersScope.Begin(ResolveRequestHeaders(request));
        var result = await _client.GetInputTokenCountAsync(
            content, "application/json",
            new RequestOptions { CancellationToken = cancellationToken });
        using var doc = JsonDocument.Parse(result.GetRawResponse().Content);
        return doc.RootElement.GetProperty("input_tokens").GetInt32();
    }

    internal static CreateResponseOptions BuildOptions(
        MessageGenerationRequest request,
        IReadOnlyDictionary<string, OpenAIModelCapabilities>? capabilityOverrides)
    {
        var options = new CreateResponseOptions
        {
            Model = request.Model,
            Instructions = request.System,
            // 대화는 호출자가 들고 매 턴 input 에 전부 보냅니다(previous_response_id 를 쓰지 않음) — Responses API 의
            // 기본값 store:true 는 이 생성기에 아무것도 주지 않고 모든 프롬프트·도구 결과·응답의 사본만 vendor 에 남깁니다.
            // 추론 연속성은 저장된 응답이 아니라 reasoning.encrypted_content 왕복으로 잇습니다(아래 reasoning 아이템).
            StoredOutputEnabled = false,
        };

        if (request.MaxTokens.HasValue)
            options.MaxOutputTokenCount = request.MaxTokens.Value;

        if (request.Temperature.HasValue)
            options.Temperature = request.Temperature.Value;

        if (request.TopP.HasValue)
            options.TopP = request.TopP.Value;

        // 노력도는 모델이 받는 값으로 번역합니다 — gpt-5.1+ 는 minimal 을, gpt-5/5.1/o 계열은 xhigh 를, gpt-4 계열은
        // reasoning 자체를 400 으로 거부합니다. None 은 「보내지 않음」이 아니라 「꺼 달라」(none, 없으면 최저)입니다.
        var wireEffort = OpenAIModelCapabilities.Resolve(request.Model, capabilityOverrides).ToWireEffort(request.ThinkingEffort);
        if (wireEffort is not null)
        {
            options.ReasoningOptions = new ResponseReasoningOptions
            {
                ReasoningEffortLevel = new ResponseReasoningEffortLevel(wireEffort),
            };
            if (wireEffort != "none")
            {
                // 암호화된 추론은 멀티턴 연속성용이라 노출 요청과 무관하게 받습니다. 요약은 ThinkingOutput 이 정합니다 —
                // None 은 요약 미요청, Full 은 가장 자세한 요약(detailed; 원문 사고는 vendor 가 주지 않음), 미설정은 auto.
                options.IncludedProperties.Add(new IncludedResponseProperty("reasoning.encrypted_content"));
                if (request.ThinkingOutput is not MessageThinkingOutput.None)
                {
                    options.ReasoningOptions.ReasoningSummaryVerbosity = request.ThinkingOutput is MessageThinkingOutput.Full
                        ? ResponseReasoningSummaryVerbosity.Detailed
                        : ResponseReasoningSummaryVerbosity.Auto;
                }
            }
        }

        if (request.OutputFormat is { } outputFormat)
        {
            options.TextOptions = new ResponseTextOptions
            {
                TextFormat = outputFormat.Schema is { } schema
                    ? ResponseTextFormat.CreateJsonSchemaFormat(
                        "output",
                        BinaryData.FromObjectAsJson(schema),
                        jsonSchemaIsStrict: false)
                    : ResponseTextFormat.CreateJsonObjectFormat()
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

        if (request.Tools is { Count: > 0 } && request.AllowParallelToolCalls is { } allowParallel)
        {
            options.ParallelToolCallsEnabled = allowParallel;
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
                            // store:false 에서는 서버가 이전 추론을 기억하지 않으므로, 응답에서 받은 암호화 본문
                            // (Signature)을 그대로 돌려줘야 모델이 도구 호출 전후의 추론을 잇습니다.
                            options.InputItems.Add(new ReasoningResponseItem(thinking.Value ?? string.Empty)
                            {
                                EncryptedContent = string.IsNullOrEmpty(thinking.Signature) ? null : thinking.Signature,
                            });
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
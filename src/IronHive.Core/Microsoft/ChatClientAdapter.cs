using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;

namespace IronHive.Core.Microsoft;

/// <summary>
/// IronHive IMessageGenerator를 Microsoft.Extensions.AI IChatClient로 래핑하는 어댑터입니다.
/// </summary>
public class ChatClientAdapter : IChatClient
{
    private readonly IMessageGenerator _generator;
    private readonly string _modelId;
    private readonly string _providerName;

    /// <summary>
    /// <c>FunctionCallContent.AdditionalProperties</c> 키 — provider가 멀티턴 연속성을 위해 tool call에
    /// 붙인 서명(<c>MessageContent.Signature</c>, 예: Gemini 3의 <c>thought_signature</c>)을 IChatClient
    /// 왕복 사이에 나릅니다. 이 어댑터가 만든 <see cref="FunctionCallContent"/>를 그대로 히스토리에 넣어 다시
    /// 보내면 provider는 같은 서명을 돌려받습니다 — 값은 불투명하며 소비자가 해석할 것이 없습니다.
    /// </summary>
    public const string SignatureKey = "IronHive.Signature";

    /// <summary>
    /// ChatClientAdapter의 새 인스턴스를 생성합니다.
    /// </summary>
    /// <param name="generator">IronHive 메시지 생성기</param>
    /// <param name="modelId">사용할 모델 ID</param>
    /// <param name="providerName">Provider 이름 (선택)</param>
    public ChatClientAdapter(IMessageGenerator generator, string modelId, string? providerName = null)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _modelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _providerName = providerName ?? "IronHive";
    }

    /// <inheritdoc />
    public ChatClientMetadata Metadata => new(
        providerName: _providerName,
        providerUri: null,
        defaultModelId: _modelId);

    /// <inheritdoc />
    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();
        var request = ConvertToRequest(messageList, options);
        var response = await _generator.GenerateMessageAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return ConvertToResponse(response);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();
        var request = ConvertToRequest(messageList, options);

        // Buffer tool calls: index -> (callId, name, argumentsJson)
        var toolCallBuffers = new Dictionary<int, (string CallId, string Name, StringBuilder Arguments, string? Signature)>();

        await foreach (var chunk in _generator.GenerateStreamingMessageAsync(request, cancellationToken)
            .ConfigureAwait(false))
        {
            switch (chunk)
            {
                case StreamingContentAddedResponse added when added.Content is ToolMessageContent tool:
                    var initialArguments = new StringBuilder();
                    if (!string.IsNullOrEmpty(tool.Input))
                        initialArguments.Append(tool.Input);
                    toolCallBuffers[added.Index] = (
                        tool.Id ?? Guid.NewGuid().ToString(),
                        tool.Name ?? string.Empty,
                        initialArguments,
                        tool.Signature);
                    break;

                // A provider may attach the tool call's signature after the block was added (Anthropic
                // delivers signatures as an update); keep it with the buffered call so the completed
                // FunctionCallContent carries it.
                case StreamingContentUpdatedResponse updated
                    when updated.Updated is SignatureUpdatedContent signatureUpdated
                         && toolCallBuffers.TryGetValue(updated.Index, out var signedBuffer):
                    toolCallBuffers[updated.Index] = signedBuffer with { Signature = signatureUpdated.Signature };
                    break;

                // The first text/thinking chunk arrives in StreamingContentAddedResponse.Content.Value,
                // not as a delta — emit it so the first character is not silently dropped.
                case StreamingContentAddedResponse added when added.Content is TextMessageContent textAdded:
                    if (!string.IsNullOrEmpty(textAdded.Value))
                    {
                        yield return new ChatResponseUpdate
                        {
                            ResponseId = null,
                            Contents = [new TextContent(textAdded.Value)]
                        };
                    }
                    break;

                case StreamingContentAddedResponse added when added.Content is ThinkingMessageContent thinkingAdded:
                    if (!string.IsNullOrEmpty(thinkingAdded.Value))
                    {
                        yield return new ChatResponseUpdate
                        {
                            ResponseId = null,
                            AdditionalProperties = new AdditionalPropertiesDictionary
                            {
                                ["IndexThinking.ThinkingContent"] = thinkingAdded.Value
                            }
                        };
                    }
                    break;

                case StreamingContentDeltaResponse delta when delta.Delta is ToolDeltaContent toolDelta:
                    if (toolCallBuffers.TryGetValue(delta.Index, out var buffer))
                    {
                        buffer.Arguments.Append(toolDelta.Input);
                    }
                    break;

                case StreamingContentCompletedResponse completed:
                    if (toolCallBuffers.TryGetValue(completed.Index, out var completedTool))
                    {
                        var arguments = ParseToolArguments(completedTool.Arguments.ToString());

                        yield return new ChatResponseUpdate
                        {
                            ResponseId = null,
                            Contents = [WithSignature(new FunctionCallContent(
                                callId: completedTool.CallId,
                                name: completedTool.Name,
                                arguments: arguments), completedTool.Signature)]
                        };
                        toolCallBuffers.Remove(completed.Index);
                    }
                    break;

                case StreamingMessageErrorResponse error:
                    throw new InvalidOperationException(
                        $"Streaming error: {error.Code} - {error.Message}");

                default:
                    var update = ConvertToStreamingUpdate(chunk);
                    if (update is not null)
                    {
                        yield return update;
                    }
                    break;
            }
        }
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceType == typeof(IMessageGenerator))
            return _generator;

        return null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private MessageGenerationRequest ConvertToRequest(IList<ChatMessage> chatMessages, ChatOptions? options)
    {
        var request = new MessageGenerationRequest
        {
            Model = options?.ModelId ?? _modelId,
            Messages = []
        };

        // Collect tool results keyed by callId for merging into assistant messages
        var toolResults = new Dictionary<string, ToolOutput>();
        foreach (var msg in chatMessages)
        {
            foreach (var content in msg.Contents)
            {
                if (content is FunctionResultContent result && result.CallId is not null)
                {
                    toolResults[result.CallId] = ToToolOutput(result);
                }
            }
        }

        foreach (var msg in chatMessages)
        {
            if (msg.Role == ChatRole.System)
            {
                request.System = msg.Text;
            }
            else
            {
                var converted = ConvertMessage(msg, toolResults);
                if (converted is not null)
                {
                    request.Messages.Add(converted);
                }
            }
        }

        if (options is not null)
        {
            // Every sampling knob the request type carries is forwarded. A knob dropped here is
            // silent — the caller sees no error, only provider-default sampling — so the omission
            // surfaces as "the model ignores my temperature" rather than as a failure. Providers
            // that cannot accept one of these drop it at their own adapter (Anthropic rejects
            // temperature/top_p/top_k on newer models), which is where that judgment belongs.
            request.MaxTokens = options.MaxOutputTokens;
            request.Temperature = options.Temperature;
            request.TopP = options.TopP;
            request.TopK = options.TopK;
            request.StopSequences = options.StopSequences?.ToList();

            if (options.Tools is { Count: > 0 })
            {
                var adapted = options.Tools.Select(t => (ITool)new AIToolAdapter(t));
                request.Tools = new ToolCollection(adapted);
            }

            request.ToolChoice = ToToolChoice(options.ToolMode);
        }

        return request;
    }

    /// <summary>
    /// M.E.AI의 <see cref="ChatToolMode"/>를 IronHive의 <see cref="ToolChoice"/>로 변환합니다.
    /// null(미설정)은 null로 유지합니다 — 모델이 자유롭게 결정하는 기본 동작을 그대로 보존합니다.
    /// </summary>
    internal static ToolChoice? ToToolChoice(ChatToolMode? toolMode) => toolMode switch
    {
        null => null,
        NoneChatToolMode => ToolChoice.None,
        RequiredChatToolMode { RequiredFunctionName: { } name } => ToolChoice.Function(name),
        RequiredChatToolMode => ToolChoice.Required,
        _ => ToolChoice.Auto
    };

    private static Message? ConvertMessage(ChatMessage message, Dictionary<string, ToolOutput> toolResults)
    {
        if (message.Role == ChatRole.User)
        {
            var Message = new Message { Role = MessageRole.User };

            foreach (var content in message.Contents)
            {
                if (content is TextContent textContent)
                {
                    Message.Content.Add(new TextMessageContent
                    {
                        Value = textContent.Text ?? string.Empty
                    });
                }
                else if (content is DataContent dataContent
                    && dataContent.MediaType?.StartsWith("image/", StringComparison.Ordinal) == true)
                {
                    Message.Content.Add(new ImageMessageContent
                    {
                        Format = GetImageFormat(dataContent.MediaType),
                        Base64 = Convert.ToBase64String(dataContent.Data.ToArray())
                    });
                }
                else if (content is DataContent audioContent
                    && audioContent.MediaType?.StartsWith("audio/", StringComparison.Ordinal) == true)
                {
                    Message.Content.Add(new AudioMessageContent
                    {
                        Format = GetAudioFormat(audioContent.MediaType),
                        Base64 = Convert.ToBase64String(audioContent.Data.ToArray())
                    });
                }
            }

            if (Message.Content.Count == 0 && !string.IsNullOrEmpty(message.Text))
            {
                Message.Content.Add(new TextMessageContent
                {
                    Value = message.Text
                });
            }

            return Message.Content.Count > 0 ? Message : null;
        }
        else if (message.Role == ChatRole.Assistant)
        {
            var Message = new Message { Role = MessageRole.Assistant };

            foreach (var content in message.Contents)
            {
                switch (content)
                {
                    case TextContent textContent:
                        Message.Content.Add(new TextMessageContent
                        {
                            Value = textContent.Text ?? string.Empty
                        });
                        break;

                    case FunctionCallContent functionCall:
                        var callId = functionCall.CallId ?? Guid.NewGuid().ToString();
                        var toolMsg = new ToolMessageContent
                        {
                            Id = callId,
                            Name = functionCall.Name,
                            Input = functionCall.Arguments is not null
                                ? JsonSerializer.Serialize(functionCall.Arguments)
                                : "{}",
                            IsApproved = true,
                            // Replay the provider's signature (Gemini 3 thought_signature and the like): the
                            // provider refuses a played-back functionCall that lost it (#326).
                            Signature = functionCall.AdditionalProperties?.TryGetValue(SignatureKey, out var signature) == true
                                ? signature as string
                                : null
                        };

                        if (toolResults.TryGetValue(callId, out var result))
                        {
                            toolMsg.Output = result;
                        }

                        Message.Content.Add(toolMsg);
                        break;
                }
            }

            if (Message.Content.Count == 0 && !string.IsNullOrEmpty(message.Text))
            {
                Message.Content.Add(new TextMessageContent
                {
                    Value = message.Text
                });
            }

            return Message;
        }

        // Skip ChatRole.Tool messages — results are merged into assistant messages above
        return null;
    }

    private ChatResponse ConvertToResponse(MessageResponse response)
    {
        var contents = new List<AIContent>();

        foreach (var content in response.Message?.Content ?? [])
        {
            if (content is TextMessageContent textContent)
            {
                contents.Add(new TextContent(textContent.Value));
            }
            else if (content is ToolMessageContent toolContent)
            {
                var args = ParseToolArguments(toolContent.Input);
                contents.Add(WithSignature(new FunctionCallContent(
                    toolContent.Id,
                    toolContent.Name,
                    args), toolContent.Signature));
            }
        }

        var chatMessage = new ChatMessage(ChatRole.Assistant, contents);

        return new ChatResponse(chatMessage)
        {
            ResponseId = response.ResponseId,
            CreatedAt = response.Timestamp,
            FinishReason = ConvertDoneReason(response.DoneReason),
            ModelId = response.Model ?? _modelId,
            Usage = response.TokenUsage != null ? new UsageDetails
            {
                InputTokenCount = response.TokenUsage.InputTokens,
                OutputTokenCount = response.TokenUsage.OutputTokens,
                TotalTokenCount = response.TokenUsage.TotalTokens
            } : null
        };
    }

    private static ChatResponseUpdate? ConvertToStreamingUpdate(StreamingMessageResponse chunk)
    {
        switch (chunk)
        {
            case StreamingMessageBeginResponse:
                return null;

            case StreamingContentDeltaResponse delta:
                var contents = new List<AIContent>();
                AdditionalPropertiesDictionary? updateProps = null;
                if (delta.Delta is TextDeltaContent textDelta)
                {
                    contents.Add(new TextContent(textDelta.Value));
                }
                else if (delta.Delta is ThinkingDeltaContent thinkingDelta)
                {
                    // Signal thinking content via AdditionalProperties so ThinkingAgentLoop picks it up.
                    // Key matches IndexThinking.Client.ThinkingChatClient.ThinkingContentKey.
                    updateProps = new AdditionalPropertiesDictionary
                    {
                        ["IndexThinking.ThinkingContent"] = thinkingDelta.Data
                    };
                }
                return new ChatResponseUpdate
                {
                    ResponseId = null,
                    Contents = contents,
                    AdditionalProperties = updateProps
                };

            case StreamingMessageDoneResponse done:
                return new ChatResponseUpdate
                {
                    ResponseId = done.ResponseId,
                    CreatedAt = done.Timestamp,
                    FinishReason = ConvertDoneReason(done.DoneReason),
                    ModelId = done.Model
                };

            default:
                return null;
        }
    }

    private static ChatFinishReason? ConvertDoneReason(MessageDoneReason? reason)
    {
        return reason switch
        {
            MessageDoneReason.EndTurn => ChatFinishReason.Stop,
            MessageDoneReason.StopSequence => ChatFinishReason.Stop,
            MessageDoneReason.MaxTokens => ChatFinishReason.Length,
            MessageDoneReason.ToolCall => ChatFinishReason.ToolCalls,
            MessageDoneReason.ContentFilter => ChatFinishReason.ContentFilter,
            _ => null
        };
    }

    /// <summary>
    /// Translates a M.E.AI tool result into IronHive's structured tool output. Content the tool
    /// returned as <see cref="AIContent"/> keeps its structure — text as text, images as images — so
    /// a provider that carries image tool results natively receives the image; any other value keeps
    /// its text form, as before. An exception recorded on the result marks the output a failure,
    /// which is what lets providers with an error flag set it. The text shown for a failure is the
    /// <see cref="FunctionResultContent.Result"/> the invoker chose, not the exception's message:
    /// whether error details reach the model is the invoker's setting, not this bridge's.
    /// </summary>
    internal static ToolOutput ToToolOutput(FunctionResultContent result)
    {
        if (result.Exception is not null)
        {
            return ToolOutput.Failure(result.Result?.ToString() ?? string.Empty);
        }

        return result.Result switch
        {
            AIContent single => ToolOutput.Success(ToToolResultContent([single])),
            IEnumerable<AIContent> many => ToolOutput.Success(ToToolResultContent(many)),
            var other => ToolOutput.Success(other?.ToString() ?? string.Empty)
        };
    }

    private static List<MessageContent> ToToolResultContent(IEnumerable<AIContent> contents)
    {
        var converted = new List<MessageContent>();
        foreach (var content in contents)
        {
            switch (content)
            {
                case TextContent text:
                    converted.Add(new TextMessageContent { Value = text.Text ?? string.Empty });
                    break;

                case DataContent data when data.MediaType?.StartsWith("image/", StringComparison.Ordinal) == true:
                    converted.Add(new ImageMessageContent
                    {
                        Format = GetImageFormat(data.MediaType),
                        Base64 = Convert.ToBase64String(data.Data.ToArray())
                    });
                    break;

                default:
                    // A block this bridge cannot carry is named rather than dropped: a tool that
                    // returned something and a tool that returned nothing must not look the same
                    // to the model.
                    var kind = content is DataContent other ? other.MediaType : content.GetType().Name;
                    converted.Add(new TextMessageContent { Value = $"[tool returned {kind} content, which cannot be forwarded]" });
                    break;
            }
        }

        return converted;
    }

    private static ImageFormat GetImageFormat(string? mediaType)
    {
        return mediaType?.ToLowerInvariant() switch
        {
            "image/png" => ImageFormat.Png,
            "image/gif" => ImageFormat.Gif,
            "image/webp" => ImageFormat.Webp,
            _ => ImageFormat.Jpeg
        };
    }

    private static AudioFormat GetAudioFormat(string? mediaType)
    {
        return mediaType?.ToLowerInvariant() switch
        {
            "audio/wav" or "audio/x-wav" => AudioFormat.Wav,
            "audio/flac" or "audio/x-flac" => AudioFormat.Flac,
            "audio/aac" => AudioFormat.Aac,
            "audio/ogg" => AudioFormat.Ogg,
            "audio/aiff" or "audio/x-aiff" => AudioFormat.Aiff,
            _ => AudioFormat.Mp3
        };
    }

    // Some local LLMs (e.g. Gemma 4 E4B) emit non-object JSON for tool-call arguments
    // (`[]`, `[null]`, scalars). Treat any non-object root as "no args" rather than
    // throwing JsonException mid-stream. See Filer issue 2026-04-28.
    /// <summary>
    /// provider 서명을 <see cref="SignatureKey"/>로 <c>FunctionCallContent.AdditionalProperties</c>에
    /// 실어, 소비자가 이 콘텐츠를 히스토리에 그대로 되돌려 보내면 요청 측(<see cref="ConvertToRequest"/>)이
    /// <c>ToolMessageContent.Signature</c>로 복원합니다. 서명이 없으면 아무것도 붙이지 않습니다.
    /// </summary>
    private static FunctionCallContent WithSignature(FunctionCallContent call, string? signature)
    {
        if (string.IsNullOrEmpty(signature))
            return call;

        (call.AdditionalProperties ??= new AdditionalPropertiesDictionary())[SignatureKey] = signature;
        return call;
    }

    private static Dictionary<string, object?>? ParseToolArguments(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        JsonElement root;
        try
        {
            root = JsonSerializer.Deserialize<JsonElement>(json);
        }
        catch (JsonException)
        {
            return null;
        }

        if (root.ValueKind != JsonValueKind.Object)
            return null;

        var arguments = new Dictionary<string, object?>();
        foreach (var prop in root.EnumerateObject())
        {
            arguments[prop.Name] = ConvertJsonElement(prop.Value);
        }
        return arguments;
    }

    private static object? ConvertJsonElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        _ => element
    };
}

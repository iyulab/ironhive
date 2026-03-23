using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;

namespace IronHive.Core.Compatibility;

/// <summary>
/// IronHive IMessageGenerator를 Microsoft.Extensions.AI IChatClient로 래핑하는 어댑터입니다.
/// </summary>
public class ChatClientAdapter : IChatClient
{
    private readonly IMessageGenerator _generator;
    private readonly string _modelId;
    private readonly string _providerName;

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
        string? completionId = null;

        // Buffer tool calls: index -> (callId, name, argumentsJson)
        var toolCallBuffers = new Dictionary<int, (string CallId, string Name, StringBuilder Arguments)>();

        await foreach (var chunk in _generator.GenerateStreamingMessageAsync(request, cancellationToken)
            .ConfigureAwait(false))
        {
            switch (chunk)
            {
                case StreamingContentAddedResponse added when added.Content is ToolMessageContent tool:
                    toolCallBuffers[added.Index] = (
                        tool.Id ?? Guid.NewGuid().ToString(),
                        tool.Name ?? string.Empty,
                        new StringBuilder());
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
                        var argsJson = completedTool.Arguments.ToString();
                        Dictionary<string, object?>? arguments = null;
                        if (!string.IsNullOrEmpty(argsJson))
                        {
                            var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(argsJson);
                            if (raw is not null)
                            {
                                arguments = [];
                                foreach (var kvp in raw)
                                {
                                    arguments[kvp.Key] = ConvertJsonElement(kvp.Value);
                                }
                            }
                        }

                        yield return new ChatResponseUpdate
                        {
                            ResponseId = completionId,
                            Contents = [new FunctionCallContent(
                                callId: completedTool.CallId,
                                name: completedTool.Name,
                                arguments: arguments)]
                        };
                        toolCallBuffers.Remove(completed.Index);
                    }
                    break;

                case StreamingMessageErrorResponse error:
                    throw new InvalidOperationException(
                        $"Streaming error: {error.Code} - {error.Message}");

                default:
                    var update = ConvertToStreamingUpdate(chunk, ref completionId);
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
        var toolResults = new Dictionary<string, string>();
        foreach (var msg in chatMessages)
        {
            foreach (var content in msg.Contents)
            {
                if (content is FunctionResultContent result && result.CallId is not null)
                {
                    toolResults[result.CallId] = result.Result?.ToString() ?? string.Empty;
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
            request.Temperature = options.Temperature;
            request.MaxTokens = options.MaxOutputTokens;
            request.TopP = options.TopP;
            request.StopSequences = options.StopSequences?.ToList();

            if (options.Tools is { Count: > 0 })
            {
                var adapted = options.Tools.Select(t => (ITool)new AIToolAdapter(t));
                request.Tools = new ToolCollection(adapted);
            }
        }

        return request;
    }

    private static Message? ConvertMessage(ChatMessage message, Dictionary<string, string> toolResults)
    {
        if (message.Role == ChatRole.User)
        {
            var userMessage = new UserMessage();

            foreach (var content in message.Contents)
            {
                if (content is TextContent textContent)
                {
                    userMessage.Content.Add(new TextMessageContent
                    {
                        Value = textContent.Text ?? string.Empty
                    });
                }
                else if (content is DataContent dataContent
                    && dataContent.MediaType?.StartsWith("image/", StringComparison.Ordinal) == true)
                {
                    userMessage.Content.Add(new ImageMessageContent
                    {
                        Format = GetImageFormat(dataContent.MediaType),
                        Base64 = Convert.ToBase64String(dataContent.Data.ToArray())
                    });
                }
            }

            if (userMessage.Content.Count == 0 && !string.IsNullOrEmpty(message.Text))
            {
                userMessage.Content.Add(new TextMessageContent
                {
                    Value = message.Text
                });
            }

            return userMessage.Content.Count > 0 ? userMessage : null;
        }
        else if (message.Role == ChatRole.Assistant)
        {
            var assistantMessage = new AssistantMessage();

            foreach (var content in message.Contents)
            {
                switch (content)
                {
                    case TextContent textContent:
                        assistantMessage.Content.Add(new TextMessageContent
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
                            IsApproved = true
                        };

                        if (toolResults.TryGetValue(callId, out var result))
                        {
                            toolMsg.Output = new ToolOutput(true, result);
                        }

                        assistantMessage.Content.Add(toolMsg);
                        break;
                }
            }

            if (assistantMessage.Content.Count == 0 && !string.IsNullOrEmpty(message.Text))
            {
                assistantMessage.Content.Add(new TextMessageContent
                {
                    Value = message.Text
                });
            }

            return assistantMessage;
        }

        // Skip ChatRole.Tool messages — results are merged into assistant messages above
        return null;
    }

    private ChatResponse ConvertToResponse(MessageResponse response)
    {
        var contents = new List<AIContent>();

        foreach (var content in response.Message.Content)
        {
            if (content is TextMessageContent textContent)
            {
                contents.Add(new TextContent(textContent.Value));
            }
            else if (content is ToolMessageContent toolContent)
            {
                Dictionary<string, object?>? arguments = null;
                if (!string.IsNullOrEmpty(toolContent.Input))
                {
                    var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(toolContent.Input);
                    if (raw is not null)
                    {
                        arguments = [];
                        foreach (var kvp in raw)
                        {
                            arguments[kvp.Key] = ConvertJsonElement(kvp.Value);
                        }
                    }
                }

                contents.Add(new FunctionCallContent(
                    toolContent.Id,
                    toolContent.Name,
                    arguments));
            }
        }

        var chatMessage = new ChatMessage(ChatRole.Assistant, contents);

        return new ChatResponse(chatMessage)
        {
            ResponseId = response.Id,
            CreatedAt = response.Timestamp,
            FinishReason = ConvertDoneReason(response.DoneReason),
            ModelId = response.Message.Model ?? _modelId,
            Usage = response.TokenUsage != null ? new UsageDetails
            {
                InputTokenCount = response.TokenUsage.InputTokens,
                OutputTokenCount = response.TokenUsage.OutputTokens,
                TotalTokenCount = response.TokenUsage.TotalTokens
            } : null
        };
    }

    private static ChatResponseUpdate? ConvertToStreamingUpdate(
        StreamingMessageResponse chunk,
        ref string? completionId)
    {
        switch (chunk)
        {
            case StreamingMessageBeginResponse begin:
                completionId = begin.Id;
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
                    ResponseId = completionId,
                    Contents = contents,
                    AdditionalProperties = updateProps
                };

            case StreamingMessageDoneResponse done:
                return new ChatResponseUpdate
                {
                    ResponseId = done.Id,
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

using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;

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
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var messageList = chatMessages.ToList();
        var request = ConvertToRequest(messageList, options);
        var response = await _generator.GenerateMessageAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return ConvertToResponse(response);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messageList = chatMessages.ToList();
        var request = ConvertToRequest(messageList, options);
        string? completionId = null;

        await foreach (var chunk in _generator.GenerateStreamingMessageAsync(request, cancellationToken)
            .ConfigureAwait(false))
        {
            var update = ConvertToStreamingUpdate(chunk, ref completionId);
            if (update != null)
                yield return update;
        }
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? key = null)
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
            Messages = [],
            MaxTokens = options?.MaxOutputTokens,
            Temperature = options?.Temperature,
            TopP = options?.TopP,
            StopSequences = options?.StopSequences?.ToList()
        };

        foreach (var msg in chatMessages)
        {
            if (msg.Role == ChatRole.System)
            {
                request.SystemPrompt = msg.Text;
            }
            else
            {
                request.Messages.Add(ConvertMessage(msg));
            }
        }

        return request;
    }

    private static Message ConvertMessage(ChatMessage message)
    {
        if (message.Role == ChatRole.User)
        {
            var userMessage = new UserMessage();
            foreach (var content in message.Contents)
            {
                if (content is TextContent textContent)
                {
                    userMessage.Content.Add(new TextMessageContent { Value = textContent.Text ?? string.Empty });
                }
                else if (content is DataContent dataContent && dataContent.MediaType?.StartsWith("image/") == true)
                {
                    userMessage.Content.Add(new ImageMessageContent
                    {
                        Format = GetImageFormat(dataContent.MediaType),
                        Base64 = Convert.ToBase64String(dataContent.Data.ToArray())
                    });
                }
            }
            return userMessage;
        }
        else // Assistant
        {
            var assistantMessage = new AssistantMessage();
            foreach (var content in message.Contents)
            {
                if (content is TextContent textContent)
                {
                    assistantMessage.Content.Add(new TextMessageContent { Value = textContent.Text ?? string.Empty });
                }
                else if (content is FunctionCallContent functionCall)
                {
                    assistantMessage.Content.Add(new ToolMessageContent
                    {
                        IsApproved = true,
                        Id = functionCall.CallId ?? Guid.NewGuid().ToString(),
                        Name = functionCall.Name,
                        Input = functionCall.Arguments?.ToString()
                    });
                }
            }
            return assistantMessage;
        }
    }

    private static ChatResponse ConvertToResponse(MessageResponse response)
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
                contents.Add(new FunctionCallContent(
                    toolContent.Id,
                    toolContent.Name,
                    toolContent.Input != null
                        ? new Dictionary<string, object?> { ["input"] = toolContent.Input }
                        : null));
            }
        }

        var chatMessage = new ChatMessage(ChatRole.Assistant, contents);

        return new ChatResponse(chatMessage)
        {
            ResponseId = response.Id,
            CreatedAt = response.Timestamp,
            FinishReason = ConvertDoneReason(response.DoneReason),
            ModelId = response.Message.Model,
            Usage = response.TokenUsage != null ? new UsageDetails
            {
                InputTokenCount = response.TokenUsage.InputTokens,
                OutputTokenCount = response.TokenUsage.OutputTokens,
                TotalTokenCount = response.TokenUsage.InputTokens + response.TokenUsage.OutputTokens
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
                if (delta.Delta is TextDeltaContent textDelta)
                {
                    contents.Add(new TextContent(textDelta.Value));
                }
                return new ChatResponseUpdate
                {
                    ResponseId = completionId,
                    Contents = contents
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
}

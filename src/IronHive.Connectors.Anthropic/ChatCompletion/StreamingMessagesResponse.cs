using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PingEvent), "ping")]
[JsonDerivedType(typeof(ErrorEvent), "error")]
[JsonDerivedType(typeof(MessageStartEvent), "message_start")]
[JsonDerivedType(typeof(MessageDeltaEvent), "message_delta")]
[JsonDerivedType(typeof(MessageStopEvent), "message_stop")]
[JsonDerivedType(typeof(ContentStartEvent), "content_block_start")]
[JsonDerivedType(typeof(ContentDeltaEvent), "content_block_delta")]
[JsonDerivedType(typeof(ContentStopEvent), "content_block_stop")]
internal abstract class StreamingMessagesResponse 
{ }

internal class PingEvent : StreamingMessagesResponse 
{ }

internal class ErrorEvent : StreamingMessagesResponse
{
    [JsonPropertyName("error")]
    public required object Error { get; set; }
}

internal class MessageStartEvent : StreamingMessagesResponse
{
    [JsonPropertyName("message")]
    public MessagesResponse? Message { get; set; }
}

internal class MessageDeltaEvent : StreamingMessagesResponse
{
    [JsonPropertyName("delta")]
    public MessageDelta? Delta { get; set; }

    [JsonPropertyName("usage")]
    public TokenUsage? Usage { get; set; }

    internal class MessageDelta
    {
        [JsonPropertyName("stop_reason")]
        public StopReason? StopReason { get; set; }

        [JsonPropertyName("stop_sequence")]
        public string? StopSequence { get; set; }
    }
}

internal class MessageStopEvent : StreamingMessagesResponse 
{ }

internal class ContentStartEvent : StreamingMessagesResponse
{
    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("content_block")]
    public IMessageContent? ContentBlock { get; set; }
}

internal class ContentDeltaEvent : StreamingMessagesResponse
{
    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("delta")]
    public IMessageDeltaContent? Delta { get; set; }
}

internal class ContentStopEvent : StreamingMessagesResponse
{
    [JsonPropertyName("index")]
    public int? Index { get; set; }
}

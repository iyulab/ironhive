using System.Text.Json.Serialization;

namespace Raggle.Engines.Anthropic.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PingEvent), "ping")]
[JsonDerivedType(typeof(ErrorEvent), "error")]
[JsonDerivedType(typeof(MessageStartEvent), "message_start")]
[JsonDerivedType(typeof(MessageStopEvent), "message_stop")]
[JsonDerivedType(typeof(MessageDeltaEvent), "message_delta")]
[JsonDerivedType(typeof(ContentStartEvent), "content_block_start")]
[JsonDerivedType(typeof(ContentDeltaEvent), "content_block_delta")]
[JsonDerivedType(typeof(ContentStopEvent), "content_block_stop")]
public abstract class StreamingMessagesResponse { }

public class PingEvent : StreamingMessagesResponse { }

public class ErrorEvent : StreamingMessagesResponse 
{
    [JsonPropertyName("error")]
    public required object Error { get; set; }
}

public class MessageStartEvent : StreamingMessagesResponse
{
    [JsonPropertyName("message")]
    public MessagesResponse? Message { get; set; }
}

public class MessageStopEvent : StreamingMessagesResponse { }

public class MessageDeltaEvent : StreamingMessagesResponse
{
    [JsonPropertyName("delta")]
    public MessageDeltaContent? Delta { get; set; }

    [JsonPropertyName("usage")]
    public TokenUsage? Usage { get; set; }
}

public class MessageDeltaContent
{
    [JsonPropertyName("stop_reason")]
    public StopReason? StopReason { get; set; }

    [JsonPropertyName("stop_sequence")]
    public string? StopSequence { get; set; }
}

public class ContentStartEvent : StreamingMessagesResponse
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// <see cref="MessageTextContent"/> or <see cref="MessageToolUseContent"/>
    /// </summary>
    [JsonPropertyName("content_block")]
    public MessageContent? ContentBlock { get; set; }
}

public class ContentDeltaEvent : StreamingMessagesResponse
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// <see cref="MessageTextDeltaContent"/> or <see cref="MessageToolUseDeltaContent"/>
    /// </summary>
    [JsonPropertyName("delta")]
    public MessageContent? ContentBlock { get; set; }
}

public class ContentStopEvent : StreamingMessagesResponse
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
}

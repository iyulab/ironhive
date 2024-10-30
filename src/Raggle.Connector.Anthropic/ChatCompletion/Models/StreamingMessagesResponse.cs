using System.Text.Json.Serialization;

namespace Raggle.Connector.Anthropic.ChatCompletion.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PingEvent), "ping")]
[JsonDerivedType(typeof(ErrorEvent), "error")]
[JsonDerivedType(typeof(MessageStartEvent), "message_start")]
[JsonDerivedType(typeof(MessageStopEvent), "message_stop")]
[JsonDerivedType(typeof(MessageDeltaEvent), "message_delta")]
[JsonDerivedType(typeof(ContentStartEvent), "content_block_start")]
[JsonDerivedType(typeof(ContentDeltaEvent), "content_block_delta")]
[JsonDerivedType(typeof(ContentStopEvent), "content_block_stop")]
internal abstract class StreamingMessagesResponse { }

internal class PingEvent : StreamingMessagesResponse { }

internal class ErrorEvent : StreamingMessagesResponse
{
    [JsonPropertyName("error")]
    internal required object Error { get; set; }
}

internal class MessageStartEvent : StreamingMessagesResponse
{
    [JsonPropertyName("message")]
    internal MessagesResponse? Message { get; set; }
}

internal class MessageStopEvent : StreamingMessagesResponse { }

internal class MessageDeltaEvent : StreamingMessagesResponse
{
    [JsonPropertyName("delta")]
    internal MessageDeltaContent? Delta { get; set; }

    [JsonPropertyName("usage")]
    internal TokenUsage? Usage { get; set; }
}

internal class MessageDeltaContent
{
    [JsonPropertyName("stop_reason")]
    internal StopReason? StopReason { get; set; }

    [JsonPropertyName("stop_sequence")]
    internal string? StopSequence { get; set; }
}

internal class ContentStartEvent : StreamingMessagesResponse
{
    [JsonPropertyName("index")]
    internal int Index { get; set; }

    /// <summary>
    /// <see cref="MessageTextContent"/> or <see cref="MessageToolUseContent"/>
    /// </summary>
    [JsonPropertyName("content_block")]
    internal MessageContent? ContentBlock { get; set; }
}

internal class ContentDeltaEvent : StreamingMessagesResponse
{
    [JsonPropertyName("index")]
    internal int Index { get; set; }

    /// <summary>
    /// <see cref="MessageTextDeltaContent"/> or <see cref="MessageToolUseDeltaContent"/>
    /// </summary>
    [JsonPropertyName("delta")]
    internal MessageContent? ContentBlock { get; set; }
}

internal class ContentStopEvent : StreamingMessagesResponse
{
    [JsonPropertyName("index")]
    internal int Index { get; set; }
}

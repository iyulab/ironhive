using System.Text.Json.Serialization;

namespace Raggle.Connector.Anthropic.ChatCompletion.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMessageContent), "text")]
[JsonDerivedType(typeof(TextDeltaMessageContent), "text_delta")]
[JsonDerivedType(typeof(ImageMessageContent), "image")]
[JsonDerivedType(typeof(ToolUseMessageContent), "tool_use")]
[JsonDerivedType(typeof(ToolUseDeltaMessageContent), "input_json_delta")]
[JsonDerivedType(typeof(ToolResultMessageContent), "tool_result")]
internal abstract class MessageContent
{
    [JsonPropertyName("cache_control")]
    internal CacheControl? CacheControl { get; set; }
}

internal class TextMessageContent : MessageContent
{
    [JsonPropertyName("text")]
    internal required string Text { get; set; }
}

internal class TextDeltaMessageContent : MessageContent
{
    [JsonPropertyName("text")]
    internal required string Text { get; set; }
}

internal class ImageMessageContent : MessageContent
{
    [JsonPropertyName("source")]
    internal required ImageSource Source { get; set; }
}

internal class ImageSource
{
    /// <summary>
    /// "base64" only
    /// </summary>
    [JsonPropertyName("type")]
    internal string Type { get; } = "base64";

    /// <summary>
    /// "image/jpeg", "image/png", "image/gif", "image/webp" 
    /// </summary>
    [JsonPropertyName("media_type")]
    internal required string MediaType { get; set; }

    /// <summary>
    /// base64 raw data
    /// </summary>
    [JsonPropertyName("data")]
    internal required string Data { get; set; }
}

internal class ToolUseMessageContent : MessageContent
{
    [JsonPropertyName("id")]
    internal string? ID { get; set; }

    [JsonPropertyName("name")]
    internal string? Name { get; set; }

    [JsonPropertyName("input")]
    internal object? Input { get; set; }
}

internal class ToolUseDeltaMessageContent : MessageContent
{
    [JsonPropertyName("partial_json")]
    internal required string PartialJson { get; set; }
}

internal class ToolResultMessageContent : MessageContent
{
    [JsonPropertyName("tool_use_id")]
    internal string? ToolUseID { get; set; }

    [JsonPropertyName("is_error")]
    internal bool IsError { get; set; } = false;

    /// <summary>
    /// <see cref="MessageTextContent"/> or <see cref="MessageImageContent"/>
    /// </summary>
    [JsonPropertyName("content")]
    internal MessageContent[] Content { get; set; } = [];
}

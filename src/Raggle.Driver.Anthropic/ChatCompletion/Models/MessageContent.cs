using System.Text.Json.Serialization;

namespace Raggle.Driver.Anthropic.ChatCompletion.Models;

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
    public CacheControl? CacheControl { get; set; }
}

internal class TextMessageContent : MessageContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

internal class TextDeltaMessageContent : MessageContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

internal class ImageMessageContent : MessageContent
{
    [JsonPropertyName("source")]
    public required ImageSource Source { get; set; }
}

internal class ImageSource
{
    /// <summary>
    /// "base64" only
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "base64";

    /// <summary>
    /// "image/jpeg", "image/png", "image/gif", "image/webp" 
    /// </summary>
    [JsonPropertyName("media_type")]
    public required string MediaType { get; set; }

    /// <summary>
    /// base64 raw data
    /// </summary>
    [JsonPropertyName("data")]
    public required string Data { get; set; }
}

internal class ToolUseMessageContent : MessageContent
{
    [JsonPropertyName("id")]
    public string? ID { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("input")]
    public object? Input { get; set; }
}

internal class ToolUseDeltaMessageContent : MessageContent
{
    [JsonPropertyName("partial_json")]
    public required string PartialJson { get; set; }
}

internal class ToolResultMessageContent : MessageContent
{
    [JsonPropertyName("tool_use_id")]
    public string? ToolUseID { get; set; }

    [JsonPropertyName("is_error")]
    public bool IsError { get; set; } = false;

    /// <summary>
    /// <see cref="MessageTextContent"/> or <see cref="MessageImageContent"/>
    /// </summary>
    [JsonPropertyName("content")]
    public MessageContent[] Content { get; set; } = [];
}

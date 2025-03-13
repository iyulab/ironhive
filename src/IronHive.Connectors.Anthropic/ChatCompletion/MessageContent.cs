using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMessageContent), "text")]
[JsonDerivedType(typeof(ImageMessageContent), "image")]
[JsonDerivedType(typeof(ToolUseMessageContent), "tool_use")]
[JsonDerivedType(typeof(ToolResultMessageContent), "tool_result")]
[JsonDerivedType(typeof(TextDeltaMessageContent), "text_delta")]
[JsonDerivedType(typeof(ToolUseDeltaMessageContent), "input_json_delta")]
internal abstract class MessageContent
{
    [JsonPropertyName("cache_control")]
    public CacheControl? CacheControl { get; set; }
}

internal class TextMessageContent : MessageContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("ciations")]
    public object? Citations { get; set; }
}

internal class ImageMessageContent : MessageContent
{
    [JsonPropertyName("source")]
    public required ImageSource Source { get; set; }
}

internal class ToolUseMessageContent : MessageContent
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("input")]
    public object? Input { get; set; }
}

internal class ToolResultMessageContent : MessageContent
{
    [JsonPropertyName("tool_use_id")]
    public string? ToolUseId { get; set; }

    [JsonPropertyName("is_error")]
    public bool IsError { get; set; } = false;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

internal class TextDeltaMessageContent : MessageContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

internal class ToolUseDeltaMessageContent : MessageContent
{
    [JsonPropertyName("partial_json")]
    public required string PartialJson { get; set; }
}

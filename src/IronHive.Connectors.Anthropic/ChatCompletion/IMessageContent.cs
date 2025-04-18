using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMessageContent), "text")]
[JsonDerivedType(typeof(ImageMessageContent), "image")]
[JsonDerivedType(typeof(ToolUseMessageContent), "tool_use")]
[JsonDerivedType(typeof(ToolResultMessageContent), "tool_result")]
[JsonDerivedType(typeof(DocumentMessageContent), "document")]
[JsonDerivedType(typeof(ThinkingMessageContent), "thinking")]
[JsonDerivedType(typeof(RedactedThinkingMessageContent), "redacted_thinking")]
internal interface IMessageContent
{ }

internal abstract class MessageContentBase : IMessageContent
{
    [JsonPropertyName("cache_control")]
    public CacheControl? CacheControl { get; set; }
}

internal class TextMessageContent : MessageContentBase
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("ciations")]
    public object? Citations { get; set; }
}

internal class ImageMessageContent : MessageContentBase
{
    [JsonPropertyName("source")]
    public required ImageSource Source { get; set; }
}

internal class ToolUseMessageContent : MessageContentBase
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("input")]
    public object? Input { get; set; }
}

internal class ToolResultMessageContent : MessageContentBase
{
    [JsonPropertyName("tool_use_id")]
    public string? ToolUseId { get; set; }

    [JsonPropertyName("is_error")]
    public bool IsError { get; set; } = false;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

internal class DocumentMessageContent : MessageContentBase
{
    [JsonPropertyName("source")]
    public object? Source { get; set; }

    [JsonPropertyName("citations")]
    public object? Citations { get; set; }

    [JsonPropertyName("context")]
    public string? Context { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

internal class ThinkingMessageContent : IMessageContent
{
    [JsonPropertyName("signature")]
    public required string Signature { get; set; }

    [JsonPropertyName("thinking")]
    public required string Thinking { get; set; }
}

internal class RedactedThinkingMessageContent : IMessageContent
{
    [JsonPropertyName("data")]
    public required string Data { get; set; }
}

using System.Text.Json.Serialization;

namespace Raggle.Engines.Anthropic.ChatCompletion;

public class Message
{
    /// <summary>
    /// "user", "assistant" 
    /// </summary>
    [JsonPropertyName("role")]
    public required string Role { get; set; }

    // string or MessageContent[]
    [JsonPropertyName("content")]
    public required ICollection<MessageContent> Content { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(MessageTextContent), "text")]
[JsonDerivedType(typeof(MessageImageContent), "image")]
[JsonDerivedType(typeof(MessageToolUseContent), "tool_use")]
[JsonDerivedType(typeof(MessageToolResultContent), "tool_result")]
public abstract class MessageContent
{
    [JsonPropertyName("cache_control")]
    public object? CacheControl { get; set; }
}

public class MessageTextContent : MessageContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; } = string.Empty;
}

public class MessageImageContent : MessageContent
{
    [JsonPropertyName("source")]
    public required ImageSource Source { get; set; }
}

public class MessageToolUseContent : MessageContent
{
    [JsonPropertyName("id")]
    public required string ID { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("input")]
    public required object Input { get; set; }
}

public class MessageToolResultContent : MessageContent
{
    [JsonPropertyName("tool_use_id")]
    public required string ToolUseID { get; set; }

    [JsonPropertyName("is_error")]
    public required bool IsError { get; set; }

    [JsonPropertyName("content")]
    public object? Content { get; set; }
}

public class ImageSource
{
    [JsonPropertyName("type")]
    public string Type { get; } = "base64";

    /// <summary>
    /// image/jpeg, image/png, image/gif, image/webp 
    /// </summary>
    [JsonPropertyName("media_type")]
    public required string MediaType { get; set; }

    [JsonPropertyName("data")]
    public required string Data { get; set; }
}
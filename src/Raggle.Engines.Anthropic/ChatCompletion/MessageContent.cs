using System.Text.Json.Serialization;

namespace Raggle.Engines.Anthropic.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMessageContent), "text")]
[JsonDerivedType(typeof(TextDeltaMessageContent), "text_delta")]
[JsonDerivedType(typeof(ImageMessageContent), "image")]
[JsonDerivedType(typeof(ToolUseMessageContent), "tool_use")]
[JsonDerivedType(typeof(ToolUseDeltaMessageContent), "input_json_delta")]
[JsonDerivedType(typeof(ToolResultMessageContent), "tool_result")]
public abstract class MessageContent
{
    [JsonPropertyName("cache_control")]
    public CacheControl? CacheControl { get; set; }
}

public class TextMessageContent : MessageContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class TextDeltaMessageContent : MessageContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class ImageMessageContent : MessageContent
{
    [JsonPropertyName("source")]
    public required ImageSource Source { get; set; }
}

public class ImageSource
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

public class ToolUseMessageContent : MessageContent
{
    [JsonPropertyName("id")]
    public required string ID { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("input")]
    public required object Input { get; set; }
}

public class ToolUseDeltaMessageContent : MessageContent
{
    [JsonPropertyName("partial_json")]
    public required string PartialJson { get; set; }
}

public class ToolResultMessageContent : MessageContent
{
    [JsonPropertyName("tool_use_id")]
    public required string ToolUseID { get; set; }

    [JsonPropertyName("is_error")]
    public bool IsError { get; set; } = false;

    /// <summary>
    /// <see cref="MessageTextContent"/> or <see cref="MessageImageContent"/>
    /// </summary>
    [JsonPropertyName("content")]
    public MessageContent[] Content { get; set; } = [];
}

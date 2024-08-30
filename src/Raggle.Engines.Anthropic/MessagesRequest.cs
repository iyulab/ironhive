using System.Text.Json.Serialization;

namespace Raggle.Engines.Anthropic;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageRole
{
    user,
    assistant
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContentType
{
    text,
    image,
}

public class MessageTextContent
{
    public ContentType Type => ContentType.text;
    public string text { get; set; }
}

public class MessageImageContent
{
    public ContentType Type => ContentType.image;
    public ImageSource Source { get; set; }
}

public class ImageSource
{
    public string Type { get; set; } = "base64";
    public string MediaType { get; set; }
    public string Data { get; set; }
}

public class Message
{
    [JsonPropertyName("role")]
    public MessageRole Role { get; set; } // "user" or "assistant"

    [JsonPropertyName("content")]
    public object Content { get; set; } // string or MessageContent[]
}

public enum ToolChoiceType
{
    auto,
    any,
    tool
}

public class ToolChoice
{
    public ToolChoiceType Type { get; set; }
}

public class Tool
{

}

public class MessagesRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("messages")]
    public required ICollection<Message> Messages { get; set; }

    [JsonPropertyName("max_tokens")]
    public required int MaxTokens { get; set; }

    //[JsonPropertyName("stop_sequences")]
    //public string[]? StopSequences { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("system")]
    public string? System { get; set; }

    //[JsonPropertyName("temperature")]
    //public double? Temperature { get; set; }

    //[JsonPropertyName("tool_choice")]
    //public ToolChoice? ToolChoice { get; set; }

    //[JsonPropertyName("tools")]
    //public ICollection<Tool>? Tools { get; set; }

    //[JsonPropertyName("top_p")]
    //public double? TopP { get; set; }

    //[JsonPropertyName("top_k")]
    //public double? TopK { get; set; }
}

using System.Text.Json.Serialization;

namespace Raggle.Engines.OpenAI.ChatCompletion.ChatRequestObject;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(MessageTextContent), "text")]
[JsonDerivedType(typeof(MessageImageContent), "image")]
public class MessageContent { }

public class MessageTextContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class MessageImageContent
{
    [JsonPropertyName("image_url")]
    public required ImageUrl ImageUrl { get; set; }
}

public class ImageUrl
{
    // url or base64
    [JsonPropertyName("url")]
    public required string URL { get; set; }

    // "low", "high", "auto"
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
}

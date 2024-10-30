using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.ChatCompletion.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMessageContent), "text")]
[JsonDerivedType(typeof(ImageMessageContent), "image_url")]
internal class MessageContent { }

internal class TextMessageContent : MessageContent
{
    [JsonPropertyName("text")]
    internal required string Text { get; set; }
}

internal class ImageMessageContent : MessageContent
{
    [JsonPropertyName("image_url")]
    internal required ImageURL ImageURL { get; set; }
}

internal class ImageURL
{
    /// <summary>
    /// Either a URL of the image or the base64 encoded image data.
    /// </summary>
    [JsonPropertyName("url")]
    internal required string URL { get; set; }

    /// <summary>
    /// "low", "high", "auto" 
    /// </summary>
    [JsonPropertyName("detail")]
    internal string? Detail { get; set; }
}

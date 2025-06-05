using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMessageContent), "text")]
[JsonDerivedType(typeof(ImageMessageContent), "image_url")]
[JsonDerivedType(typeof(AudioMessageContent), "input_audio")]
internal class MessageContent { }

internal class TextMessageContent : MessageContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

internal class ImageMessageContent : MessageContent
{
    [JsonPropertyName("image_url")]
    public required ImageUrl ImageUrl { get; set; }
}

internal class AudioMessageContent : MessageContent
{
    [JsonPropertyName("input_audio")]
    public required InputAudio InputAudio { get; set; }
}

internal class ImageUrl
{
    /// <summary>
    /// Either a URL of the image or the base64 encoded image data.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// "low", "high", "auto", default is "auto"
    /// </summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
}

internal class InputAudio
{
    /// <summary>
    /// the base64 encoded audio data.
    /// </summary>
    [JsonPropertyName("data")]
    public required string Data { get; set; }

    /// <summary>
    /// "wav", "mp3"
    /// </summary>
    [JsonPropertyName("format")]
    public required string Format { get; set; }
}
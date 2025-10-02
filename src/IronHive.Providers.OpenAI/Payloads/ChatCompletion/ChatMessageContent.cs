using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextChatMessageContent), "text")]
[JsonDerivedType(typeof(ImageChatMessageContent), "image_url")]
[JsonDerivedType(typeof(AudioChatMessageContent), "input_audio")]
public abstract class ChatMessageContent 
{ }

public class TextChatMessageContent : ChatMessageContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class ImageChatMessageContent : ChatMessageContent
{
    [JsonPropertyName("image_url")]
    public required ImageSource ImageUrl { get; set; }

    public class ImageSource
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
}

public class AudioChatMessageContent : ChatMessageContent
{
    [JsonPropertyName("input_audio")]
    public required AudioSource InputAudio { get; set; }

    public class AudioSource
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
}

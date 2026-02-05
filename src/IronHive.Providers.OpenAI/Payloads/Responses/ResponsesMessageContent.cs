using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesInputTextContent), "input_text")]
[JsonDerivedType(typeof(ResponsesInputImageContent), "input_image")]
[JsonDerivedType(typeof(ResponsesInputFileContent), "input_file")]
[JsonDerivedType(typeof(ResponsesInputAudioContent), "input_audio")]
[JsonDerivedType(typeof(ResponsesOutputTextContent), "output_text")]
[JsonDerivedType(typeof(ResponsesRefusalContent), "refusal")]
public abstract class ResponsesMessageContent
{ }

public class ResponsesInputTextContent : ResponsesMessageContent
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class ResponsesInputImageContent : ResponsesMessageContent
{
    /// <summary>
    /// "high", "low", "auto"
    /// </summary>
    [JsonPropertyName("detail")]
    public required string Detail { get; set; }

    [JsonPropertyName("file_id")]
    public string? FileId { get; set; }

    /// <summary>
    /// image URL or base64-encoded image data
    /// </summary>
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }
}

public class ResponsesInputFileContent : ResponsesMessageContent
{
    [JsonPropertyName("file_data")]
    public string? FileData { get; set; }

    [JsonPropertyName("file_id")]
    public string? FileId { get; set; }

    [JsonPropertyName("file_url")]
    public string? FileUrl { get; set; }

    [JsonPropertyName("filename")]
    public string? Filename { get; set; }
}

public class ResponsesInputAudioContent : ResponsesMessageContent
{
    [JsonPropertyName("input_audio")]
    public required InputAudio Audio { get; set; }

    public class InputAudio
    {
        /// <summary>
        /// base64-encoded audio data
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

public class ResponsesOutputTextContent : ResponsesMessageContent
{
    [JsonPropertyName("annotations")]
    public ICollection<ResponsesAnnotation>? Annotations { get; set; }

    [JsonPropertyName("logprobs")]
    public ICollection<ResponsesLogProbs>? Logprobs { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class ResponsesRefusalContent : ResponsesMessageContent
{
    [JsonPropertyName("refusal")]
    public required string Refusal { get; set; }
}

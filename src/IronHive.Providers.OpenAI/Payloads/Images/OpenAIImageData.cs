using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Images;

public class OpenAIOutputImageData
{
    [JsonPropertyName("b64_json")]
    public string? B64Json { get; set; }

    [JsonPropertyName("revised_prompt")]
    public string? RevisedPrompt { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class OpenAIInputImageData
{
    [JsonPropertyName("file_id")]
    public string? FileId { get; set; }

    /// <summary>
    /// url or base64 string, maxlength is 20971520 (20MB)
    /// </summary>
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }
}
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Images;

/// <summary>
/// OpenAI 이미지 생성 응답
/// </summary>
public class CreateImagesResponse : OpenAIPayloadBase
{
    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("background")]
    public string? Background { get; set; }

    [JsonPropertyName("data")]
    public ICollection<OpenAIOutputImageData>? Data { get; set; }

    [JsonPropertyName("output_format")]
    public string? OutputFormat { get; set; }

    [JsonPropertyName("quality")]
    public string? Quality { get; set; }

    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("usage")]
    public ImagesTokenUsage? Usage { get; set; }
}

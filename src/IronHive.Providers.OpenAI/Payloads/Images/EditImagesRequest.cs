using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Images;

/// <summary>
/// OpenAI 이미지 편집 요청
/// </summary>
public class EditImagesRequest : OpenAIPayloadBase
{
    [JsonPropertyName("images")]
    public required ICollection<OpenAIInputImageData> Images { get; set; }

    [JsonPropertyName("prompt")]
    public required string Prompt { get; set; }

    /// <summary>
    /// only supported for gpt-image models, "transparent", "opaque", "auto"
    /// </summary>
    [JsonPropertyName("background")]
    public string? Background { get; set; }

    /// <summary>
    /// "high" or "low"
    /// </summary>
    [JsonPropertyName("input_fidelity")]
    public string? InputFidelity { get; set; }

    [JsonPropertyName("mask")]
    public OpenAIInputImageData? Mask { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// only supported for gpt-image models, "low", "auto"
    /// </summary>
    [JsonPropertyName("moderation")]
    public string? Moderation { get; set; }

    /// <summary>
    /// between 1 and 10, default is 1
    /// </summary>
    [JsonPropertyName("n")]
    public int? N { get; set; }

    /// <summary>
    /// only supported for gpt-image models, between 0 and 100, default is 100
    /// </summary>
    [JsonPropertyName("output_compression")]
    public int? OutputCompression { get; set; }

    /// <summary>
    /// only supported for gpt-image models, "png", "jpeg", "webp"
    /// </summary>
    [JsonPropertyName("output_format")]
    public string? OutputFormat { get; set; }

    /// <summary>
    /// used for streaming partial image results,
    /// between 0 and 3, default is 0
    /// </summary>
    [JsonPropertyName("partial_images")]
    public int? PartialImages { get; set; }

    /// <summary>
    /// "auto" is the default value
    /// <para>only supported for gpt-image models, "low", "medium", "high"</para>
    /// <para>only supported for dall-e models, "standard", "hd"</para>
    /// </summary>
    [JsonPropertyName("quality")]
    public string? Quality { get; set; }

    /// <summary>
    /// Must be one of 1024x1024, 1536x1024 (landscape), 1024x1536 (portrait), or auto (default value) for the GPT image models, 
    /// one of 256x256, 512x512, or 1024x1024 for dall-e-2, 
    /// and one of 1024x1024, 1792x1024, or 1024x1792 for dall-e-3.
    /// </summary>
    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }
}

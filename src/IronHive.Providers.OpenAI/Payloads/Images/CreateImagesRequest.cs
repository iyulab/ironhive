using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Images;

/// <summary>
/// OpenAI 이미지 생성 요청
/// </summary>
public class CreateImagesRequest : OpenAIPayloadBase
{
    [JsonPropertyName("prompt")]
    public required string Prompt { get; set; }

    /// <summary>
    /// only supported for gpt-image models, "transparent", "opaque", "auto"
    /// </summary>
    [JsonPropertyName("background")]
    public string? Background { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// only supported for gpt-image models, "low", "auto"
    /// </summary>
    [JsonPropertyName("moderation")]
    public string? Moderation { get; set; }

    /// <summary>
    /// number of images to generate,
    /// between 1 and 10, not supported for dall-e-3
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
    /// <para>"low", "medium", "high" for GPT-image models</para>
    /// <para>"standard", "hd" for dall-e models</para>
    /// </summary>
    [JsonPropertyName("quality")]
    public string? Quality { get; set; }

    /// <summary>
    /// only supported for dall-e models, "url" or "b64_json"
    /// </summary>
    [JsonPropertyName("response_format")]
    public string? ResponseFormat { get; set; }

    /// <summary>
    /// <para>1024x1024, 1536x1024, 1024x1536 auto for the GPT-image models </para>
    /// <para>256x256, 512x512, 1024x1024 for dall-e-2</para>
    /// <para>1024x1024, 1792x1024, 1024x1792 for dall-e-3</para>
    /// </summary>
    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    /// <summary>
    /// only supported for dall-e models, "vivid", "natural"
    /// </summary>
    [JsonPropertyName("style")]
    public string? Style { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }
}

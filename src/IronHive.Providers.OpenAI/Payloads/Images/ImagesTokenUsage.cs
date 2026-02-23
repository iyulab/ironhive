using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Images;

public class ImagesTokenUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }

    [JsonPropertyName("input_tokens_details")]
    public ImagesTokensDetails? InputTokensDetails { get; set; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonPropertyName("output_tokens_details")]
    public ImagesTokensDetails? OutputTokensDetails { get; set; }
}

public class ImagesTokensDetails
{
    [JsonPropertyName("image_tokens")]
    public int ImageTokens { get; set; }

    [JsonPropertyName("text_tokens")]
    public int TextTokens { get; set; }
}

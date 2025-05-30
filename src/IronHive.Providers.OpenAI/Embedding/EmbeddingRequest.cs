using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Embedding;

internal class EmbeddingRequest
{
    [JsonPropertyName("input")]
    public required IEnumerable<string> Input { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// support "base64", "float"
    /// </summary>
    [JsonPropertyName("encoding_format")]
    public string? EncodingFormat { get; } = "float";

    /// <summary>
    /// Only 'text-embedding-3' later model works
    /// </summary>
    [JsonPropertyName("dimensions")]
    public int? Dimensions { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }
}

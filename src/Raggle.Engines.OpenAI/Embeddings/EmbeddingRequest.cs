using System.Text.Json.Serialization;

namespace Raggle.Engines.OpenAI.Embeddings;

public class EmbeddingRequest
{
    [JsonPropertyName("input")]
    public required ICollection<string> Input { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    // "float" or "base64"
    [JsonPropertyName("encoding_format")]
    public string? EncodingFormat { get; set; }

    // Only the 'text-embedding-3' later model works correctly
    [JsonPropertyName("dimensions")]
    public int? Dimensions { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }
}

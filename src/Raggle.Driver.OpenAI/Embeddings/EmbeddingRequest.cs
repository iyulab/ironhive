using System.Text.Json.Serialization;

namespace Raggle.Driver.OpenAI.Embeddings;

internal class EmbeddingRequest
{
    [JsonPropertyName("input")]
    public required IEnumerable<string> Input { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// openai support "base64" format,
    /// but not used in this library, "float" only
    /// </summary>
    [JsonPropertyName("encoding_format")]
    public string? EncodingFormat { get; } = "float";

    /// <summary>
    /// Only 'text-embedding-3' later model works correctly 
    /// </summary>
    [JsonPropertyName("dimensions")]
    public int? Dimensions { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }
}

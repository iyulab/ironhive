using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.Embeddings.Models;

internal class EmbeddingRequest
{
    [JsonPropertyName("input")]
    internal required ICollection<string> Input { get; set; }

    [JsonPropertyName("model")]
    internal required string Model { get; set; }

    /// <summary>
    /// openai support "base64" format,
    /// but not used in this library, "float" only
    /// </summary>
    [JsonPropertyName("encoding_format")]
    internal string? EncodingFormat { get; } = "float";

    /// <summary>
    /// Only 'text-embedding-3' later model works correctly 
    /// </summary>
    [JsonPropertyName("dimensions")]
    internal int? Dimensions { get; set; }

    [JsonPropertyName("user")]
    internal string? User { get; set; }
}

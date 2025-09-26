using System.Text.Json.Serialization;
using IronHive.Providers.OpenAI.JsonConverters;

namespace IronHive.Providers.OpenAI.Embedding.Models;

public class OpenAIEmbeddingRequest
{
    [JsonPropertyName("input")]
    [JsonConverter(typeof(EmbeddingInputJsonConverter))]
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
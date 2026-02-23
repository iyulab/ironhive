using System.Text.Json.Serialization;
using IronHive.Abstractions.Json;
using IronHive.Providers.OpenAI.JsonConverters;

namespace IronHive.Providers.OpenAI.Payloads.Embedding;

public class OpenAIEmbeddingRequest: OpenAIPayloadBase
{
    [JsonPropertyName("input")]
    [JsonConverter(typeof(EmbeddingInputJsonConverter))]
    public required IEnumerable<string> Input { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// Only 'text-embedding-3' later model works
    /// </summary>
    [JsonPropertyName("dimensions")]
    public int? Dimensions { get; set; }

    /// <summary>
    /// support "base64", "float"
    /// </summary>
    [JsonPropertyName("encoding_format")]
    public string? EncodingFormat { get; } = "float";

    [JsonPropertyName("user")]
    public string? User { get; set; }
}
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Embedding;

public class OpenAIEmbeddingResponse: OpenAIPayloadBase
{
    /// <summary>
    /// "list" always.
    /// </summary>
    [JsonPropertyName("object")]
    public string ObjectType { get; } = "list";

    [JsonPropertyName("data")]
    public IEnumerable<OpenAIEmbedding>? Data { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("usage")]
    public EmbeddingTokenUsage? Usage { get; set; }
}

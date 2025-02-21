using System.Text.Json.Serialization;

namespace Raggle.Connectors.OpenAI.Embeddings;

internal class EmbeddingResponse
{
    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("embedding")]
    public IEnumerable<float>? Embedding { get; set; }

    /// <summary>
    /// "embedding" only
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; } = "embedding";
}

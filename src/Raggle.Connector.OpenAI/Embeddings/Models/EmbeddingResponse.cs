using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.Embeddings.Models;

internal class EmbeddingResponse
{
    [JsonPropertyName("index")]
    internal int Index { get; set; }

    [JsonPropertyName("embedding")]
    internal required ICollection<float> Embedding { get; set; }

    /// <summary>
    /// "embedding" only
    /// </summary>
    [JsonPropertyName("object")]
    internal string Object { get; } = "embedding";
}

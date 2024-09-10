using System.Text.Json.Serialization;

namespace Raggle.Engines.OpenAI.Embeddings;

public class EmbeddingResponse
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("embedding")]
    public required ICollection<float> Embedding { get; set; }

    /// <summary>
    /// "embedding" only
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; } = "embedding";
}

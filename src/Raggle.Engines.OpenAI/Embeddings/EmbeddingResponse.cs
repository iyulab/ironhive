using System.Text.Json.Serialization;

namespace Raggle.Engines.OpenAI.Embeddings;

public class EmbeddingResponse
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = [];

    // always "embedding"
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;
}

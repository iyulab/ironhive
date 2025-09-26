using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Embedding.Models;

public class OpenAIEmbedding
{
    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("embedding")]
    public float[]? Embedding { get; set; }

    /// <summary>
    /// "embedding" only
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; } = "embedding";
}

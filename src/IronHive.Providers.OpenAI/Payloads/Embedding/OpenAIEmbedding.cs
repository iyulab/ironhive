using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Embedding;

public class OpenAIEmbedding
{
    [JsonPropertyName("embedding")]
    public float[]? Embedding { get; set; }

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    /// <summary>
    /// "embedding" only
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; } = "embedding";
}

using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Embedding;

public class OpenAIEmbeddingResponse
{
    /// <summary>
    /// "list" always.
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; } = "list";

    [JsonPropertyName("data")]
    public IEnumerable<OpenAIEmbedding>? Data { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("usage")]
    public EmbeddingTokenUsage? Usage { get; set; }
}

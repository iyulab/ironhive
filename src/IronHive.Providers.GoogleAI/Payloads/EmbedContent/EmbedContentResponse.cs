using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.EmbedContent;

internal class EmbedContentResponse
{
    [JsonPropertyName("embedding")]
    public required ContentEmbedding Embedding { get; set; }
}

internal class BatchEmbedContentResponse
{
    [JsonPropertyName("embeddings")]
    public IEnumerable<ContentEmbedding> Embeddings { get; set; } = [];
}
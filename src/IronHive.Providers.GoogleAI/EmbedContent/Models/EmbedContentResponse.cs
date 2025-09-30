using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.EmbedContent.Models;

internal class EmbedContentResponse
{
    [JsonPropertyName("embedding")]
    public required ContentEmbedding Embedding { get; set; }
}

internal class BatchEmbedContentResponse
{
    [JsonPropertyName("embeddings")]
    public ICollection<ContentEmbedding> Embeddings { get; set; } = [];
}
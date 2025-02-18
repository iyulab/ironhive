namespace Raggle.Abstractions.AI;

public class EmbeddingsResponse
{
    public string? Model { get; set; }

    public IEnumerable<EmbeddingData>? Embeddings { get; set; }

    public record EmbeddingData
    {
        /// <summary>
        /// Gets or sets the index of the embedding.
        /// </summary>
        public int? Index { get; set; }

        /// <summary>
        /// Gets or sets the embedding values.
        /// </summary>
        public IEnumerable<float>? Embedding { get; set; }
    }
}

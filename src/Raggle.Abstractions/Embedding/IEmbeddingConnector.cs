namespace Raggle.Abstractions.Embedding;

public interface IEmbeddingConnector
{
    /// <summary>
    /// Gets the available embedding models.
    /// </summary>
    Task<IEnumerable<EmbeddingModel>> GetModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates multiple embeddings for the given request.
    /// </summary>
    Task<EmbeddingsResponse> EmbedBatchAsync(
        EmbeddingsRequest request,
        CancellationToken cancellationToken = default);
}

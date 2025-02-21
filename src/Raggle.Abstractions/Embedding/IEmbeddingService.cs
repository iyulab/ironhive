namespace Raggle.Abstractions.Embedding;

public interface IEmbeddingService
{
    /// <summary>
    /// Gets the available embedding models.
    /// </summary>
    Task<IEnumerable<EmbeddingModel>> GetModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an embedding for the given input using the specified model.
    /// </summary>
    Task<IEnumerable<float>> EmbedAsync(
        string model,
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates multiple embeddings for the given request.
    /// </summary>
    Task<EmbeddingsResponse> EmbedBatchAsync(
        EmbeddingsRequest request,
        CancellationToken cancellationToken = default);
}

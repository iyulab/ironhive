namespace Raggle.Abstractions.AI;

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
    Task<float[]> EmbeddingAsync(
        string model,
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embeddings for the given request.
    /// </summary>
    Task<EmbeddingsResponse> EmbeddingsAsync(
        EmbeddingsRequest request,
        CancellationToken cancellationToken = default);
}

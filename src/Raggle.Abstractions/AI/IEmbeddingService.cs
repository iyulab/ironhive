namespace Raggle.Abstractions.AI;

public interface IEmbeddingService
{
    Task<IEnumerable<EmbeddingModel>> GetEmbeddingModelsAsync(
        CancellationToken cancellationToken = default);

    Task<EmbeddingResponse> EmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default);

    Task<EmbeddingsResponse> EmbeddingsAsync(
        EmbeddingsRequest request,
        CancellationToken cancellationToken = default);
}

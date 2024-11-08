namespace Raggle.Abstractions.AI;

public interface IEmbeddingService
{
    Task<IEnumerable<EmbeddingModel>> GetEmbeddingModelsAsync(
        CancellationToken cancellationToken = default);

    Task<EmbeddingResponse> EmbeddingAsync(
        string model,
        string input,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<EmbeddingResponse>> EmbeddingsAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default);
}

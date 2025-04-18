namespace IronHive.Abstractions.Embedding;

public interface IEmbeddingService
{
    /// <summary>
    /// Generates an embedding for the given input using the specified model.
    /// </summary>
    Task<IEnumerable<float>> EmbedAsync(
        string provider,
        string model,
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates multiple embeddings for the given request.
    /// </summary>
    Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string provider,
        string model,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default);
}

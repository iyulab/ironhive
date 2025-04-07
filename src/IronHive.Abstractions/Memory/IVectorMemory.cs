using IronHive.Abstractions.Embedding;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// Interface for Hive Memory Service
/// </summary>
public interface IVectorMemory
{
    Task<IEnumerable<float>> EmbedAsync(
        string input,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        IEnumerable<string> input,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> ListCollectionsAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default);

    Task<bool> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    Task CreateCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<VectorRecord>> FindVectorsAsync(
        string collectionName,
        string sourceId,
        int limit = 20,
        CancellationToken cancellationToken = default);

    Task UpdateVectorContentAsync(
        string collectionName,
        string vectorId,
        object? content,
        CancellationToken cancellationToken = default);

    Task DeleteVectorsAsync(
        string collectionName,
        string sourceId,
        CancellationToken cancellationToken = default);

    Task ScheduleVectorizationAsync(
        string collectionName,
        IMemorySource source,
        IEnumerable<string> steps,
        IDictionary<string, object?>? handlerOptions = null,
        CancellationToken cancellationToken = default);

    Task<VectorSearchResult> SearchVectorsAsync(
        string collectionName,
        string query,
        float minScore = 0,
        int limit = 5,
        IEnumerable<string>? sourceIds = null,
        CancellationToken cancellationToken = default);
}

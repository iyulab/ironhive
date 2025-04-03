using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;

namespace IronHive.Abstractions;

public interface IHiveMemory
{
    string EmbedProvider { get; init; }

    string EmbedModel { get; init; }

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

    Task MemorizeAsync(
        string collectionName,
        IMemorySource source,
        IEnumerable<string> steps,
        IDictionary<string, object?>? handlerOptions = null,
        CancellationToken cancellationToken = default);

    Task UnMemorizeAsync(
        string collectionName,
        string sourceId,
        CancellationToken cancellationToken = default);

    Task<VectorSearchResult> SearchAsync(
        string collectionName,
        string query,
        float minScore = 0,
        int limit = 5,
        IEnumerable<string>? sourceIds = null,
        CancellationToken cancellationToken = default);
}

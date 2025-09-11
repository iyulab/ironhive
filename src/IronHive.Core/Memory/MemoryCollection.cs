using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Vector;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class MemoryCollection : IMemoryCollection
{
    private readonly IStorageRegistry _storages;
    private readonly IEmbeddingService _embedder;

    public MemoryCollection(IStorageRegistry storages, IEmbeddingService embedder)
    {
        _storages = storages;
        _embedder = embedder;
    }

    /// <inheritdoc />
    public required string StorageName { get; init; }

    /// <inheritdoc />
    public required string CollectionName { get; init; }

    /// <inheritdoc />
    public required string EmbeddingProvider { get; init; }

    /// <inheritdoc />
    public required string EmbeddingModel { get; init; }

    /// <inheritdoc />
    public async Task IndexSourceAsync(
        string queueName,
        IMemorySource source,
        CancellationToken cancellationToken = default)
    {
        var ctx = new MemoryContext
        {
            Source = source,
            Target = new VectorMemoryTarget
            {
                StorageName = StorageName,
                CollectionName = CollectionName,
                EmbeddingProvider = EmbeddingProvider,
                EmbeddingModel = EmbeddingModel,
            },
        };

        if (!_storages.TryGet<IQueueStorage>(StorageName, out var queue))
            throw new InvalidOperationException($"Vector storage '{StorageName}' is not registered.");
        await queue.EnqueueAsync(ctx, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeindexSourceAsync(
        string sourceId,
        CancellationToken cancellationToken = default)
    {
        if (!_storages.TryGet<IVectorStorage>(StorageName, out var storage))
            throw new InvalidOperationException($"Vector storage '{StorageName}' is not registered.");

        var filter = new VectorRecordFilter();
        filter.AddSourceId(sourceId);
        await storage.DeleteVectorsAsync(CollectionName, filter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VectorSearchResult> SemanticSearchAsync(
        string query,
        MemorySearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (!_storages.TryGet<IVectorStorage>(StorageName, out var storage))
            throw new InvalidOperationException($"Vector storage '{StorageName}' is not registered.");

        options ??= new MemorySearchOptions();
        var sourceIds = options.SourceIds;
        VectorRecordFilter? filter = null;
        if (sourceIds != null && sourceIds.Any())
        {
            filter = new VectorRecordFilter();
            filter.AddSourceIds(sourceIds);
        }

        var embeddings = await _embedder.EmbedAsync(
            provider: EmbeddingProvider,
            modelId: EmbeddingModel,
            input: query,
            cancellationToken: cancellationToken);
        var records = await storage.SearchVectorsAsync(
            collectionName: CollectionName,
            vector: embeddings,
            minScore: options.MinScore,
            limit: options.Limit,
            filter: filter,
            cancellationToken: cancellationToken);

        return new VectorSearchResult
        {
            CollectionName = CollectionName,
            SearchQuery = query,
            ScoredVectors = records,
        };
    }
}

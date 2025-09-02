using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Pipelines;
using IronHive.Abstractions;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Vector;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class MemoryService : IMemoryService
{
    private readonly IServiceProvider _services;
    private readonly IEmbeddingService _embedder;
    private readonly IKeyedCollection<IQueueStorage> _queues;
    private readonly IKeyedCollection<IVectorStorage> _vectors;
    private IPipelineRunner<PipelineContext> _pipeline;

    public MemoryService(IServiceProvider services, IPipelineRunner<PipelineContext> pipeline)
    {
        _services = services;
        _embedder = services.GetRequiredService<IEmbeddingService>();

        var storages = services.GetRequiredService<IKeyedCollectionGroup<IKeyedStorage>>();
        _queues = storages.Of<IQueueStorage>();
        _vectors = storages.Of<IVectorStorage>();
        _pipeline = pipeline;
    }

    /// <inheritdoc />
    public void SetPipeline(PipelineBuildDelegate configure)
    {
        _pipeline = configure(PipelineFactory.Create<PipelineContext>(_services));
    }

    /// <inheritdoc />
    public IMemoryWorkerService CreateWorkers(MemoryWorkerConfig config)
    {
        return new MemoryWorkerService(this, config);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<VectorCollection>> ListCollectionsAsync(
        string storageName,
        CancellationToken cancellationToken = default)
    {
        if (!_vectors.TryGet(storageName, out var vector))
            throw new KeyNotFoundException($"Vector storage key '{storageName}' not found.");

        return await vector.ListCollectionsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> CollectionExistsAsync(
        string storageName,
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        if (!_vectors.TryGet(storageName, out var vector))
            throw new KeyNotFoundException($"Vector storage key '{storageName}' not found.");

        return await vector.CollectionExistsAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VectorCollection?> GetCollectionInfoAsync(
        string storageName,
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        if (!_vectors.TryGet(storageName, out var vector))
            throw new KeyNotFoundException($"Vector storage key '{storageName}' not found.");

        return await vector.GetCollectionInfoAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string storageName,
        string collectionName,
        string embeddingProvider,
        string embeddingModel,
        CancellationToken cancellationToken = default)
    {
        if (!_vectors.TryGet(storageName, out var vector))
            throw new KeyNotFoundException($"Vector storage key '{storageName}' not found.");

        const string sample = "dimension calculation sample";
        var embeddings = await _embedder.EmbedAsync(embeddingProvider, embeddingModel, sample, cancellationToken)
            ?? throw new InvalidOperationException("Failed to get embeddings for dimension calculation.");

        await vector.CreateCollectionAsync(new VectorCollection
        {
            Name = collectionName,
            Dimensions = embeddings.Count(),
            EmbeddingProvider = embeddingProvider,
            EmbeddingModel = embeddingModel,
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string storageName,
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        if (!_vectors.TryGet(storageName, out var vector))
            throw new KeyNotFoundException($"Vector storage key '{storageName}' not found.");
        await vector.DeleteCollectionAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task QueueIndexSourceAsync(
        string queueName,
        IMemorySource source,
        IMemoryTarget target,
        CancellationToken cancellationToken = default)
    {
        if (target is not VectorMemoryTarget vt)
            throw new InvalidOperationException("currently only VectorMemoryTarget is supported.");

        if (!_vectors.TryGet(vt.StorageName, out var vector))
            throw new KeyNotFoundException($"Vector storage key '{vt.StorageName}' not found.");
        var collection = await vector.GetCollectionInfoAsync(vt.CollectionName, cancellationToken)
            ?? throw new InvalidOperationException($"Collection '{vt.CollectionName}' does not exist.");

        var ctx = new PipelineContext
        {
            Source = source,
            Target = new VectorMemoryTarget
            {
                StorageName = vt.StorageName,
                CollectionName = collection.Name,
                EmbeddingProvider = collection.EmbeddingProvider,
                EmbeddingModel = collection.EmbeddingModel,
            },
        };

        if (!_queues.TryGet(queueName, out var _queue))
            throw new KeyNotFoundException($"Queue storage key '{queueName}' not found.");
        await _queue.EnqueueAsync(ctx, cancellationToken);
    }

    /// <inheritdoc />
    public async Task IndexSourceAsync(
        IMemorySource source,
        IMemoryTarget target,
        CancellationToken cancellationToken = default)
    {
        if (target is not VectorMemoryTarget vt)
            throw new InvalidOperationException("currently only VectorMemoryTarget is supported.");

        if (!_vectors.TryGet(vt.StorageName, out var vector))
            throw new KeyNotFoundException($"Vector storage key '{vt.StorageName}' not found.");

        var collection = await vector.GetCollectionInfoAsync(vt.CollectionName, cancellationToken)
            ?? throw new InvalidOperationException($"Collection '{vt.CollectionName}' does not exist.");
        var ctx = new PipelineContext
        {
            Source = source,
            Target = new VectorMemoryTarget
            {
                StorageName = vector.StorageName,
                CollectionName = collection.Name,
                EmbeddingProvider = collection.EmbeddingProvider,
                EmbeddingModel = collection.EmbeddingModel,
            },
        };
        await _pipeline.InvokeAsync(ctx, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteSourceAsync(
        string storageName,
        string collectionName, 
        string sourceId, 
        CancellationToken cancellationToken = default)
    {
        if (!_vectors.TryGet(storageName, out var vector))
            throw new KeyNotFoundException($"Vector storage key '{storageName}' not found.");

        var filter = new VectorRecordFilter();
        filter.AddSourceId(sourceId);
        await vector.DeleteVectorsAsync(collectionName, filter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VectorSearchResult> SearchSimilarAsync(
        string storageName,
        string collectionName,
        string query,
        float minScore = 0,
        int limit = 5,
        IEnumerable<string>? sourceIds = null,
        CancellationToken cancellationToken = default)
    {
        if (!_vectors.TryGet(storageName, out var vector))
            throw new KeyNotFoundException($"Vector storage key '{storageName}' not found.");

        var collection = await vector.GetCollectionInfoAsync(collectionName, cancellationToken)
            ?? throw new InvalidOperationException($"Collection '{collectionName}' does not exist.");

        VectorRecordFilter? filter = null;
        if (sourceIds != null && sourceIds.Any())
        {
            filter = new VectorRecordFilter();
            filter.AddSourceIds(sourceIds);
        }

        var embeddings = await _embedder.EmbedAsync(
            provider: collection.EmbeddingProvider, 
            modelId: collection.EmbeddingModel, 
            input: query, 
            cancellationToken: cancellationToken);
        var records = await vector.SearchVectorsAsync(
            collectionName: collection.Name,
            vector: embeddings,
            minScore: minScore,
            limit: limit,
            filter: filter,
            cancellationToken: cancellationToken);

        return new VectorSearchResult
        {
            CollectionName = collectionName,
            SearchQuery = query,
            ScoredVectors = records,
        };
    }
}

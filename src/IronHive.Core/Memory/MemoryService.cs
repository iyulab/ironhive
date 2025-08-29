using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Storages;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Pipelines;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class MemoryService : IMemoryService
{
    private readonly IQueueStorage<MemoryPipelineContext<object>> _queue;
    private readonly IVectorStorage _vector;
    private readonly IEmbeddingService _embedder;
    private readonly IPipelineRunner<MemoryPipelineContext<object>, MemoryPipelineContext<object>> _pipeline;

    public MemoryService(
        IQueueStorage<MemoryPipelineContext<object>> queue,
        IVectorStorage vector,
        IEmbeddingService embedder,
        IPipelineRunner<MemoryPipelineContext<object>, MemoryPipelineContext<object>> pipeline)
    {
        _queue = queue;
        _vector = vector;
        _embedder = embedder;
        _pipeline = pipeline;

        Workers = new MemoryWorkerService(this);
    }

    public IQueueStorage<MemoryPipelineContext<object>> QueueStorage 
    {
        get => _queue;
        set => throw new NotImplementedException(); 
    }
    
    public IVectorStorage VectorStorage 
    {
        get => _vector;
        set => throw new NotImplementedException(); 
    }

    /// <inheritdoc />
    public IMemoryWorkerService Workers { get; }

    /// <inheritdoc />
    public async Task<IEnumerable<VectorCollection>> ListCollectionsAsync( 
        CancellationToken cancellationToken = default)
    {
        return await _vector.ListCollectionsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> CollectionExistsAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        return await _vector.CollectionExistsAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VectorCollection?> GetCollectionInfoAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        return await _vector.GetCollectionInfoAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string collectionName,
        string embeddingProvider,
        string embeddingModel,
        CancellationToken cancellationToken = default)
    {
        const string sample = "dimension calculation sample";
        var embeddings = await _embedder.EmbedAsync(embeddingProvider, embeddingModel, sample, cancellationToken)
            ?? throw new InvalidOperationException("Failed to get embeddings for dimension calculation.");

        await _vector.CreateCollectionAsync(new VectorCollection
        {
            Name = collectionName,
            Dimensions = embeddings.Count(),
            EmbeddingProvider = embeddingProvider,
            EmbeddingModel = embeddingModel,
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        await _vector.DeleteCollectionAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task QueueIndexSourceAsync(
        string collectionName, 
        IMemorySource source,
        CancellationToken cancellationToken = default)
    {   
        var collection = await _vector.GetCollectionInfoAsync(collectionName, cancellationToken)
            ?? throw new InvalidOperationException($"Collection '{collectionName}' does not exist.");

        var ctx = new MemoryPipelineContext<object>
        {
            Source = source,
            Target = new VectorMemoryTarget
            {
                CollectionName = collection.Name,
                EmbeddingProvider = collection.EmbeddingProvider,
                EmbeddingModel = collection.EmbeddingModel,
            },
        };
        await _queue.EnqueueAsync(ctx, cancellationToken);
    }

    /// <inheritdoc />
    public async Task IndexSourceAsync(
        string collectionName, 
        IMemorySource source,
        CancellationToken cancellationToken = default)
    {
        var collection = await _vector.GetCollectionInfoAsync(collectionName, cancellationToken)
            ?? throw new InvalidOperationException($"Collection '{collectionName}' does not exist.");
        var ctx = new MemoryPipelineContext<object>
        {
            Source = source,
            Target = new VectorMemoryTarget
            {
                CollectionName = collection.Name,
                EmbeddingProvider = collection.EmbeddingProvider,
                EmbeddingModel = collection.EmbeddingModel,
            },
        };
        await _pipeline.InvokeAsync(ctx, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteSourceAsync(
        string collectionName, 
        string sourceId, 
        CancellationToken cancellationToken = default)
    {
        var filter = new VectorRecordFilter();
        filter.AddSourceId(sourceId);
        await _vector.DeleteVectorsAsync(collectionName, filter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VectorSearchResult> SearchSimilarAsync(
        string collectionName,
        string query,
        float minScore = 0,
        int limit = 5,
        IEnumerable<string>? sourceIds = null,
        CancellationToken cancellationToken = default)
    {
        var collection = await _vector.GetCollectionInfoAsync(collectionName, cancellationToken)
            ?? throw new InvalidOperationException($"Collection '{collectionName}' does not exist.");

        VectorRecordFilter? filter = null;
        if (sourceIds != null && sourceIds.Any())
        {
            filter = new VectorRecordFilter();
            filter.AddSourceIds(sourceIds);
        }

        var vector = await _embedder.EmbedAsync(collection.EmbeddingProvider, collection.EmbeddingModel, query, cancellationToken);
        var records = await _vector.SearchVectorsAsync(
            collectionName: collection.Name,
            vector: vector,
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

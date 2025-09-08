using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Pipelines;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Vector;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class VectorMemoryService : IMemoryService
{
    private readonly IQueueStorage _queue;
    private readonly IVectorStorage _vector;
    private readonly IEmbeddingService _embedder;
    private readonly IPipelineRunner<PipelineContext> _pipeline;

    public VectorMemoryService(
        IQueueStorage queue,
        IVectorStorage vector,
        IEmbeddingService embedder,
        IPipelineRunner<PipelineContext> pipeline)
    {
        _queue = queue;
        _vector = vector;
        _embedder = embedder;
        _pipeline = pipeline;
    }

    /// <summary>
    /// 현재 벡터 스토리지의 이름입니다.
    /// </summary>
    public required string StorageName { get; init; }

    /// <summary>
    /// 현재 벡터 스토리지의 컬렉션 이름입니다.
    /// </summary>
    public required string CollectionName { get; init; }

    /// <inheritdoc />
    public required IMemoryWorkerService Workers { get; init; }

    /// <inheritdoc />
    public async Task QueueIndexSourceAsync(
        IMemorySource source,
        CancellationToken cancellationToken = default)
    {
        var collection = await _vector.GetCollectionInfoAsync(CollectionName, cancellationToken)
            ?? throw new InvalidOperationException($"Collection '{CollectionName}' does not exist.");

        var ctx = new PipelineContext
        {
            Source = source,
            Target = new VectorMemoryTarget
            {
                StorageName = StorageName,
                CollectionName = collection.Name,
                EmbeddingProvider = collection.EmbeddingProvider,
                EmbeddingModel = collection.EmbeddingModel,
            },
        };

        await _queue.EnqueueAsync(ctx, cancellationToken);
    }

    /// <inheritdoc />
    public async Task IndexSourceAsync(
        IMemorySource source,
        CancellationToken cancellationToken = default)
    {
        var collection = await _vector.GetCollectionInfoAsync(CollectionName, cancellationToken)
            ?? throw new InvalidOperationException($"Collection '{CollectionName}' does not exist.");

        var ctx = new PipelineContext
        {
            Source = source,
            Target = new VectorMemoryTarget
            {
                StorageName = StorageName,
                CollectionName = collection.Name,
                EmbeddingProvider = collection.EmbeddingProvider,
                EmbeddingModel = collection.EmbeddingModel,
            },
        };
        await _pipeline.InvokeAsync(ctx, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteSourceAsync(
        string sourceId, 
        CancellationToken cancellationToken = default)
    {
        var filter = new VectorRecordFilter();
        filter.AddSourceId(sourceId);
        await _vector.DeleteVectorsAsync(CollectionName, filter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VectorSearchResult> SearchSimilarAsync(
        string query,
        float minScore = 0,
        int limit = 5,
        IEnumerable<string>? sourceIds = null,
        CancellationToken cancellationToken = default)
    {
        var collection = await _vector.GetCollectionInfoAsync(CollectionName, cancellationToken)
            ?? throw new InvalidOperationException($"Collection '{CollectionName}' does not exist.");

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
        var records = await _vector.SearchVectorsAsync(
            collectionName: collection.Name,
            vector: embeddings,
            minScore: minScore,
            limit: limit,
            filter: filter,
            cancellationToken: cancellationToken);

        return new VectorSearchResult
        {
            CollectionName = CollectionName,
            SearchQuery = query,
            ScoredVectors = records,
        };
    }

    /// <summary>
    /// 벡터 메모리의 컬렉션을 생성합니다.
    /// </summary>
    private async Task EnsureCollectionAsync(
        string storageName,
        string collectionName,
        string embeddingProvider,
        string embeddingModel,
        CancellationToken cancellationToken = default)
    {
        if (await _vector.CollectionExistsAsync(collectionName, cancellationToken))
            return;

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
}

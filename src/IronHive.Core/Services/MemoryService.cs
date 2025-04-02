using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;

namespace IronHive.Core.Services;

public class MemoryService : IMemoryService
{
    private readonly IQueueStorage _queue;
    private readonly IPipelineStorage _pipeline;
    private readonly IVectorStorage _vector;
    private readonly IEmbeddingService _embedding;

    public MemoryService(IQueueStorage queue, IPipelineStorage pipeline, IVectorStorage vector, IEmbeddingService embedding)
    {
        _queue = queue;
        _pipeline = pipeline;
        _vector = vector;
        _embedding = embedding;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> ListCollectionsAsync(
        string? prefix = null, 
        CancellationToken cancellationToken = default)
    {
        return await _vector.ListCollectionsAsync(prefix, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> CollectionExistsAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        return await _vector.CollectionExistsAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string collectionName, 
        string embedProvider, 
        string embedModel, 
        CancellationToken cancellationToken = default)
    {
        if (await _vector.CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' already exists.");

        var dummy = "this text is used to calculate the embedding dimensions";
        var embedding = await _embedding.EmbedAsync(embedProvider, embedModel, dummy, cancellationToken);
        var dimensions = embedding.Count();
        await _vector.CreateCollectionAsync(collectionName, dimensions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        if (!await _vector.CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' does not exist.");

        await _vector.DeleteCollectionAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task MemorizeAsync(
        string collectionName, 
        IMemorySource source, 
        IEnumerable<string> steps, 
        IDictionary<string, object>? handlerOptions = null, 
        CancellationToken cancellationToken = default)
    {
        var id = source.Id;
        var pipeline = new PipelineContext
        {
            Id = id,
            Source = source,
            Steps = steps.ToList(),
            HandlerOptions = handlerOptions,
        };

        await _queue.EnqueueAsync(id, cancellationToken);
        await _pipeline.SetAsync(pipeline, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UnMemorizeAsync(
        string collectionName, 
        string sourceId, 
        CancellationToken cancellationToken = default)
    {
        if (await _pipeline.ContainsAsync(sourceId, cancellationToken))
            await _pipeline.DeleteAsync(sourceId, cancellationToken);

        var filter = new VectorRecordFilter();
        filter.AddSourceId(sourceId);
        await _vector.DeleteVectorsAsync(collectionName, filter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VectorSearchResult> SearchAsync(
        string collectionName, 
        string embedProvider, 
        string embedModel, 
        string query, 
        float minScore = 0, 
        int limit = 5, 
        IEnumerable<string>? sourceIds = null,
        CancellationToken cancellationToken = default)
    {
        VectorRecordFilter? filter = null;
        if (sourceIds != null && !sourceIds.Any())
        {
            filter = new VectorRecordFilter();
            filter.AddSourceIds(sourceIds);
        }
        
        var vector = await _embedding.EmbedAsync(embedProvider, embedModel, query, cancellationToken);
        var records = await _vector.SearchVectorsAsync(
            collectionName: collectionName,
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

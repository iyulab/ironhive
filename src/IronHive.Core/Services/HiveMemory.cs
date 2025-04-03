using IronHive.Abstractions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;

namespace IronHive.Core.Services;

public class HiveMemory : IHiveMemory
{
    private readonly IQueueStorage _queue;
    private readonly IVectorStorage _vector;
    private readonly IEmbeddingService _embedding;
    private readonly IEnumerable<IPipelineEventHandler> _events;

    private readonly int _dimensions;

    public required string EmbedProvider { get; init; }
    public required string EmbedModel { get; init; }

    public HiveMemory(IQueueStorage queue, IVectorStorage vector, 
        IEmbeddingService embedding,
        IEnumerable<IPipelineEventHandler> events)
    {
        _queue = queue;
        _vector = vector;
        _embedding = embedding;
        _events = events;
        _dimensions = GetDimensions();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<float>> EmbedAsync(
        string input,
        CancellationToken cancellationToken = default)
        => await _embedding.EmbedAsync(EmbedProvider, EmbedModel, input, cancellationToken);

    /// <inheritdoc />
    public Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        IEnumerable<string> input, 
        CancellationToken cancellationToken = default)
        => _embedding.EmbedBatchAsync(EmbedProvider, EmbedModel, input, cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        if (await _vector.CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' already exists.");

        await _vector.CreateCollectionAsync(collectionName, _dimensions, cancellationToken);
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
        IDictionary<string, object?>? handlerOptions = null, 
        CancellationToken cancellationToken = default)
    {
        var request = new PipelineRequest
        {
            Id = source.Id,
            Source = source,
            Steps = steps.ToList(),
            HandlerOptions = handlerOptions,
        };

        await _queue.EnqueueAsync(request, cancellationToken);
        await Task.WhenAll(_events.Select(e => e.OnQueuedAsync(request.Id)));
    }

    /// <inheritdoc />
    public async Task UnMemorizeAsync(
        string collectionName, 
        string sourceId, 
        CancellationToken cancellationToken = default)
    {
        var filter = new VectorRecordFilter();
        filter.AddSourceId(sourceId);
        await _vector.DeleteVectorsAsync(collectionName, filter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VectorSearchResult> SearchAsync(
        string collectionName,
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
        
        var vector = await EmbedAsync(query, cancellationToken);
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

    // 지정된 모델의 차원을 계산합니다.
    private int GetDimensions()
    {
        var dummy = "this text is used to calculate the embedding dimensions";
        var embedding = EmbedAsync(dummy).Result;
        return embedding.Count();
    }
}

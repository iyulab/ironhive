using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Storages;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class MemoryService : IMemoryService
{
    private readonly IServiceProvider _services;
    private readonly IQueueStorage<MemoryPipelineRequest> _queue;
    private readonly IVectorStorage _vector;
    private readonly IMemoryEmbedder _embedder;

    private int? _dimensions = null;

    public MemoryService(IServiceProvider services)
    {
        _services = services;
        _queue = services.GetRequiredService<IQueueStorage<MemoryPipelineRequest>>();
        _vector = services.GetRequiredService<IVectorStorage>();
        _embedder = services.GetRequiredService<IMemoryEmbedder>();
    }

    /// <inheritdoc />
    public ICollection<IMemoryWorker> Workers { get; set; } = [];

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

        if (!_dimensions.HasValue)
        {
            const string sample = "dimension calculation sample";
            var embeddings = await _embedder.EmbedAsync(sample, CancellationToken.None);
            _dimensions = embeddings.Count();
        }
        
        await _vector.CreateCollectionAsync(collectionName, _dimensions.Value, cancellationToken);
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
    public async Task QueueIndexSourceAsync(
        string collectionName, 
        IMemorySource source, 
        IEnumerable<string> steps, 
        IDictionary<string, object?>? handlerOptions = null, 
        CancellationToken cancellationToken = default)
    {
        // Ensure at least one worker is available
        if (Workers.Count != 0)
            Workers.Add(new MemoryWorker(_services));
        
        var request = new MemoryPipelineRequest
        {
            Source = source,
            Target = new VectorMemoryTarget
            {
                CollectionName = collectionName,
            },
            Steps = steps,
            HandlerOptions = handlerOptions,
        };
        await _queue.EnqueueAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task IndexSourceAsync(
        string collectionName, 
        IMemorySource source, 
        IEnumerable<string> steps, 
        IDictionary<string, object?>? handlerOptions = null, 
        CancellationToken cancellationToken = default)
    {
        var worker = new MemoryWorker(_services);
        var request = new MemoryPipelineRequest
        {
            Source = source,
            Target = new VectorMemoryTarget
            {
                CollectionName = collectionName,
            },
            Steps = steps,
            HandlerOptions = handlerOptions,
        };
        await worker.ExecuteAsync(request, cancellationToken);
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
        VectorRecordFilter? filter = null;
        if (sourceIds != null && sourceIds.Any())
        {
            filter = new VectorRecordFilter();
            filter.AddSourceIds(sourceIds);
        }

        var vector = await _embedder.EmbedAsync(query, cancellationToken);
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

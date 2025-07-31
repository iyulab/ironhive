using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Storages;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core.Services;

public class MemoryService : IMemoryService
{
    private readonly IQueueStorage<MemoryPipelineRequest> _queue;
    private readonly IVectorStorage _vector;
    private readonly IMemoryEmbedder _embedder;

    private readonly int _dimensions;

    public MemoryService(IServiceProvider services)
    {
        _queue = services.GetRequiredService<IQueueStorage<MemoryPipelineRequest>>();
        _vector = services.GetRequiredService<IVectorStorage>();
        _embedder = services.GetRequiredService<IMemoryEmbedder>();

        // 임베딩 차원을 계산합니다.
        var dummy = "this text is used to calculate the embedding dimensions";
        _dimensions = _embedder.EmbedAsync(dummy).Result.Count();
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
    public async Task<IEnumerable<VectorRecord>> FindVectorsAsync(
        string collectionName,
        string sourceId,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (!await _vector.CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' does not exist.");

        var filter = new VectorRecordFilter();
        filter.AddSourceId(sourceId);
        var records = await _vector.FindVectorsAsync(collectionName, limit, filter, cancellationToken);
        return records;
    }

    /// <inheritdoc />
    public async Task UpdateVectorContentAsync(
        string collectionName,
        string vectorId,
        object? content,
        CancellationToken cancellationToken = default)
    {
        var filter = new VectorRecordFilter();
        filter.AddVectorId(vectorId);
        var vectors = await _vector.FindVectorsAsync(collectionName, 1, filter, cancellationToken);
        var vector = vectors.FirstOrDefault();

        if (vector == null)
            throw new InvalidOperationException($"Vector '{vectorId}' not found in collection '{collectionName}'.");

        // 업데이트 합니다.
        vector.Content = content;
        await _vector.UpsertVectorsAsync(collectionName, [vector], cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteVectorsAsync(
        string collectionName, 
        string sourceId,
        CancellationToken cancellationToken = default)
    {
        var filter = new VectorRecordFilter();
        filter.AddSourceId(sourceId);
        await _vector.DeleteVectorsAsync(collectionName, filter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VectorSearchResult> SearchVectorsAsync(
        string collectionName,
        string query,
        float minScore = 0,
        int limit = 5,
        IEnumerable<string>? sourceIds = null,
        CancellationToken cancellationToken = default)
    {
        var filter = GetVectorRecordFilter(sourceIds);
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

    /// <inheritdoc />
    public async Task ScheduleVectorizationAsync(
        string collectionName,
        IMemorySource source,
        IEnumerable<string> steps,
        IDictionary<string, object?>? handlerOptions = null,
        CancellationToken cancellationToken = default)
    {
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

    // 필터 생성
    private VectorRecordFilter? GetVectorRecordFilter(IEnumerable<string>? sourceIds = null)
    {
        if (sourceIds != null && sourceIds.Any())
        {
            var filter = new VectorRecordFilter();
            filter.AddSourceIds(sourceIds);
            return filter;
        }
        else
        {
            return null;
        }
    }
}

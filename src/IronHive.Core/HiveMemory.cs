using IronHive.Abstractions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core;

public class HiveMemory : IHiveMemory
{
    private readonly IServiceProvider _services;
    private readonly IQueueStorage _queue;
    private readonly IPipelineStorage _pipeline;
    private readonly IVectorStorage _vector;
    private readonly IEmbeddingService _embedding;

    private CancellationTokenSource _cts = new();

    public required string QueueName { get; init; }

    public HiveMemory(IServiceProvider services)
    {
        _services = services;
        _queue = services.GetRequiredService<IQueueStorage>();
        _pipeline = services.GetRequiredService<IPipelineStorage>();
        _vector = services.GetRequiredService<IVectorStorage>();
        _embedding = services.GetRequiredService<IEmbeddingService>();
    }

    /// <inheritdoc />
    public async Task StartWorkerAsync()
    {
        await _queue.CreateQueueAsync(QueueName);
        var worker = new PipelineWorker(_services) { QueueName = QueueName };
        _ = worker.StartAsync(_cts.Token);
    }

    /// <inheritdoc />
    public async Task StopWorkerAsync()
    {
        _cts.Cancel();
        await Task.Delay(1_000);
        _cts.Dispose();
        _cts = new CancellationTokenSource();
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
    public Task<DataPipeline> GetIngestionStatusAsync(
        string collectionName, 
        string sourceId, 
        CancellationToken cancellationToken = default)
    {
        return _pipeline.GetAsync(sourceId, cancellationToken);
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
        var pipeline = new DataPipeline
        {
            Id = id,
            Source = source,
            Steps = steps.ToList(),
            HandlerOptions = handlerOptions,
        };

        await _queue.EnqueueAsync(QueueName, id, cancellationToken);
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

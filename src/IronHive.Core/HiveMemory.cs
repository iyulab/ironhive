using IronHive.Abstractions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;

namespace IronHive.Core;

public class HiveMemory : IHiveMemory
{
    private readonly IEmbeddingService _service;
    private readonly IPipelineOrchestrator _orchestrator;

    public IVectorStorage Storage { get; }

    public HiveMemory(IEmbeddingService embedding, IVectorStorage storage, IPipelineOrchestrator orchestrator)
    {
        _service = embedding;
        Storage = storage;
        _orchestrator = orchestrator;
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string collectionName, 
        string embedProvider, 
        string embedModel, 
        CancellationToken cancellationToken = default)
    {
        //if (await Storage.CollectionExistsAsync(collectionName, cancellationToken))
        //    throw new InvalidOperationException($"Collection '{collectionName}' already exists.");

        var dummy = "this text is used to calculate the embedding dimensions";
        var embedding = await _service.EmbedAsync(embedProvider, embedModel, dummy, cancellationToken);
        var dimensions = embedding.Count();
        await Storage.CreateCollectionAsync(collectionName, dimensions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        if (!await Storage.CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' does not exist.");

        await Storage.DeleteCollectionAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public Task<DataPipeline> MemorizeAsync(
        string collectionName, 
        IMemorySource source, 
        IEnumerable<string> steps, 
        IDictionary<string, object>? handlerOptions = null, 
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task UnMemorizeAsync(
        string collectionName, 
        string sourceId, 
        CancellationToken cancellationToken = default)
    {
        var filter = new VectorRecordFilter();
        filter.AddSourceId(sourceId);
        await Storage.DeleteVectorsAsync(collectionName, filter, cancellationToken);
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
        
        var vector = await _service.EmbedAsync(embedProvider, embedModel, query, cancellationToken);
        var records = await Storage.SearchVectorsAsync(
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

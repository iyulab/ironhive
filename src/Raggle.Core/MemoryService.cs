using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Queue;

namespace Raggle.Core;

public class MemoryService : IMemoryService
{
    private bool isBusy = false;

    private readonly IDocumentStorage _documentStorage;
    private readonly IVectorStorage _vectorStorage;
    private readonly IPipelineOrchestrator _orchestrator;
    private readonly IQueueClient? _queueClient;

    public MemoryService(
        IDocumentStorage documentStorage,
        IVectorStorage vectorStorage,
        IPipelineOrchestrator orchestrator,
        IQueueClient? queueClient = null)
    {
        _documentStorage = documentStorage;
        _vectorStorage = vectorStorage;
        _orchestrator = orchestrator;
        _queueClient = queueClient;
    }

    public async Task<IEnumerable<string>> GetAllCollections(CancellationToken cancellationToken = default)
    {
        return await _documentStorage.GetAllCollectionsAsync(cancellationToken);
    }

    // ============ Transaction 필요 ============
    public async Task CreateCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        await _documentStorage.CreateCollectionAsync(collection, cancellationToken);

        try
        {
            await _vectorStorage.CreateCollectionAsync(collection, cancellationToken);
        }
        catch
        {
            await _documentStorage.DeleteCollectionAsync(collection);
            throw;
        }
    }

    // ============ Transaction 필요 ============
    public async Task DeleteCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        await _documentStorage.DeleteCollectionAsync(collection, cancellationToken);
        await _vectorStorage.DeleteCollectionAsync(collection, cancellationToken);
    }

    public async Task<IEnumerable<DocumentRecord>> GetMemoriesAsync(
        string collection,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        return await _documentStorage.FindDocumentRecordsAsync(collection, filter, cancellationToken);
    }

    public async Task<DocumentRecord?> GetMemoryAsync(
        string collection,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        var filter = new MemoryFilterBuilder().AddDocumentId(documentId).Build();
        var records = await _documentStorage.FindDocumentRecordsAsync(collection, filter, cancellationToken);
        return records.FirstOrDefault();
    }

    // ============ Transaction 필요없음 Fail 응답 ============
    public async Task MemorizeAsync(
        MemorizeRequest request,
        CancellationToken cancellationToken = default)
    {
        var pipeline = new DataPipeline
        {
            CollectionName = request.CollectionName,
            DocumentId = request.DocumentId,
            Steps = request.Steps.ToList(),
            Tags = request.Tags,
            UploadFile = request.File
        };

        if (isBusy)
        {
            throw new NotImplementedException();
        }
        else
        {
            await _orchestrator.ExecuteAsync(pipeline, cancellationToken);
        }
    }

    // ============ Transaction 필요 ============
    public async Task UnMemorizeAsync(
        string collection,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        await _documentStorage.DeleteDocumentRecordAsync(collection, documentId, cancellationToken);
        await _vectorStorage.DeleteRecordsAsync(collection, documentId, cancellationToken);
    }

    public async Task SearchMemoriesAsync(
        string collection,
        string query,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        //await _vectorStorage.FindRecordsAsync(collection, cancellationToken);
        throw new NotImplementedException();
    }

}

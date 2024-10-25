using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Utils;

namespace Raggle.Core;

public class MemoryService : IMemoryService
{
    private readonly MimeTypeDetector _detecter = new();

    private readonly IDocumentStorage _documentStorage;
    private readonly IVectorStorage _vectorStorage;
    private readonly IPipelineOrchestrator _orchestrator;
    
    public MemoryService(
        IDocumentStorage documentStorage,
        IVectorStorage vectorStorage,
        IPipelineOrchestrator orchestrator)
    {
        _documentStorage = documentStorage;
        _vectorStorage = vectorStorage;
        _orchestrator = orchestrator;
    }

    public async Task<IEnumerable<string>> GetCollectionListAsync(CancellationToken cancellationToken = default)
    {
        return await _documentStorage.GetCollectionListAsync(cancellationToken);
    }

    public async Task CreateCollectionAsync(string collectionName, ulong vectorSize, CancellationToken cancellationToken = default)
    {   
        try
        {
            await _documentStorage.CreateCollectionAsync(collectionName, cancellationToken);
            await _vectorStorage.CreateCollectionAsync(collectionName, vectorSize, cancellationToken);
        }
        catch
        {
            await DeleteCollectionAsync(collectionName, cancellationToken);
            throw;
        }
    }

    public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        if (await _documentStorage.ExistCollectionAsync(collectionName, cancellationToken))
            await _documentStorage.DeleteCollectionAsync(collectionName, cancellationToken);

        if (await _vectorStorage.ExistCollectionAsync(collectionName, cancellationToken))
            await _vectorStorage.DeleteCollectionAsync(collectionName, cancellationToken);
    }

    public async Task<IEnumerable<DocumentProfile>> FindDocumentsAsync(
        string collectionName,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        return await _documentStorage.FindDocumentsAsync(collectionName, filter, cancellationToken);
    }

    public async Task<DataPipeline> MemorizeDocumentAsync(
        string collectionName,
        string documentId,
        string[] steps,
        string[]? tags = null,
        UploadRequest? uploadRequest = null,
        CancellationToken cancellationToken = default)
    {
        if (uploadRequest != null)
        {
            if (!_detecter.TryGetContentType(uploadRequest.FileName, out var contentType))
                throw new InvalidOperationException($"{uploadRequest.FileName} is dont know content type");

            var document = new DocumentProfile
            {
                Status = MemorizationStatus.NotMemorized,
                DocumentId = documentId,
                FileName = uploadRequest.FileName,
                ContentLength = uploadRequest.Content.Length,
                ContentType = contentType,
                Tags = tags ?? [],
                CreatedAt = DateTime.UtcNow,
            };
            await _documentStorage.UpsertDocumentAsync(collectionName, document, uploadRequest.Content, cancellationToken);
        }

        var pipeline = new DataPipeline
        {
            CollectionName = collectionName,
            DocumentId = documentId,
            Steps = steps.ToList(),
            StartedAt = DateTime.UtcNow,
        };
        _ = _orchestrator.ExecuteAsync(pipeline, cancellationToken);
        return pipeline;
    }

    public async Task UnMemorizeDocumentAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        await _vectorStorage.DeleteVectorsAsync(collectionName, documentId, cancellationToken);
        await _documentStorage.DeleteDocumentAsync(collectionName, documentId, cancellationToken);
    }

    public async Task SearchDocumentMemoryAsync(
        string collectionName,
        string query,
        float minScore = 0.0f,
        ulong limit = 5,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        float[] input = [];
        await _vectorStorage.SearchVectorsAsync(collectionName, input, minScore, limit, filter, cancellationToken);
    }

    #region Private Methods

    #endregion

}

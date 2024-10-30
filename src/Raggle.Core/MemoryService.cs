using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Memory.Document;
using Raggle.Abstractions.Memory.Vector;
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

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetCollectionListAsync(
        CancellationToken cancellationToken = default)
    {
        return await _documentStorage.GetCollectionListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string collectionName,
        ulong vectorSize,
        CancellationToken cancellationToken = default)
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

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        if (await _documentStorage.ExistCollectionAsync(collectionName, cancellationToken))
            await _documentStorage.DeleteCollectionAsync(collectionName, cancellationToken);

        if (await _vectorStorage.ExistCollectionAsync(collectionName, cancellationToken))
            await _vectorStorage.DeleteCollectionAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentRecord>> FindDocumentsAsync(
        string collectionName,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        return await _documentStorage.FindDocumentsAsync(collectionName, filter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DocumentRecord> UploadDocumentAsync(
        string collectionName,
        string documentId,
        string fileName,
        Stream content,
        string[]? tags = null,
        CancellationToken cancellationToken = default)
    {
        if (!_detecter.TryGetContentType(fileName, out var contentType))
            throw new InvalidOperationException($"{fileName} is dont know content type");

        var document = new DocumentRecord
        {
            Status = MemorizationStatus.NotMemorized,
            CollectionName = collectionName,
            DocumentId = documentId,
            FileName = fileName,
            ContentType = contentType,
            ContentLength = content.Length,
            CreatedAt = DateTime.UtcNow,
            Tags = tags ?? [],
        };

        return await _documentStorage.UpsertDocumentAsync(
            document: document,
            content: content,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DataPipeline> MemorizeDocumentAsync(
        string collectionName,
        string documentId,
        string[] steps,
        UploadRequest? uploadRequest = null,
        CancellationToken cancellationToken = default)
    {
        DocumentRecord document;
        if (uploadRequest != null)
        {
            document = await UploadDocumentAsync(collectionName, documentId, uploadRequest.FileName, uploadRequest.Content, uploadRequest.Tags, cancellationToken);
        }
        else
        {
            var filter = new MemoryFilter([documentId]);
            document = (await FindDocumentsAsync(collectionName, filter, cancellationToken)).FirstOrDefault()
                ?? throw new InvalidOperationException($"{documentId} is not found in {collectionName}");
        }

        var pipeline = new DataPipeline
        {
            Document = document,
            Steps = steps.ToList(),
            StartedAt = DateTime.UtcNow,
        };
        await _orchestrator.ExecuteAsync(pipeline, cancellationToken);
        return pipeline;
    }

    /// <inheritdoc />
    public async Task UnMemorizeDocumentAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        await _vectorStorage.DeleteVectorsAsync(collectionName, documentId, cancellationToken);
        await _documentStorage.DeleteDocumentAsync(collectionName, documentId, cancellationToken);
    }

    /// <inheritdoc />
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

}

using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Utils;
using Raggle.Core.Utils;
using System.Collections.Concurrent;
using System.Threading;

namespace Raggle.Core.Memory;

public class RaggleMemory : IRaggleMemory
{
    private readonly MemoryEmbeddingService _embedding;
    private readonly ContentTypeDetector _detecter = new();
    private readonly ConcurrentDictionary<string, IPipelineHandler> _handlers = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private readonly IDocumentStorage _document;
    private readonly IVectorStorage _vector;
    private readonly IPipelineOrchestrator _orchestrator;

    public RaggleMemory(IServiceProvider provider, RaggleMemoryConfig config)
    {
        _document = provider.GetRequiredKeyedService<IDocumentStorage>(config.DocumentStorageServiceKey);
        _vector = provider.GetRequiredKeyedService<IVectorStorage>(config.VectorDBServiceKey);
        _orchestrator = new PipelineOrchestrator(_document);
        _embedding = new MemoryEmbeddingService(config.EmbeddingModel, provider.GetRequiredKeyedService<IEmbeddingService>(config.EmbeddingServiceKey));
    }



    // =============== 삭제?? ===============
    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetCollectionListAsync(
        CancellationToken cancellationToken = default)
    {
        return await _document.GetCollectionListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sampleText = "Sample text to determine vector size";
            var embeddingResponse = await _embedding.EmbeddingsAsync([sampleText], cancellationToken);
            var vectorEmbedding = embeddingResponse.FirstOrDefault()?.Embedding
                ?? throw new InvalidOperationException("Failed to retrieve the embedding vector from the response.");

            await _vector.CreateCollectionAsync(collectionName, vectorEmbedding.Length, cancellationToken);
            await _document.CreateCollectionAsync(collectionName, cancellationToken);
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
        if (await _vector.CollectionExistsAsync(collectionName, cancellationToken))
            await _vector.DeleteCollectionAsync(collectionName, cancellationToken);

        if (await _document.CollectionExistsAsync(collectionName, cancellationToken))
            await _document.DeleteCollectionAsync(collectionName, cancellationToken);
    }

    // =============== 삭제?? ===============
    public async Task<IEnumerable<DocumentRecord>> FindDocumentsAsync(
        string collectionName,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        return await _document.FindDocumentsAsync(collectionName, filter, cancellationToken);
    }

    // =============== 삭제?? ===============
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

        return await _document.UpsertDocumentAsync(
            document: document,
            content: content,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DataPipeline> MemorizeDocumentAsync(
        string collectionName,
        string documentId,
        string[] steps,
        DocumentUploadRequest? uploadRequest = null,
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
        await _vector.DeleteVectorsAsync(collectionName, documentId, cancellationToken);
        await _document.DeleteDocumentAsync(collectionName, documentId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SearchDocumentMemoryAsync(
        string collectionName,
        string query,
        float minScore = 0.0f,
        int limit = 5,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _embedding.EmbeddingsAsync([query], cancellationToken);
        var embedding = response.FirstOrDefault()?.Embedding
            ?? throw new InvalidOperationException("Failed to retrieve the embedding vector from the response.");
        
        var results = await _vector.SearchVectorsAsync(collectionName, embedding, minScore, limit, filter, cancellationToken)
        return;
    }


    // =============== 추가?? ===============


    ///// <inheritdoc />
    //public bool TryGetHandler<T>(out T handler) where T : IPipelineHandler
    //{
    //    handler = _handlers.Values.OfType<T>().FirstOrDefault()!;
    //    return handler != null;
    //}

    ///// <inheritdoc />
    //public bool TryGetHandler(string name, out IPipelineHandler handler)
    //{
    //    return _handlers.TryGetValue(name, out handler!);
    //}

    /// <inheritdoc />
    public bool TryAddHandler(string stepName, IPipelineHandler handler)
    {
        return _handlers.TryAdd(stepName, handler);
    }

    /// <inheritdoc />
    public bool TryRemoveHandler(string stepName)
    {
        return _handlers.TryRemove(stepName, out _);
    }

    /// <inheritdoc />
    public bool IsLocked()
    {
        return _semaphore.CurrentCount == 0;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(DataPipeline pipeline, CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);

            pipeline.InitializeSteps();
            pipeline.Status = PipelineStatus.Processing;
            await UpsertPipelineAsync(pipeline, cancellationToken);
            await UpsertDocumentStatusAsync(MemorizationStatus.Memorizing, pipeline.Document, cancellationToken);

            while (pipeline.Status == PipelineStatus.Processing)
            {
                var stepName = pipeline.GetNextStepName();
                if (string.IsNullOrWhiteSpace(stepName))
                {
                    if (pipeline.Steps.Count == pipeline.CompletedSteps.Count)
                    {
                        pipeline.Status = PipelineStatus.Completed;
                        pipeline.CompletedAt = DateTime.UtcNow;
                        pipeline.ErrorMessage = "All steps are completed";
                        await UpsertPipelineAsync(pipeline, cancellationToken);
                        await UpsertDocumentStatusAsync(MemorizationStatus.Memorized, pipeline.Document, cancellationToken);
                    }
                    else
                    {
                        var message = "Something went wrong when processing the pipeline, steps are not completed";
                        await UpsertFaildPipelineAsync(pipeline, message, cancellationToken);
                        await UpsertDocumentStatusAsync(MemorizationStatus.FailedMemorization, pipeline.Document, cancellationToken);
                    }
                    break;
                }
                else if (_handlers.TryGetValue(stepName, out var handler))
                {
                    pipeline = await handler.ProcessAsync(pipeline, cancellationToken);
                    pipeline.CompleteStep(stepName);
                    await UpsertPipelineAsync(pipeline, cancellationToken);
                }
                else
                {
                    var message = $"Handler not found for step '{stepName}'";
                    await UpsertFaildPipelineAsync(pipeline, message, cancellationToken);
                    await UpsertDocumentStatusAsync(MemorizationStatus.FailedMemorization, pipeline.Document, cancellationToken);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            await UpsertFaildPipelineAsync(pipeline, ex.Message, cancellationToken);
            await UpsertDocumentStatusAsync(MemorizationStatus.FailedMemorization, pipeline.Document, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<DataPipeline> GetPipelineAsync(string collectionName, string documentId, CancellationToken cancellationToken = default)
    {
        var files = await _document.GetDocumentFilesAsync(collectionName, documentId, cancellationToken);
        var filename = files.Where(f => f.EndsWith(DocumentFileHelper.PipelineFileExtension)).FirstOrDefault()
            ?? throw new InvalidOperationException($"Pipeline file not found for {documentId}");

        var stream = await _document.ReadDocumentFileAsync(
            collectionName: collectionName,
            documentId: documentId,
            filePath: filename,
            cancellationToken: cancellationToken);
        var pipeline = JsonDocumentSerializer.Deserialize<DataPipeline>(stream);
        return pipeline;
    }

    #region Private Methods

    private async Task UpsertPipelineAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var filename = DocumentFileHelper.GetPipelineFileName(pipeline.Document.FileName);
        var stream = JsonDocumentSerializer.SerializeToStream(pipeline);
        await _document.WriteDocumentFileAsync(
            collectionName: pipeline.Document.CollectionName,
            documentId: pipeline.Document.DocumentId,
            filePath: filename,
            content: stream,
            cancellationToken: cancellationToken);
    }

    private async Task UpsertFaildPipelineAsync(DataPipeline pipeline, string message, CancellationToken cancellationToken)
    {
        pipeline.Status = PipelineStatus.Failed;
        pipeline.FailedAt = DateTime.UtcNow;
        pipeline.ErrorMessage = message;
        await UpsertPipelineAsync(pipeline, cancellationToken);
    }

    private async Task UpsertDocumentAsync(DocumentRecord document, CancellationToken cancellationToken)
    {
        await _document.UpsertDocumentAsync(
            document: document,
            cancellationToken: cancellationToken);
    }

    private async Task UpsertDocumentStatusAsync(MemorizationStatus status, DocumentRecord document, CancellationToken cancellationToken)
    {
        document.Status = status;
        document.LastUpdatedAt = DateTime.UtcNow;
        await UpsertDocumentAsync(document, cancellationToken);
    }

    #endregion
}

using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Utils;
using Raggle.Core.Utils;

namespace Raggle.Core.Memory;

public class RaggleMemory : IRaggleMemory
{
    private readonly IServiceProvider _services;
    private readonly IDocumentStorage _document;
    private readonly IVectorStorage _vector;

    private readonly ContentTypeDetector _detecter = new();
    private readonly Dictionary<string, IPipelineHandler> _handlers = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public RaggleMemory(IServiceProvider services, RaggleMemoryConfig config)
    {
        _services = services;
        _document = services.GetRequiredKeyedService<IDocumentStorage>(config.DocumentStorageServiceKey);
        _vector = services.GetRequiredKeyedService<IVectorStorage>(config.VectorStorageServiceKey);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string collectionName,
        object embedServiceKey,
        string embedModel,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sampleText = "Sample text to determine vector size";
            var embedding = await GetEmbeddingAsync(embedServiceKey, embedModel, sampleText, cancellationToken);

            await _vector.CreateCollectionAsync(collectionName, embedding.Length, cancellationToken);
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

    /// <inheritdoc />
    public async Task<DataPipeline> MemorizeDocumentAsync(
        string collectionName,
        string documentId,
        string fileName,
        Stream content,
        string[] steps,
        string[]? tags = null,
        CancellationToken cancellationToken = default)
    {
        await _document.WriteDocumentFileAsync(
            collectionName,
            documentId,
            fileName,
            content,
            overwrite: true,
            cancellationToken);

        _detecter.TryGetContentType(fileName, out var contentType);
        var pipeline = new DataPipeline
        {
            Document = new DocumentRecord
            {
                CollectionName = collectionName,
                ContentType = contentType,
                DocumentId = documentId,
                FileName = fileName,
                Tags = tags ?? [],
                Status = MemorizationStatus.Memorizing,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
            },
            Steps = steps.ToList(),
            StartedAt = DateTime.UtcNow,
        };

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
    public async Task GetNearestMemorySourceAsync(
        string collectionName,
        object embedServiceKey,
        string embedModel,
        string query,
        float minScore = 0,
        int limit = 5,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var embedding = await GetEmbeddingAsync(embedServiceKey, embedModel, query, cancellationToken);
        var results = await _vector.SearchVectorsAsync(collectionName,embedding, minScore, limit, filter, cancellationToken);
        return;
    }

    /// <inheritdoc />
    public void SetHandler<T>(string stepName) 
        where T : class, IPipelineHandler
    {
        var handler = ActivatorUtilities.CreateInstance<T>(_services);
        if (_handlers.ContainsKey(stepName))
            _handlers[stepName] = handler;
        else
            _handlers.Add(stepName, handler);
    }

    /// <inheritdoc />
    public void RemoveHandler(string stepName)
    {
        if (_handlers.ContainsKey(stepName))
            _handlers.Remove(stepName);
    }

    /// <inheritdoc />
    public bool IsLocked()
    {
        return _semaphore.CurrentCount == 0;
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

    private async Task<float[]> GetEmbeddingAsync(
        object embedServiceKey,
        string embedModel,
        string text,
        CancellationToken cancellationToken)
    {
        var embedService = _services.GetRequiredKeyedService<IEmbeddingService>(embedServiceKey);
        var response = await embedService.EmbeddingAsync(embedModel, text, cancellationToken);
        return response.Embedding;
    }

    #endregion
}

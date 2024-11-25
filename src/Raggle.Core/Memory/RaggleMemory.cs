using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Utils;
using Raggle.Core.Utils;

namespace Raggle.Core.Memory;

public class RaggleMemory : IRaggleMemory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentStorage _documentStorage;
    private readonly IVectorStorage _vectorStorage;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    // 제거??
    private readonly ContentTypeDetector _detector = new();

    public RaggleMemory(IServiceProvider services)
    {
        _serviceProvider = services;
        _documentStorage = services.GetRequiredService<IDocumentStorage>();
        _vectorStorage = services.GetRequiredService<IVectorStorage>();
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

            await _vectorStorage.CreateCollectionAsync(collectionName, embedding.Length, cancellationToken);
            await _documentStorage.CreateCollectionAsync(collectionName, cancellationToken);
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
        if (await _vectorStorage.CollectionExistsAsync(collectionName, cancellationToken))
            await _vectorStorage.DeleteCollectionAsync(collectionName, cancellationToken);

        if (await _documentStorage.CollectionExistsAsync(collectionName, cancellationToken))
            await _documentStorage.DeleteCollectionAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DataPipeline> MemorizeDocumentAsync(
        string collectionName,
        string documentId,
        string fileName,
        Stream content,
        object[] steps,
        string[]? tags = null,
        CancellationToken cancellationToken = default)
    {
        await _documentStorage.WriteDocumentFileAsync(
            collectionName,
            documentId,
            fileName,
            content,
            overwrite: true,
            cancellationToken);

        _detector.TryGetContentType(fileName, out var contentType);
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
                var stepKey = pipeline.GetNextStepKey();
                if (stepKey == null)
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

                var handler = _serviceProvider.GetKeyedService<IPipelineHandler>(stepKey);

                if (handler != null)
                {
                    pipeline = await handler.ProcessAsync(pipeline, cancellationToken);
                    pipeline.CompleteStep(stepKey);
                    await UpsertPipelineAsync(pipeline, cancellationToken);
                }
                else
                {
                    var message = $"Handler not found for step '{stepKey.ToString()}'";
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
        await _vectorStorage.DeleteVectorsAsync(collectionName, documentId, cancellationToken);
        await _documentStorage.DeleteDocumentAsync(collectionName, documentId, cancellationToken);
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
        var results = await _vectorStorage.SearchVectorsAsync(collectionName,embedding, minScore, limit, filter, cancellationToken);
        return;
    }

    /// <inheritdoc />
    public bool IsLocked()
    {
        return _semaphore.CurrentCount == 0;
    }

    /// <inheritdoc />
    public async Task<DataPipeline> GetPipelineAsync(string collectionName, string documentId, CancellationToken cancellationToken = default)
    {
        var files = await _documentStorage.GetDocumentFilesAsync(collectionName, documentId, cancellationToken);
        var filename = files.Where(f => f.EndsWith(DocumentFileHelper.PipelineFileExtension)).FirstOrDefault()
            ?? throw new InvalidOperationException($"Pipeline file not found for {documentId}");

        var stream = await _documentStorage.ReadDocumentFileAsync(
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
        await _documentStorage.WriteDocumentFileAsync(
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
        await _documentStorage.UpsertDocumentAsync(
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
        var embedService = _serviceProvider.GetRequiredKeyedService<IEmbeddingService>(embedServiceKey);
        var response = await embedService.EmbeddingAsync(embedModel, text, cancellationToken);
        return response.Embedding;
    }

    #endregion
}

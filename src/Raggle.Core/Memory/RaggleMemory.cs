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
        string embedServiceKey,
        string embedModelName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sampleText = "Sample text to determine vector size";
            var embedding = await GetEmbeddingAsync(embedServiceKey, embedModelName, sampleText, cancellationToken);

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
    public async Task MemorizeDocumentAsync(
        string collectionName,
        string documentId,
        string fileName,
        Stream? content,
        string[] steps,
        string[]? tags = null,
        IDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (content != null)
        {
            await _documentStorage.WriteDocumentFileAsync(
                collectionName,
                documentId,
                fileName,
                content,
                overwrite: true,
                cancellationToken);
        }

        _detector.TryGetContentType(fileName, out var contentType);
        var pipeline = new DataPipeline
        {
            CollectionName = collectionName,
            DocumentId = documentId,
            FileInfo = new Abstractions.Memory.FileInfo
            {
                FileName = fileName,
                ContentType = contentType
            },
            Steps = steps.ToList(),
            Metadata = metadata,
            Tags = tags
        };

        try
        {
            await _semaphore.WaitAsync(cancellationToken);

            // 초기화
            pipeline = pipeline.Start();
            await UpsertPipelineAsync(pipeline, cancellationToken);

            while (pipeline.StatusInfo.Status == PipelineStatus.Processing)
            {
                var currentStep = pipeline.CurrentStep;
                if (currentStep == null)
                {
                    pipeline = pipeline.Complete();
                    await UpsertPipelineAsync(pipeline, cancellationToken);
                    break;
                }
                else
                {
                    var handler = _serviceProvider.GetRequiredKeyedService<IPipelineHandler>(currentStep);
                    pipeline = await handler.ProcessAsync(pipeline, cancellationToken);
                    pipeline = pipeline.Next();
                    await UpsertPipelineAsync(pipeline, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            pipeline = pipeline.Failed(ex.Message);
            await UpsertPipelineAsync(pipeline, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task UnMemorizeDocumentAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        await _vectorStorage.DeleteVectorsAsync(collectionName, documentId, cancellationToken);
        await foreach(var file in _documentStorage.GetDocumentFilesAsync(collectionName, documentId, cancellationToken))
        {
            await _documentStorage.DeleteDocumentFileAsync(collectionName, documentId, file, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<object> GetNearestMemorySourceAsync(
        string collectionName,
        string embedServiceKey,
        string embedModelName,
        string query,
        float minScore = 0,
        int limit = 5,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var embedding = await GetEmbeddingAsync(embedServiceKey, embedModelName, query, cancellationToken);
        var results = await _vectorStorage.SearchVectorsAsync(collectionName,embedding, minScore, limit, filter, cancellationToken);
        return results;
    }

    /// <inheritdoc />
    public bool IsLocked()
    {
        return _semaphore.CurrentCount == 0;
    }

    /// <inheritdoc />
    public async Task<DataPipeline?> GetPipelineAsync(string collectionName, string documentId, CancellationToken cancellationToken = default)
    {
        await foreach (var file in _documentStorage.GetDocumentFilesAsync(collectionName, documentId, cancellationToken))
        {
            if (!file.EndsWith(DocumentFileHelper.PipelineFileExtension))
                continue;

            var stream = await _documentStorage.ReadDocumentFileAsync(collectionName, documentId, file, cancellationToken);
            var pipeline = JsonDocumentSerializer.Deserialize<DataPipeline>(stream);
            return pipeline;
        }
        return null;
    }

    #region Private Methods

    private async Task UpsertPipelineAsync(
        DataPipeline pipeline, 
        CancellationToken cancellationToken = default)
    {
        var filename = DocumentFileHelper.GetPipelineFileName(pipeline.FileInfo.FileName);
        var stream = JsonDocumentSerializer.SerializeToStream(pipeline);
        await _documentStorage.WriteDocumentFileAsync(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            filePath: filename,
            content: stream,
            cancellationToken: cancellationToken);
    }

    private async Task<float[]> GetEmbeddingAsync(
        string embedServiceKey,
        string embedModelName,
        string text,
        CancellationToken cancellationToken)
    {
        var embedService = _serviceProvider.GetRequiredKeyedService<IEmbeddingService>(embedServiceKey);
        var response = await embedService.EmbeddingAsync(embedModelName, text, cancellationToken);
        return response.Embedding;
    }

    #endregion
}

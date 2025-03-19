//using Microsoft.Extensions.DependencyInjection;
//using IronHive.Abstractions.Embedding;
//using IronHive.Abstractions.Memory;
//using IronHive.Core.Extensions;

//namespace IronHive.Core.Memory;

//public class MemoryService : IMemoryService
//{
//    private const string _pipelineSuffix = "pipeline";

//    private readonly IServiceProvider _serviceProvider;
//    private readonly IFileStorage _documentStorage;
//    private readonly IVectorStorage _vectorStorage;
//    private readonly IEmbeddingService _embeddingService;

//    private readonly SemaphoreSlim _semaphore = new(1, 1);

//    // 제거??
//    private readonly MimeTypeDetector _detector = new();

//    public MemoryService(IServiceProvider services)
//    {
//        _serviceProvider = services;
//        _documentStorage = services.GetRequiredService<IFileStorage>();
//        _vectorStorage = services.GetRequiredService<IVectorStorage>();
//        _embeddingService = services.GetRequiredService<IEmbeddingService>();
//    }

//    /// <inheritdoc />
//    public async Task CreateCollectionAsync(
//        string collectionName,
//        string embedModel,
//        CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            var sampleText = "Sample text to determine vector size";
//            var embedding = await _embeddingService.EmbedAsync(embedModel, sampleText, cancellationToken);

//            await _vectorStorage.CreateCollectionAsync(collectionName, embedding.Count(), cancellationToken);
//            await _documentStorage.CreateCollectionAsync(collectionName, cancellationToken);
//        }
//        catch
//        {
//            await DeleteCollectionAsync(collectionName, cancellationToken);
//            throw;
//        }
//    }

//    /// <inheritdoc />
//    public async Task DeleteCollectionAsync(
//        string collectionName,
//        CancellationToken cancellationToken = default)
//    {
//        if (await _vectorStorage.CollectionExistsAsync(collectionName, cancellationToken))
//            await _vectorStorage.DeleteCollectionAsync(collectionName, cancellationToken);

//        if (await _documentStorage.CollectionExistsAsync(collectionName, cancellationToken))
//            await _documentStorage.DeleteCollectionAsync(collectionName, cancellationToken);
//    }

//    /// <inheritdoc />
//    public async Task UploadDocumentAsync(
//        string collectionName,
//        string documentId,
//        string fileName,
//        Stream data,
//        CancellationToken cancellationToken = default)
//    {
//        await _documentStorage.WriteDocumentFileAsync(
//            collectionName: collectionName,
//            documentId: documentId,
//            filePath: fileName,
//            data: data,
//            overwrite: true,
//            cancellationToken: cancellationToken);
//    }

//    /// <inheritdoc />
//    public async Task DeleteDocumentAsync(
//        string collectionName, 
//        string documentId, 
//        CancellationToken cancellationToken = default)
//    {
//        await _vectorStorage.DeleteVectorsAsync(collectionName, documentId, cancellationToken);
//        await foreach (var file in _documentStorage.GetDocumentFilesAsync(collectionName, documentId, cancellationToken))
//        {
//            await _documentStorage.DeleteDocumentFileAsync(collectionName, documentId, file, cancellationToken);
//        }
//    }

//    /// <inheritdoc />
//    public async Task MemorizeDocumentAsync(
//        string collectionName,
//        string documentId,
//        string fileName,
//        string[] steps,
//        IDictionary<string, object>? options = null,
//        string[]? tags = null,
//        CancellationToken cancellationToken = default)
//    {
//        _detector.TryGetContentType(fileName, out var contentType);
//        var pipeline = new DataPipeline
//        {
//            FileName = fileName,
//            MimeType = contentType,
//            CollectionName = collectionName,
//            DocumentId = documentId,
//            Steps = steps.ToList(),
//            Options = options,
//            Tags = tags
//        };
//        await UpsertPipelineAsync(pipeline, cancellationToken);

//        try
//        {
//            await _semaphore.WaitAsync(cancellationToken);

//            pipeline = pipeline.Start();
//            await UpsertPipelineAsync(pipeline, cancellationToken);

//            while (pipeline.Status == PipelineStatus.Processing)
//            {
//                var currentStep = pipeline.CurrentStep;
//                if (currentStep == null)
//                {
//                    pipeline = pipeline.Complete();
//                    await UpsertPipelineAsync(pipeline, cancellationToken);
//                    break;
//                }
//                else
//                {
//                    var handler = _serviceProvider.GetRequiredKeyedService<IPipelineHandler>(currentStep);
//                    pipeline = await handler.ProcessAsync(pipeline, cancellationToken);
//                    pipeline = pipeline.Next();
//                    await UpsertPipelineAsync(pipeline, cancellationToken);
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            pipeline = pipeline.Failed(ex.Message);
//            await UpsertPipelineAsync(pipeline, cancellationToken);
//        }
//        finally
//        {
//            _semaphore.Release();
//        }
//    }

//    /// <inheritdoc />
//    public async Task UnMemorizeDocumentAsync(
//        string collectionName,
//        string documentId,
//        CancellationToken cancellationToken = default)
//    {
//        await _vectorStorage.DeleteVectorsAsync(collectionName, documentId, cancellationToken);
//    }

//    /// <inheritdoc />
//    public async Task<IEnumerable<ScoredVectorPoint>> SearchSimilarVectorsAsync(
//        string collectionName,
//        string embedModel,
//        string query,
//        float minScore = 0,
//        int limit = 5,
//        MemoryFilter? filter = null,
//        CancellationToken cancellationToken = default)
//    {
//        var embedding = await _embeddingService.EmbedAsync(embedModel, query, cancellationToken);
//        var results = await _vectorStorage.SearchVectorsAsync(collectionName,embedding, minScore, limit, filter, cancellationToken);
//        return results;
//    }

//    /// <inheritdoc />
//    public bool IsLocked()
//    {
//        return _semaphore.CurrentCount == 0;
//    }

//    /// <inheritdoc />
//    public async Task<DataPipeline> GetPipelineAsync(
//        string collectionName, 
//        string documentId,
//        CancellationToken cancellationToken = default)
//    {
//        return await _documentStorage.GetDocumentJsonFirstAsync<DataPipeline>(
//            collectionName: collectionName,
//            documentId: documentId, 
//            suffix: _pipelineSuffix,
//            cancellationToken: cancellationToken);
//    }

//    #region Private Methods

//    private async Task UpsertPipelineAsync(
//        DataPipeline pipeline,
//        CancellationToken cancellationToken = default)
//    {
//        await _documentStorage.UpsertDocumentJsonAsync(
//            collectionName: pipeline.CollectionName,
//            documentId: pipeline.DocumentId,
//            fileName: Path.GetFileNameWithoutExtension(pipeline.FileName),
//            suffix: _pipelineSuffix,
//            value: pipeline,
//            cancellationToken: cancellationToken);
//    }

//    #endregion
//}

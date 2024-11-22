using Raggle.Abstractions.Memory;
using Raggle.Core.Utils;
using System.Collections.Concurrent;

namespace Raggle.Core.Memory;

public class PipelineOrchestrator : IPipelineOrchestrator
{
    private readonly ConcurrentDictionary<string, IPipelineHandler> _handlers = [];
    private readonly IDocumentStorage _documentStorage;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public PipelineOrchestrator(IDocumentStorage documentStorage)
    {
        _documentStorage = documentStorage;
    }

    /// <inheritdoc />
    public bool TryGetHandler<T>(out T handler) where T : IPipelineHandler
    {
        handler = _handlers.Values.OfType<T>().FirstOrDefault()!;
        return handler != null;
    }

    /// <inheritdoc />
    public bool TryGetHandler(string name, out IPipelineHandler handler)
    {
        return _handlers.TryGetValue(name, out handler!);
    }

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
        throw new NotImplementedException();
        //try
        //{
        //    await _semaphore.WaitAsync(cancellationToken);

        //    pipeline.InitializeSteps();
        //    pipeline.Status = PipelineStatus.Processing;
        //    await UpsertPipelineAsync(pipeline, cancellationToken);
        //    await UpsertDocumentStatusAsync(MemorizationStatus.Memorizing, pipeline.Document, cancellationToken);

        //    while (pipeline.Status == PipelineStatus.Processing)
        //    {
        //        var stepName = pipeline.GetNextStepKey();
        //        if (string.IsNullOrWhiteSpace(stepName))
        //        {
        //            if (pipeline.Steps.Count == pipeline.CompletedSteps.Count)
        //            {
        //                pipeline.Status = PipelineStatus.Completed;
        //                pipeline.CompletedAt = DateTime.UtcNow;
        //                pipeline.ErrorMessage = "All steps are completed";
        //                await UpsertPipelineAsync(pipeline, cancellationToken);
        //                await UpsertDocumentStatusAsync(MemorizationStatus.Memorized, pipeline.Document, cancellationToken);
        //            }
        //            else
        //            {
        //                var message = "Something went wrong when processing the pipeline, steps are not completed";
        //                await UpsertFaildPipelineAsync(pipeline, message, cancellationToken);
        //                await UpsertDocumentStatusAsync(MemorizationStatus.FailedMemorization, pipeline.Document, cancellationToken);
        //            }
        //            break;
        //        }
        //        else if (_handlers.TryGetValue(stepName, out var handler))
        //        {
        //            pipeline = await handler.ProcessAsync(pipeline, cancellationToken);
        //            pipeline.CompleteStep(stepName);
        //            await UpsertPipelineAsync(pipeline, cancellationToken);
        //        }
        //        else
        //        {
        //            var message = $"Handler not found for step '{stepName}'";
        //            await UpsertFaildPipelineAsync(pipeline, message, cancellationToken);
        //            await UpsertDocumentStatusAsync(MemorizationStatus.FailedMemorization, pipeline.Document, cancellationToken);
        //            break;
        //        }
        //    }
        //}
        //catch(Exception ex)
        //{
        //    await UpsertFaildPipelineAsync(pipeline, ex.Message, cancellationToken);
        //    await UpsertDocumentStatusAsync(MemorizationStatus.FailedMemorization, pipeline.Document, cancellationToken);
        //}
        //finally 
        //{ 
        //    _semaphore.Release();
        //}
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

    #endregion
}

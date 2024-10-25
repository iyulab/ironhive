using Raggle.Abstractions.Memory;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raggle.Core;

public class PipelineOrchestrator : IPipelineOrchestrator
{
    private const string DefaultPipelineFileName = "__pipeline_status.json";
    private readonly ConcurrentDictionary<string, IPipelineHandler> _handlers = [];
    private readonly IDocumentStorage _documentStorage;

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
    public async Task ExecuteAsync(DataPipeline pipeline, CancellationToken cancellationToken = default)
    {
        pipeline.InitializeSteps();
        pipeline.Status = PipelineStatus.Processing;
        await UpsertPipelineAsync(pipeline, cancellationToken);

        while (pipeline.Status == PipelineStatus.Processing)
        {
            var stepName = pipeline.GetNextStepName();
            if (string.IsNullOrWhiteSpace(stepName))
            {
                if (pipeline.Steps.Count == pipeline.CompletedSteps.Count)
                {
                    pipeline.Status = PipelineStatus.Completed;
                }
                else
                {
                    pipeline.Status = PipelineStatus.Failed;
                    pipeline.Message = "The pipeline has steps that are not completed";
                }
                await UpsertPipelineAsync(pipeline, cancellationToken);
            }
            else if (_handlers.TryGetValue(stepName, out var handler))
            {
                pipeline = await handler.ProcessAsync(pipeline, cancellationToken);
                pipeline.CompleteStep(stepName);
            }
            else
            {
                pipeline.Status = PipelineStatus.Failed;
                pipeline.Message = $"Handler not found for step '{stepName}'";
                await UpsertPipelineAsync(pipeline, cancellationToken);
            }
        }
    }

    private async Task UpsertPipelineAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(pipeline);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        await _documentStorage.WriteDocumentFileAsync(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            filePath: DefaultPipelineFileName,
            content: stream,
            cancellationToken: cancellationToken);
    }
}

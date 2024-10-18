using Raggle.Abstractions.Memory;
using System.Text;
using System.Text.Json;

namespace Raggle.Core;

public class PipelineOrchestrator : IPipelineOrchestrator
{
    private const string DefaultPipelineFileName = "__pipeline_status.json";
    private readonly IDocumentStorage _documentStorage;
    private readonly Dictionary<string, IPipelineHandler> _handlers = [];

    public PipelineOrchestrator(IDocumentStorage documentStorage)
    {
        _documentStorage = documentStorage;
    }

    // 핸들러를 딕셔너리에 추가
    public bool TryAddHandler(string stepName, IPipelineHandler handler)
    {
        return _handlers.TryAdd(stepName, handler);
    }

    // 핸들러를 딕셔너리에서 제거
    public bool TryRemoveHandler(string stepName)
    {
        return _handlers.Remove(stepName);
    }

    // 파이프라인을 실행
    public async Task ExecuteAsync(DataPipeline pipeline, CancellationToken cancellationToken = default)
    {
        pipeline.InitializeSteps();
        pipeline.Status = DataPipelineStatus.Processing;
        while (pipeline.Status == DataPipelineStatus.Processing)
        {
            var stepName = pipeline.GetNextStepName();
            if (string.IsNullOrWhiteSpace(stepName))
            {
                if (pipeline.Steps.Count == pipeline.CompletedSteps.Count)
                {
                    pipeline.Status = DataPipelineStatus.Completed;
                }
                else
                {
                    pipeline.Status = DataPipelineStatus.Failed;
                    pipeline.Message = "The pipeline has steps that are not completed";
                }
            }
            else if (_handlers.TryGetValue(stepName, out var handler))
            {
                pipeline = await handler.ProcessAsync(pipeline, cancellationToken);
                pipeline.CompleteStep(stepName);
                await UpsertPipelineAsync(pipeline, cancellationToken);
            }
            else
            {
                pipeline.Status = DataPipelineStatus.Failed;
                pipeline.Message = $"Handler not found for step '{stepName}'";
            }
        }
    }

    private async Task UpsertPipelineAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(pipeline);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        await _documentStorage.WriteDocumentFileAsync(
            collection: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            filePath: DefaultPipelineFileName,
            content: stream,
            cancellationToken: cancellationToken);
    }
}

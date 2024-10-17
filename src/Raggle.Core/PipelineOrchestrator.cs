using Raggle.Abstractions.Memory;

namespace Raggle.Core;

public class PipelineOrchestrator
{
    private readonly Dictionary<string, IPipelineHandler> _handlers = [];

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
    public async Task<DataPipeline> ExecuteAsync(DataPipeline pipeline, CancellationToken cancellationToken = default)
    {
        pipeline.Validate();

        while (true)
        {
            var stepName = pipeline.GetNextStep();
            if (string.IsNullOrWhiteSpace(stepName))
            {
                if (pipeline.RemainingSteps.Count > 0)
                {
                    throw new InvalidOperationException("Pipeline has remaining steps but no step to execute.");
                }
                else
                {
                    pipeline.Status = DataPipelineStatus.Completed;
                    pipeline.LastUpdatedAt = DateTime.UtcNow;
                    break;
                }
            }
            else if (_handlers.TryGetValue(stepName, out var handler))
            {
                pipeline = await handler.ProcessAsync(pipeline, cancellationToken);
                pipeline.RemainingSteps.Remove(stepName);
                pipeline.CompletedSteps.Add(stepName);
                pipeline.LastUpdatedAt = DateTime.UtcNow;
            }
            else
            {
                throw new InvalidOperationException($"Handler for step '{stepName}' is not registered in Orchestra.");
            }
        }

        return pipeline;
    }
}

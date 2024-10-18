namespace Raggle.Abstractions.Memory;

public interface IPipelineOrchestrator
{
    bool TryAddHandler(string stepName, IPipelineHandler handler);

    bool TryRemoveHandler(string stepName);

    Task ExecuteAsync(DataPipeline pipeline, CancellationToken cancellationToken = default);
}

namespace Raggle.Abstractions.Memory;

public interface IPipelineOrchestrator
{
    bool TryGetHandler<T>(out T handler) where T : IPipelineHandler;

    bool TryGetHandler(string name, out IPipelineHandler handler);

    bool TryAddHandler(string stepName, IPipelineHandler handler);

    bool TryRemoveHandler(string stepName);

    Task ExecuteAsync(DataPipeline pipeline, CancellationToken cancellationToken = default);
}

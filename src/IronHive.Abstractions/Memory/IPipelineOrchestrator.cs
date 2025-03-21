namespace IronHive.Abstractions.Memory;

public interface IPipelineOrchestrator
{
    Task RunPipelineAsync(
        DataPipeline pipeline,
        CancellationToken cancellationToken = default);
}

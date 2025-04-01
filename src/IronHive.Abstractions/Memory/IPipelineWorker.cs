namespace IronHive.Abstractions.Memory;

public interface IPipelineWorker
{
    Task RunPipelineAsync(
        DataPipeline pipeline,
        CancellationToken cancellationToken = default);
}

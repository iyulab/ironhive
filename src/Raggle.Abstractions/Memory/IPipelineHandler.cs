namespace Raggle.Abstractions.Memory;

// 파이프라인 핸들러 인터페이스
public interface IPipelineHandler
{
    Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken);
}

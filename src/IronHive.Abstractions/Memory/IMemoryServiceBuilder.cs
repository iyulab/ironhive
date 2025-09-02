using IronHive.Abstractions.Pipelines;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// 메모리 파이프라인을 빌드하는 대리자입니다.
/// </summary>
/// <param name="builder">메모리 파이프라인 빌더</param>
/// <returns>메모리 파이프라인 실행기</returns>
public delegate IPipelineRunner<PipelineContext> PipelineBuildDelegate(
    IPipelineBuilder<PipelineContext, PipelineContext> builder);

/// <summary>
/// 메모리 서비스 빌더 인터페이스입니다.
/// </summary>
public interface IMemoryServiceBuilder
{
    /// <summary>
    /// 메모리 파이프라인을 설정합니다.
    /// </summary>
    IMemoryServiceBuilder SetPipeline(PipelineBuildDelegate configure);

    /// <summary>
    /// 메모리 서비스를 빌드합니다.
    /// </summary>
    IMemoryService Build();
}

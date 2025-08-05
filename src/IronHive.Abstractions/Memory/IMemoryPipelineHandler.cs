namespace IronHive.Abstractions.Memory;

/// <summary>
/// 메모리 파이프라인 처리를 위한 핸들러 인터페이스
/// </summary>
public interface IMemoryPipelineHandler
{
    /// <summary>
    /// 주어진 파이프라인을 비동기적으로 처리합니다.
    /// </summary>
    /// <param name="context">처리할 데이터 파이프라인 컨텍스트입니다.</param>
    /// <returns>
    /// 처리된 <see cref="MemoryPipelineContext"/>를 반환 합니다.
    /// </returns>
    Task<MemoryPipelineContext> ProcessAsync(
        MemoryPipelineContext context,
        CancellationToken cancellationToken = default);
}
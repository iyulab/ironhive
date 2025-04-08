namespace IronHive.Abstractions.Memory;

/// <summary>
/// 작업 수행을 담당하는 인터페이스입니다.
/// </summary>
public interface IPipelineWorker : IDisposable
{
    /// <summary>
    /// 지속적으로 작업을 수행하는 메서드입니다.
    /// </summary>
    /// <param name="cancellationToken">작업 취소/중지 토큰입니다.</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 컨텍스트에서 작업을 수행하는 메서드입니다.
    /// </summary>
    /// <param name="source">작업을 수행할 메모리 소스입니다.</param>
    /// <param name="target">작업 결과를 저장할 메모리 타겟입니다.</param>
    /// <param name="handlerOptions">작업 핸들러에 전달할 옵션입니다.</param>
    /// <param name="cancellationToken">작업 취소/중지 토큰입니다.</param>
    Task ExecuteAsync(PipelineRequest request, CancellationToken cancellationToken = default);
}

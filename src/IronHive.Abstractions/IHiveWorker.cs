namespace IronHive.Abstractions;

/// <summary>
/// 작업 수행을 담당하는 인터페이스입니다.
/// </summary>
public interface IHiveWorker<T>
{
    /// <summary>
    /// 지속적으로 작업을 수행하는 메서드입니다.
    /// </summary>
    /// <param name="cancellationToken">작업 취소/중지 토큰입니다.</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 컨텍스트에서 작업을 수행하는 메서드입니다.
    /// </summary>
    /// <param name="context">작업을 수행할 컨텍스트입니다.</param>
    /// <param name="cancellationToken">작업 취소/중지 토큰입니다.</param>
    Task ExecuteAsync(T context, CancellationToken cancellationToken = default);
}

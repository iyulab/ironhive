namespace IronHive.Abstractions.Memory;

/// <summary>
/// 파이프라인 작업자의 상태를 나타내는 열거형입니다.
/// </summary>
public enum PipelineWorkerStatus
{
    /// <summary>
    /// 작업이 실행된 상태입니다.
    /// </summary>
    Started,

    /// <summary>
    /// 작업이 중지된 상태입니다.
    /// </summary>
    Stopped,

    /// <summary>
    /// 작업 중지 요청이 들어온 상태입니다.
    /// </summary>
    StopRequested
}

/// <summary>
/// 작업 수행을 담당하는 인터페이스입니다.
/// </summary>
public interface IMemoryPipelineWorker : IDisposable
{
    /// <summary>
    /// 현재 작업이 실행 중인지 여부를 나타냅니다.
    /// </summary>
    PipelineWorkerStatus Status { get; }

    /// <summary>
    /// 지속적으로 작업을 수행하는 메서드입니다.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// 실행중인 작업을 중지하는 메서드입니다.
    /// </summary>
    /// <param name="force">작업을 즉시 중단합니다.</param>
    Task StopAsync(bool force = false);

    /// <summary>
    /// 지정된 요청에 따라 작업을 수행하는 메서드입니다.
    /// </summary>
    Task ExecuteAsync(MemoryPipelineRequest request, CancellationToken cancellationToken = default);
}

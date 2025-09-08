namespace IronHive.Abstractions.Memory;

/// <summary>
/// 작업자의 상태를 나타내는 열거형입니다.
/// </summary>
public enum MemoryWorkerState
{
    StartRequested = 0,        // 작업 시작 요청됨
    Idle = 1,                  // 메시지 없음, 대기 상태 (Delay)
    Processing = 2,            // 메시지 처리 중
    StopRequested = 3,         // 작업 중지 요청됨
    Stopped = 4,               // 작업 중지됨
}

/// <summary>
/// 작업 수행을 담당하는 가장 작은 단위의 작업자(Worker)입니다.
/// </summary>
public interface IMemoryWorker : IDisposable
{
    /// <summary>
    /// 현재 작업자의 상태를 나타냅니다.
    /// </summary>
    MemoryWorkerState State { get; }

    /// <summary>
    /// 작업자의 상태가 변경될 때 발생하는 이벤트입니다.
    /// </summary>
    event EventHandler<MemoryWorkerState>? StateChanged;

    /// <summary>
    /// 지속적으로 작업을 수행하는 메서드입니다.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// 실행중인 작업을 중지하는 메서드입니다.
    /// </summary>
    /// <param name="force">
    /// 만약 true이면 현재 작업중인 작업을 강제로 중지합니다. 
    /// false이면 현재 작업이 완료될 때까지 기다린후 중지합니다.
    /// </param>
    Task StopAsync(bool force = false);
}

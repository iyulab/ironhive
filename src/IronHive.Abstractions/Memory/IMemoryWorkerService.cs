namespace IronHive.Abstractions.Memory;

/// <summary>
/// 메모리 기반 작업을 처리하는 워커들의 수명을 관리하고,
/// 워크로드에 따라 자동으로 확장 및 축소할 수 있는 워커 매니저의 인터페이스입니다.
/// </summary>
public interface IMemoryWorkerService : IDisposable
{
    /// <summary>
    /// 현재 워커 서비스가 실행 중인지 여부를 나타냅니다.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 현재 실행 중인 워커의 수를 반환합니다.
    /// </summary>
    int CurrentWorkers { get; }

    /// <summary>
    /// 유지할 최소 워커 수입니다.
    /// 워커가 이 수 이하로 줄어들지 않도록 보장합니다.
    /// </summary>
    int MinWorkers { get; set; }

    /// <summary>
    /// 허용 가능한 최대 워커 수입니다.
    /// 워커가 이 수를 초과하지 않도록 제한합니다.
    /// </summary>
    int MaxWorkers { get; set; }

    /// <summary>
    /// 워커 매니저를 시작하고, 초기 워커들을 생성하여 실행합니다.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// 모든 워커를 중지시키고 매니저를 종료합니다.
    /// </summary>
    /// <param name="force">즉시 강제 종료할지 여부 (기본: false)</param>
    Task StopAsync(bool force = false);
}
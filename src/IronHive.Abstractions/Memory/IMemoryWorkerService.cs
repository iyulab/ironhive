namespace IronHive.Abstractions.Memory;

/// <summary>
/// 메모리 기반 작업을 처리하는 워커들의 수명을 관리하고,
/// 워크로드에 따라 자동으로 워커들의 숫자가 확장 및 축소되는 워커 서비스입니다.
/// </summary>
public interface IMemoryWorkerService : IDisposable
{
    /// <summary>
    /// 현재 서비스가 실행 중인지 여부를 나타냅니다.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 현재 실행 중인 워커의 수를 반환합니다.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// 최소 실행 가능한 워커의 수를 설정하거나 가져옵니다.
    /// </summary>
    int MinCount { get; set; }

    /// <summary>
    /// 최대 실행 가능한 워커의 수를 설정하거나 가져옵니다.
    /// </summary>
    int MaxCount { get; set; }

    /// <summary>
    /// 설정에 따라 서비스를 시작하고, 초기 워커들을 생성하여 실행합니다.
    /// <param name="interval">워커들이 작업 큐를 확인하는 간격 (기본값: 5초)</param>
    /// </summary>
    Task StartAsync(TimeSpan? interval = null);

    /// <summary>
    /// 모든 워커를 중지시키고 대기합니다.
    /// </summary>
    /// <param name="force">
    /// true이면 현재 작업중인 작업들을 강제로 중지합니다.
    /// false이면 현재 작업들이 완료될 때까지 기다린후 중지합니다.
    /// </param>
    Task StopAsync(bool force = false);
}
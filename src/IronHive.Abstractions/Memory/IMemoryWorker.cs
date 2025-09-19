using IronHive.Abstractions.Workflow;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// 메모리 기반 작업을 처리하는 워커 서비스입니다.
/// </summary>
public interface IMemoryWorker : IDisposable
{
    /// <summary>
    /// 현재 서비스가 실행 중인지 여부를 나타냅니다.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 현재 실행 중인 작업의 수를 나타냅니다.
    /// </summary>
    int RunningTaskCount { get; }

    /// <summary>
    /// 최대 실행 가능한 작업 수를 설정합니다. 기본값은 5입니다.
    /// </summary>
    int MaxConcurrentTasks { get; set; }

    /// <summary>
    /// 파이프라인의 진행 상황을 알리는 이벤트
    /// </summary>
    event EventHandler<WorkflowEventArgs<MemoryContext>>? Progressed;

    /// <summary>
    /// 설정에 따라 작업을 수행합니다.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// 모든 작업을 중지시키고 대기합니다.
    /// </summary>
    /// <param name="force">
    /// true이면 현재 작업중인 작업들을 강제로 중지합니다.
    /// false이면 현재 작업들이 완료될 때까지 기다린후 중지합니다.
    /// </param>
    Task StopAsync(bool force = false);
}
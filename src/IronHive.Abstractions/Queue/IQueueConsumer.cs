namespace IronHive.Abstractions.Queue;

/// <summary>
/// 구현정책에 따라 큐에서 메시지를 지속적으로 처리하는 런너(consumer) 제어용 서비스입니다.
/// </summary>
public interface IQueueConsumer : IDisposable
{
    /// <summary>
    /// 현재 소비 루프가 실행 중인지 여부입니다.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 소비 루프를 시작합니다. 이미 실행 중이라면 추가 실행을 시도하지 않습니다.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 소비 루프를 중지합니다. 처리 중인 메시지가 있다면 완료 후 중지됩니다.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}

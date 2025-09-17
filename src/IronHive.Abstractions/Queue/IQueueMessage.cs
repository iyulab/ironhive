namespace IronHive.Abstractions.Queue;

/// <summary>
/// 큐에서 꺼낸 단일 메시지를 나타냅니다.
/// </summary>
public interface IQueueMessage<T>
{
    /// <summary>
    /// 메시지 본문(페이로드)입니다.
    /// </summary>
    T Body { get; }

    /// <summary>
    /// 메시지 처리를 성공으로 확정(ACK)하고 큐에서 영구 제거합니다.
    /// </summary>
    Task CompleteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 메시지를 다시 큐에 넣습니다(재큐). 보통 재시도 목적입니다.
    /// </summary>
    Task RequeueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 메시지를 데드 레터 큐(DLQ)로 이동시킵니다.
    /// 더 이상 자동 재시도하지 않고 원인 분석을 위해 보관하는 것이 일반적입니다.
    /// </summary>
    /// <param name="reason">데드 처리 사유(로깅/모니터링용)</param>
    Task DeadAsync(string reason, CancellationToken cancellationToken = default);
}

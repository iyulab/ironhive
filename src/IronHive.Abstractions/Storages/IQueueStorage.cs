namespace IronHive.Abstractions.Storages;

/// <summary>
/// 큐를 저장하고 관리하는 스토리지를 나타내는 인터페이스입니다.
/// </summary>
public interface IQueueStorage : IDisposable
{
    /// <summary>
    /// 스토리지의 이름을 가져옵니다.
    /// </summary>
    string StorageName { get; }

    /// <summary>
    /// 큐에 있는 메시지의 개수를 가져옵니다. (처리 중인 메시지는 제외)
    /// </summary>
    Task<int> CountAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 큐에 있는 모든 메시지를 삭제합니다. (처리 중인 메시지도 포함)
    /// </summary>
    Task ClearAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 타입의 메시지를 큐에 추가합니다.
    /// </summary>
    Task EnqueueAsync<T>(
        T message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 메시지를 큐에서 하나 꺼내고, 해당 메시지를 처리 중 상태로 변환합니다.
    /// 반드시 명시적으로 확인(Ack)을 해야 영구적으로 제거됩니다.
    /// </summary>
    /// <returns>최근 메시지, 또는 큐가 비어 있으면 null 반환</returns>
    Task<QueueMessage<T>?> DequeueAsync<T>(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 이전에 꺼낸 메시지를 확인(Acknowledge)하여 처리 중 상태에서 제거합니다.
    /// </summary>
    /// <param name="messageTag">확인할 메시지의 태그</param>
    Task AckAsync(
        object messageTag,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 이전에 꺼낸 메시지를 거부(Reject)합니다. 필요 시 다시 큐에 넣을 수 있습니다.
    /// </summary>
    /// <param name="messageTag">거부할 메시지의 태그</param>
    /// <param name="requeue">true이면 메시지를 큐에 다시 추가합니다. false이면 메시지를 영구적으로 제거합니다.</param>
    Task NackAsync(
        object messageTag,
        bool requeue = false,
        CancellationToken cancellationToken = default);
}

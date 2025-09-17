using IronHive.Abstractions.Registries;

namespace IronHive.Abstractions.Queue;

/// <summary>
/// 큐를 저장/조회/소비하는 스토리지 서비스입니다.
/// <para>
public interface IQueueStorage : IStorageItem
{
    /// <summary>
    /// 큐에서 메시지를 지속적으로 가져와 처리하는 소비자(Consumer)를 생성합니다.
    /// </summary>
    /// <typeparam name="T">소비할 메시지의 페이로드 타입</typeparam>
    /// <param name="onReceived">
    /// 메시지를 가져왔을 때 호출되는 비동기 콜백.
    /// 콜백이 정상 완료되면 메시지는 <see cref="IQueueMessage{T}.CompleteAsync(CancellationToken)"/>로 확정하는 것이 일반적입니다.
    /// 예외가 발생하면 구현체 정책에 따라 재시도/재큐/데드레터로 전환될 수 있습니다.
    /// </param>
    /// <param name="cancellationToken">소비자 생성 작업을 취소하기 위한 토큰</param>
    /// <returns>시작/중지를 제어할 수 있는 <see cref="IQueueConsumer"/> 인스턴스</returns>
    Task<IQueueConsumer> CreateConsumerAsync<T>(
        Func<IQueueMessage<T>, Task> onReceived,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 현재 큐에서 즉시 가져올 수 있는(잠금/처리 중 아님) 메시지 개수를 반환합니다.
    /// </summary>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>가져올 수 있는 메시지 수</returns>
    Task<int> CountAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 큐에 존재하는 모든 메시지를 제거합니다.
    /// (대기/잠금/데드레터 포함 — 구현체 정책에 따름)
    /// </summary>
    /// <param name="cancellationToken">취소 토큰</param>
    Task ClearAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 타입의 메시지를 큐에 추가합니다.
    /// </summary>
    /// <typeparam name="T">메시지 페이로드 타입</typeparam>
    /// <param name="message">큐에 적재할 메시지 본문</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task EnqueueAsync<T>(
        T message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 메시지를 하나 꺼내어 '처리 중(In-Process)' 상태로 전환합니다.
    /// 명시적으로 <see cref="IQueueMessage{T}.CompleteAsync(System.Threading.CancellationToken)"/>를 호출해야 영구 삭제됩니다.
    /// </summary>
    /// <typeparam name="T">메시지 페이로드 타입</typeparam>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>가장 최근(또는 구현체 정책에 따른) 메시지. 없으면 <c>null</c>.</returns>
    Task<IQueueMessage<T>?> DequeueAsync<T>(
        CancellationToken cancellationToken = default);
}
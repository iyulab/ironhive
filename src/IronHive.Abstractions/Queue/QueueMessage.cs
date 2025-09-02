namespace IronHive.Abstractions.Queue;

/// <summary>
/// 큐에서 전달되는 메시지를 나타내는 제네릭 클래스입니다.
/// </summary>
public class QueueMessage<T>
{
    /// <summary>
    /// 실제 전달되는 메시지 데이터입니다.
    /// </summary>
    public required T Payload { get; set; }

    /// <summary>
    /// 해당 메시지를 식별하거나 확인(ACK)할 때 사용하는 태그입니다.
    /// </summary>
    public required object Tag { get; set; }
}
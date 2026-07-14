namespace IronHive.Abstractions.Messages;

/// <summary>
/// 메시지 파이프라인 전반(MessageRequest → MessageContext → MessageResponse/StreamingMessageDoneResponse)에서
/// 공유되는 임의 데이터 저장소입니다.
/// </summary>
public class MessageContextItems : Dictionary<string, object?>
{
    public MessageContextItems()
        : base(StringComparer.Ordinal)
    { }

    public MessageContextItems(MessageContextItems source)
        : base(source, StringComparer.Ordinal)
    { }

    public T? Get<T>(string key) =>
        TryGetValue(key, out var value) && value is T typed ? typed : default;
}

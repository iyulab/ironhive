using System.Collections;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 메시지를 담는 컬렉션 구현입니다.
/// </summary>
public sealed class MessageCollection : ICollection<IMessage>
{
    private readonly List<IMessage> _items;

    public MessageCollection()
    {
        _items = new();
    }

    public MessageCollection(IEnumerable<IMessage> messages)
    {
        _items = new(messages);
    }

    /// <summary>
    /// 마지막 메시지가 T 타입이면 그 메시지를 반환합니다.
    /// 그렇지 않으면 새로운 T 타입의 메시지를 생성하여 컬렉션에 추가하고 반환합니다.
    /// </summary>
    public T LastOrCreate<T>() where T : IMessage, new()
    {
        if (_items.Count == 0)
        {
            var message = new T();
            _items.Add(message);
            return message;
        }

        if (_items[^1] is T lastMessage)
        {
            return lastMessage;
        }
        else
        {
            var message = new T();
            _items.Add(message);
            return message;
        }
    }

    #region ICollection Implementations

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public IMessage this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public void Add(IMessage message) => _items.Add(message);

    public bool Remove(IMessage message) => _items.Remove(message);

    public bool Contains(IMessage message) => _items.Contains(message);

    public void CopyTo(IMessage[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public void Clear() => _items.Clear();

    public IEnumerator<IMessage> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}
using System.Collections;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 메시지를 담는 컬렉션 구현입니다.
/// 아직 별다른 기능은 없습니다.
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
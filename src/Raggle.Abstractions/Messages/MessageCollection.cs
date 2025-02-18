using System.Collections;

namespace Raggle.Abstractions.Messages;

public sealed class MessageCollection : ICollection<Message>
{
    private readonly List<Message> _items = [];

    public void Append(MessageRole role, IMessageContent content)
    {
        if (_items.Last().Role == role)
            _items.Last().Content.Add(content);
        else
            Add(role, content);
    }

    public void Append(MessageRole role, MessageContentCollection content)
    {
        if (_items.Last().Role == role)
            _items.Last().Content.AddRange(content);
        else
            Add(role, content);
    }

    public void Add(MessageRole role, IMessageContent content)
    {
        _items.Add(new Message
        {
            Role = role,
            Content = new MessageContentCollection { content },
            TimeStamp = DateTime.UtcNow
        });
    }

    public void Add(MessageRole role, MessageContentCollection content)
    {
        _items.Add(new Message
        {
            Role = role,
            Content = content,
            TimeStamp = DateTime.UtcNow
        });
    }

    #region ICollection Implementations

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public Message this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public void Add(Message message) => _items.Add(message);

    public bool Remove(Message message) => _items.Remove(message);

    public bool Contains(Message message) => _items.Contains(message);

    public void CopyTo(Message[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public void Clear() => _items.Clear();

    public IEnumerator<Message> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}
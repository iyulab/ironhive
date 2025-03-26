using System.Collections;

namespace IronHive.Abstractions.Messages;

public sealed class MessageCollection : ICollection<Message>
{
    private readonly List<Message> _items;

    public MessageCollection()
    {
        _items = new();
    }

    public MessageCollection(IEnumerable<Message> messages)
    {
        _items = new(messages);
    }

    public void AddUserMessage(IMessageContent content)
    {
        Add(MessageRole.User, content);
    }

    public void AddAssistantMessage(IMessageContent content)
    {
        Add(MessageRole.Assistant, content);
    }

    public void Add(MessageRole role, IMessageContent content)
    {
        if (_items.Last().Role == role)
        {
            _items.Last().Content.Add(content);
        }
        else
        {
            var message = new Message { Role = role };
            message.Content.Add(content);
            _items.Add(message);
        }
    }

    public void Add(MessageRole role, IEnumerable<IMessageContent> content)
    {
        if (_items.Last().Role == role)
        {
            _items.Last().Content.AddRange(content);
        }
        else
        {
            var message = new Message { Role = role };
            message.Content.AddRange(content);
            Add(message);
        }
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
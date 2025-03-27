using System.Collections;

namespace IronHive.Abstractions.Messages;

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

    public void Append(IUserContent content)
    {
        if (_items.Last() is UserMessage user)
        {
            user.Content.Add(content);
        }
        else
        {
            var message = new UserMessage();
            message.Content.Add(content);
            _items.Add(message);
        }
    }

    public void Append(IEnumerable<IUserContent> content)
    {
        if (_items.Last() is UserMessage user)
        {
            user.Content.AddRange(content);
        }
        else
        {
            var message = new UserMessage();
            message.Content.AddRange(content);
            _items.Add(message);
        }
    }

    public void Append(IAssistantContent content)
    {
        if (_items.Last() is AssistantMessage assistant)
        {
            assistant.Content.Add(content);
        }
        else
        {
            var message = new AssistantMessage();
            message.Content.Add(content);
            _items.Add(message);
        }
    }

    public void Append(IEnumerable<IAssistantContent> content)
    {
        if (_items.Last() is AssistantMessage assistant)
        {
            assistant.Content.AddRange(content);
        }
        else
        {
            var message = new AssistantMessage();
            message.Content.AddRange(content);
            _items.Add(message);
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
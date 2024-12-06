using System.Collections;

namespace Raggle.Abstractions.Messages;

public sealed class MessageCollection : ICollection<Message>
{
    private readonly List<Message> _items = [];

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public Message this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public void AddUserMessage(IMessageContent content)
    {
        AddMessage(MessageRole.User, content);
    }

    public void AddUserMessage(MessageContentCollection content)
    {
        AddMessage(MessageRole.User, content);
    }

    // 어시스턴트 메시지를 추가합니다. 이전 메시지가 어시스턴트 메시지인 경우, 같은 메시지에 추가합니다.
    public void AddAssistantMessage(IMessageContent content)
    {
        if (_items.Last().Role == MessageRole.Assistant)
            _items.Last().Content.Add(content);
        else
            AddMessage(MessageRole.Assistant, content);
    }

    // 어시스턴트 메시지를 추가합니다. 이전 메시지가 어시스턴트 메시지인 경우, 같은 메시지에 추가합니다.
    public void AddAssistantMessage(MessageContentCollection content)
    {
        if (_items.Last().Role == MessageRole.Assistant)
            _items.Last().Content.AddRange(content);
        else
            AddMessage(MessageRole.Assistant, content);
    }

    public void AddMessage(MessageRole role, IMessageContent content)
    {
        _items.Add(new Message
        {
            Role = role,
            Content = new MessageContentCollection { content },
            TimeStamp = DateTime.UtcNow
        });
    }

    public void AddMessage(MessageRole role, MessageContentCollection content)
    {
        _items.Add(new Message
        {
            Role = role,
            Content = content,
            TimeStamp = DateTime.UtcNow
        });
    }

    public MessageCollection Clone()
    {
        var clone = new MessageCollection();
        foreach (var message in _items)
        {
            clone.Add(message.Clone());
        }
        return clone;
    }

    #region Implementation Methods

    public void Add(Message message) => _items.Add(message);

    public bool Remove(Message message) => _items.Remove(message);

    public bool Contains(Message message) => _items.Contains(message);

    public void CopyTo(Message[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public void Clear() => _items.Clear();

    public IEnumerator<Message> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}
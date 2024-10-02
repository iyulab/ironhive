using System.Collections;

namespace Raggle.Abstractions.Models;

public class ChatHistory : ICollection<IMessage>
{
    private readonly List<IMessage> _messages = [];

    public int Count => _messages.Count;

    public bool IsReadOnly => false;

    public ChatHistory Clone()
    {
        var clone = new ChatHistory();
        foreach (var message in _messages)
        {
            clone.Add(message);
        }
        return clone;
    }

    public void Add(IMessage message)
    {
        _messages.Add(message);
    }

    public bool Remove(IMessage message)
    {
        return _messages.Remove(message);
    }

    public bool Contains(IMessage message)
    {
        return _messages.Contains(message);
    }

    public void CopyTo(IMessage[] array, int index)
    {
        _messages.CopyTo(array, index);
    }

    public void Clear()
    {
        _messages.Clear();
    }

    public IEnumerator<IMessage> GetEnumerator()
    {
        return _messages.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _messages.GetEnumerator();
    }
}

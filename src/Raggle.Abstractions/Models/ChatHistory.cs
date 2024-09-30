using System.Collections;

namespace Raggle.Abstractions.Models;

public class ChatHistory : ICollection<ChatMessage>
{
    private readonly List<ChatMessage> _messages = [];

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

    public void Add(ChatMessage message)
    {
        _messages.Add(message);
    }

    public bool Remove(ChatMessage message)
    {
        return _messages.Remove(message);
    }

    public bool Contains(ChatMessage message)
    {
        return _messages.Contains(message);
    }

    public void CopyTo(ChatMessage[] array, int index)
    {
        _messages.CopyTo(array, index);
    }

    public void Clear()
    {
        _messages.Clear();
    }

    public IEnumerator<ChatMessage> GetEnumerator()
    {
        return _messages.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _messages.GetEnumerator();
    }
}

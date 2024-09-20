using System.Collections;

namespace Raggle.Abstractions.Models;

public class ChatSession : ICollection<ChatMessage>
{
    private readonly List<ChatMessage> _messages = [];

    public int Count => _messages.Count;

    public bool IsReadOnly => false;

    public void Add(ChatMessage item)
    {
        _messages.Add(item);
    }

    public void AddUserMessage(string text, string[] images)
    {
        var contents = new List<ContentBlock>();
        foreach (var image in images)
        {
            contents.Add(new ImageContentBlock { Data = image });
        }
        contents.Add(new TextContentBlock { Text = text });
        Add(new ChatMessage { Role = ChatRole.User, Contents = contents });
    }

    public void AppendAssistantMessage(ToolContentBlock tool)
    {

    }

    public void AppendAssistantMessage(TextContentBlock text)
    {

    }

    public bool Remove(ChatMessage item)
    {
        return _messages.Remove(item);
    }

    public bool Contains(ChatMessage item)
    {
        return _messages.Contains(item);
    }

    public void CopyTo(ChatMessage[] array, int index)
    {
        _messages.CopyTo(array, index);
    }

    public ChatSession Clone()
    {
        var clone = new ChatSession();
        foreach (var message in _messages)
        {
            clone.Add(message);
        }
        return clone;
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

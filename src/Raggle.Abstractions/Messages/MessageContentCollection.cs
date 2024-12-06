using Raggle.Abstractions.Tools;
using System.Collections;

namespace Raggle.Abstractions.Messages;

/// <summary>
/// IMessageContent를 담는 컬렉션 구현입니다.
/// </summary>
public class MessageContentCollection : ICollection<IMessageContent>
{
    private readonly List<IMessageContent> _items = new();

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public IMessageContent this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public void AddText(string? text)
    {
        Add(new TextContent 
        { 
            Index = _items.Count,
            Text = text 
        });
    }

    public void AddImage(string? data)
    {
        Add(new ImageContent 
        { 
            Index = _items.Count,
            Data = data
        });
    }

    public void AddTool(string? id, string? name, string? arguments, FunctionResult? result)
    {
        Add(new ToolContent 
        {
            Index = _items.Count,
            Id = id,
            Name = name, 
            Arguments = arguments,
            Result = result
        });
    }

    #region Implementation Methods

    public void AddRange(IEnumerable<IMessageContent> collection) => _items.AddRange(collection);

    public void Add(IMessageContent item) => _items.Add(item);

    public void Clear() => _items.Clear();

    public bool Contains(IMessageContent item) => _items.Contains(item);

    public void CopyTo(IMessageContent[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public bool Remove(IMessageContent item) => _items.Remove(item);

    public IEnumerator<IMessageContent> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

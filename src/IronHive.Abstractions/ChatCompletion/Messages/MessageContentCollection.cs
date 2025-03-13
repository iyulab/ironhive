using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace IronHive.Abstractions.ChatCompletion.Messages;

/// <summary>
/// IMessageContent를 담는 컬렉션 구현입니다.
/// </summary>
public class MessageContentCollection : ICollection<IMessageContent>
{
    private readonly List<IMessageContent> _items = new();

    /// <summary>
    /// TextContent를 추가합니다.
    /// </summary>
    public void AddText(string? value)
    {
        Add(new TextContent
        {
            Value = value
        });
    }

    /// <summary>
    /// ImageContent를 추가합니다.
    /// </summary>
    public void AddImage(string? data)
    {
        Add(new ImageContent
        {
            Data = data
        });
    }

    /// <summary>
    /// ToolContent를 추가합니다.
    /// </summary>
    public void AddTool(string? id, string? name, string? arguments, string? result)
    {
        Add(new ToolContent
        {
            Id = id,
            Name = name,
            Arguments = arguments,
            Result = result
        });
    }

    /// <summary>
    /// 배열을 추가합니다.
    /// </summary>
    public void AddRange(IEnumerable<IMessageContent> collection)
    {
        foreach (var item in collection)
        {
            Add(item);
        }
    }

    /// <summary>
    /// 해당 인덱스의 아이템을 가져옵니다.
    /// </summary>
    public bool TryGetAt<T>(int index, [MaybeNullWhen(false)] out T item) 
        where T : IMessageContent
    {
        if (index >= 0 && index < _items.Count && _items[index] is T typedItem)
        {
            item = typedItem;
            return true;
        }
        item = default;
        return false;
    }

    #region ICollection Implementations

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public IMessageContent this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public void Add(IMessageContent item)
    {
        // 인덱스 번호를 자동으로 설정합니다.
        item.Index = _items.Count;
        _items.Add(item);
    }

    public void Clear() => _items.Clear();

    public bool Contains(IMessageContent item) => _items.Contains(item);

    public void CopyTo(IMessageContent[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public bool Remove(IMessageContent item) => _items.Remove(item);

    public IEnumerator<IMessageContent> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

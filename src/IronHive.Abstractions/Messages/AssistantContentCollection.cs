using System.Collections;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// IAssistantContent를 담는 컬렉션 구현입니다.
/// </summary>
public class AssistantContentCollection : ICollection<IAssistantContent>
{
    private readonly List<IAssistantContent> _items = new();

    /// <summary>
    /// 아이템을 추가합니다.
    /// 아이템의 인덱스 번호는 자동으로 재할당됩니다.
    /// </summary>
    public void Add(IAssistantContent item)
    {
        item.Index = _items.Count;
        _items.Add(item);
    }

    /// <summary>
    /// 아이템 배열을 추가합니다. 
    /// 인덱스 번호는 자동으로 재할당됩니다.
    /// </summary>
    public void AddRange(IEnumerable<IAssistantContent> collection)
    {
        foreach (var item in collection)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Tool 사용을 기준으로 아이템을 분리합니다.
    /// Tool 컨텐츠들 다음 항목부터 새 그룹으로 분리됩니다.
    /// 예) [ThinkingContent, ToolContent, ToolContent, TextContent, ToolContent, TextContent]
    ///  => [ThinkingContent, ToolContent, ToolContent], [TextContent, ToolContent], [TextContent]
    /// </summary>
    public IEnumerable<AssistantContentCollection> Split()
    {
        if (_items.Count == 0)
            return Enumerable.Empty<AssistantContentCollection>();

        var result = new List<AssistantContentCollection>();
        var currentGroup = new AssistantContentCollection();
        var previousWasTool = false;

        foreach (var item in _items)
        {
            var isTool = item is AssistantToolContent;

            // 이전 항목이 Tool이었고 현재 항목이 Tool이 아닌 경우 새 그룹 시작
            if (previousWasTool && !isTool)
            {
                result.Add(currentGroup);
                currentGroup = new AssistantContentCollection();
            }

            currentGroup.Add(item);
            previousWasTool = isTool;
        }

        // 마지막 그룹 추가
        if (currentGroup.Count > 0)
        {
            result.Add(currentGroup);
        }

        return result;
    }

    #region ICollection Implementations

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public IAssistantContent this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public void Clear() => _items.Clear();

    public bool Contains(IAssistantContent item) => _items.Contains(item);

    public void CopyTo(IAssistantContent[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public bool Remove(IAssistantContent item) => _items.Remove(item);

    public IEnumerator<IAssistantContent> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

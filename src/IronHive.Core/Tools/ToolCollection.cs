using IronHive.Abstractions.Tools;

namespace IronHive.Core.Tools;

/// <inheritdoc />
public class ToolCollection : KeyedCollection<string, ITool>, IToolCollection
{
    public ToolCollection() : this(Enumerable.Empty<ITool>())
    { }

    public ToolCollection(IEnumerable<ITool> tools, IEqualityComparer<string>? comparer = null)
        : base(t => t.UniqueName, tools, comparer)
    {
        foreach (var tool in tools)
        {
            Add(tool);
        }
    }

    /// <inheritdoc />
    public bool RequiresApproval(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return TryGet(key, out var tool) && tool.RequiresApproval;
    }

    /// <inheritdoc />
    public IToolCollection WhereBy(IEnumerable<string> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var result = new ToolCollection();

        // 중복 키를 제거하고, 존재하는 항목만 안전하게 복사
        foreach (var key in new HashSet<string>(keys, StringComparer.OrdinalIgnoreCase))
        {
            if (TryGet(key, out var tool))
            {
                // Set: 중복 키 대비 안전
                result.Set(tool);
            }
        }
        return result;
    }
}

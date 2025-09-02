using IronHive.Abstractions.Tools;

namespace IronHive.Core.Tools;

/// <inheritdoc />
public class ToolCollection : KeyedCollection<ITool>, IToolCollection
{
    public ToolCollection(StringComparer? comparer = null) 
        : base(t => t.UniqueName, comparer)
    { }

    public ToolCollection(IEnumerable<ITool> tools, StringComparer? comparer = null) 
        : base(tools, t => t.UniqueName, comparer)
    { }

    /// <inheritdoc />
    public IToolCollection WhereBy(IEnumerable<string> keys)
    {
        var items = new ToolCollection();
        foreach (var key in keys)
        {
            if (TryGet(key, out var tool))
                items.Add(tool);
        }
        return items;
    }

    /// <inheritdoc />
    public bool RequiresApproval(string key)
    {
        if (TryGet(key, out var tool))
            return tool.RequiresApproval;
        return false;
    }
}

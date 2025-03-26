using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace IronHive.Abstractions.Tools;

public class ToolCollection : ICollection<ITool>
{
    private readonly Dictionary<string, ITool> _items = [];

    public ToolCollection()
    { }

    public ToolCollection(IEnumerable<ITool> items)
    {
        _items = items.ToDictionary(t  => t.Name, t => t);
    }

    public bool TryGetValue(string name, [MaybeNullWhen(false)] out ITool tool)
    {
        return _items.TryGetValue(name, out tool);
    }

    public void AddRange(IEnumerable<ITool> tools)
    {
        foreach (var tool in tools)
        {
            Add(tool);
        }
    }

    #region ICollection Implementations

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public ITool this[int index]
    {
        get => _items.Values.ElementAt(index);
        set => _items[_items.Keys.ElementAt(index)] = value;
    }

    public ITool this[string name]
    {
        get => _items[name];
        set => _items[name] = value;
    }

    public void Add(ITool tool) => _items.Add(tool.Name, tool);

    public void Clear() => _items.Clear();

    public bool Contains(ITool tool) => _items.ContainsKey(tool.Name);

    public void CopyTo(ITool[] array, int arrayIndex) => _items.Values.CopyTo(array, arrayIndex);

    public bool Remove(ITool tool) => _items.Remove(tool.Name);

    public IEnumerator<ITool> GetEnumerator() => _items.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

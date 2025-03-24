using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace IronHive.Abstractions.ChatCompletion.Tools;

public class FunctionToolCollection : ICollection<FunctionTool>
{
    private readonly Dictionary<string, FunctionTool> _items = [];

    public FunctionToolCollection()
    { }

    public FunctionToolCollection(IEnumerable<FunctionTool> items)
    {
        _items = items.ToDictionary(t  => t.Name, t => t);
    }

    public bool TryGetValue(string name, [MaybeNullWhen(false)] out FunctionTool tool)
    {
        return _items.TryGetValue(name, out tool);
    }

    public void AddRange(IEnumerable<FunctionTool> tools)
    {
        foreach (var tool in tools)
        {
            Add(tool);
        }
    }

    #region ICollection Implementations

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public FunctionTool this[int index]
    {
        get => _items.Values.ElementAt(index);
        set => _items[_items.Keys.ElementAt(index)] = value;
    }

    public FunctionTool this[string name]
    {
        get => _items[name];
        set => _items[name] = value;
    }

    public void Add(FunctionTool tool) => _items.Add(tool.Name, tool);

    public void Clear() => _items.Clear();

    public bool Contains(FunctionTool tool) => _items.ContainsKey(tool.Name);

    public void CopyTo(FunctionTool[] array, int arrayIndex) => _items.Values.CopyTo(array, arrayIndex);

    public bool Remove(FunctionTool tool) => _items.Remove(tool.Name);

    public IEnumerator<FunctionTool> GetEnumerator() => _items.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

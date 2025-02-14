using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Raggle.Abstractions.Tools;

public class ToolCollection : ICollection<ITool>
{
    private readonly Dictionary<string, ITool> _items = [];

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public ITool this[int index] => _items.Values.ElementAt(index);

    public ITool this[string name] => _items[name];

    public bool TryGetValue(string name, [NotNullWhen(true)] out ITool tool)
    {
        return _items.TryGetValue(name, out tool!);
    }

    public void AddRange(IEnumerable<ITool> tools)
    {
        foreach (var tool in tools)
        {
            Add(tool);
        }
    }

    #region Implementation Methods

    public void Add(ITool tool) => _items.Add(tool.Name, tool);

    public void Clear() => _items.Clear();

    public bool Contains(ITool tool) => _items.ContainsKey(tool.Name);

    public void CopyTo(ITool[] array, int arrayIndex) => _items.Values.CopyTo(array, arrayIndex);

    public bool Remove(ITool tool) => _items.Remove(tool.Name);

    public IEnumerator<ITool> GetEnumerator() => _items.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Raggle.Abstractions.Tools;

public class FunctionToolCollection : ICollection<FunctionTool>
{
    private readonly Dictionary<string, FunctionTool> _items = [];

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public FunctionTool this[int index] => _items.Values.ElementAt(index);

    public FunctionTool this[string name] => _items[name];

    public bool TryGetValue(string name, [NotNullWhen(true)] out FunctionTool tool) => _items.TryGetValue(name, out tool!);

    public void AddRange(IEnumerable<FunctionTool> tools)
    {
        foreach (var tool in tools)
        {
            Add(tool);
        }
    }

    #region Implementation Methods

    public void Add(FunctionTool tool) => _items.Add(tool.Name, tool);

    public void Clear() => _items.Clear();

    public bool Contains(FunctionTool tool) => _items.ContainsKey(tool.Name);

    public void CopyTo(FunctionTool[] array, int arrayIndex) => _items.Values.CopyTo(array, arrayIndex);

    public bool Remove(FunctionTool tool) => _items.Remove(tool.Name);

    public IEnumerator<FunctionTool> GetEnumerator() => _items.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

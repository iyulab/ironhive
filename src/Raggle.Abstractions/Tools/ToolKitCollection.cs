using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Raggle.Abstractions.Tools;

public class ToolKitCollection : IDictionary<string, ToolKit>
{

    public ToolKit this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public ICollection<string> Keys => throw new NotImplementedException();

    public ICollection<ToolKit> Values => throw new NotImplementedException();

    public int Count => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public void Add(string key, ToolKit value)
    {
        throw new NotImplementedException();
    }

    public void Add(KeyValuePair<string, ToolKit> item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(KeyValuePair<string, ToolKit> item)
    {
        throw new NotImplementedException();
    }

    public bool ContainsKey(string key)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<string, ToolKit>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<string, ToolKit>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public bool Remove(string key)
    {
        throw new NotImplementedException();
    }

    public bool Remove(KeyValuePair<string, ToolKit> item)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out ToolKit value)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

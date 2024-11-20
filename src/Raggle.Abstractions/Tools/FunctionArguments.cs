using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Raggle.Abstractions.Tools;

public class FunctionArguments : IDictionary<string, object>
{
    private readonly Dictionary<string, object> _inner;

    public FunctionArguments()
    {
        _inner = new Dictionary<string, object>();
    }

    public FunctionArguments(IDictionary<string, object> dictionary)
    {
        _inner = new Dictionary<string, object>(dictionary);
    }

    #region Implementations

    public object this[string key]
    {
        get => _inner[key];
        set => _inner[key] = value;
    }

    public ICollection<string> Keys => _inner.Keys;

    public ICollection<object> Values => _inner.Values;

    public int Count => _inner.Count;

    public bool IsReadOnly => false;

    public void Add(string key, object value) => _inner.Add(key, value);

    public void Add(KeyValuePair<string, object> item) => _inner.Add(item.Key, item.Value);

    public bool Remove(string key) => _inner.Remove(key);

    public bool Remove(KeyValuePair<string, object> item) => ((IDictionary<string, object>)_inner).Remove(item);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => _inner.TryGetValue(key, out value);

    public void Clear() => _inner.Clear();

    public bool Contains(KeyValuePair<string, object> item) => _inner.Contains(item);

    public bool ContainsKey(string key) => _inner.ContainsKey(key);

    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => ((IDictionary<string, object>)_inner).CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _inner.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

    #endregion
}

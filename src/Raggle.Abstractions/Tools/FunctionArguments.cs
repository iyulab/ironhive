using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Raggle.Abstractions.Tools;

public class FunctionArguments : IDictionary<string, object>
{
    private Dictionary<string, object> _inner = new();
    private string _buffer = string.Empty;

    public FunctionArguments() 
    { }

    public FunctionArguments(IDictionary<string, object> dictionary)
    {
        _inner = new Dictionary<string, object>(dictionary);
    }

    public FunctionArguments(string json)
    {
        _inner = ParseJson(json);
    }

    public void Append(string? partialJson)
    {
        if (string.IsNullOrEmpty(partialJson))
            return;

        _buffer += partialJson;
        if (_buffer.EndsWith('}'))
        {
            _inner = ParseJson(_buffer);
            _buffer = string.Empty;
        }
    }

    public override string ToString()
    {
        try
        {
            return JsonSerializer.Serialize(_inner);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return @"{}";
        }
    }

    private static Dictionary<string, object> ParseJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json) 
                ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return new Dictionary<string, object>();
        }
    }

    #region IDictionary Implementation

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

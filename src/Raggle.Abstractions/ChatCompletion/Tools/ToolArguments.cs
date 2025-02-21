using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Raggle.Abstractions.ChatCompletion.Tools;

public class ToolArguments : IDictionary<string, object>
{
    private Dictionary<string, object> _inner = new();
    private string _buffer = string.Empty;

    public ToolArguments()
    { }

    public ToolArguments(IDictionary<string, object> dictionary)
    {
        _inner = new Dictionary<string, object>(dictionary);
    }

    public ToolArguments(string? json)
    {
        if (TryParseJson(json, out var value))
            _inner = value;
    }

    public void Append(string? partialJson)
    {
        if (string.IsNullOrEmpty(partialJson))
            return;

        _buffer += partialJson;
        if (_buffer.EndsWith('}') && TryParseJson(_buffer, out var value))
        {
            _inner = value;
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

    private static bool TryParseJson(string? json, [NotNullWhen(true)] out Dictionary<string, object> value)
    {
        try
        {
            if (string.IsNullOrEmpty(json))
            {
                value = new Dictionary<string, object>();
                return true;
            }
            value = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            value = null!;
        }
        return value != null;
    }

    #region IDictionary Implementations

    public ICollection<string> Keys => _inner.Keys;

    public ICollection<object> Values => _inner.Values;

    public int Count => _inner.Count;

    public bool IsReadOnly => false;

    public object this[string key]
    {
        get => _inner[key];
        set => _inner[key] = value;
    }

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

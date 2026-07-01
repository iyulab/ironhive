using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using IronHive.Abstractions.Tools;
using IronHive.Core.Utilities;

namespace IronHive.Core.Tools;

/// <inheritdoc />
public class ToolCollection : IToolCollection
{
    private readonly ConcurrentDictionary<string, ITool> _items;
    private readonly IEqualityComparer<string> _comparer;

    public ToolCollection(StringComparer? comparer = null)
        : this([], comparer)
    { }

    public ToolCollection(IEnumerable<ITool> tools, StringComparer? comparer = null)
    {
        _comparer = comparer ?? StringComparer.OrdinalIgnoreCase;
        _items = new ConcurrentDictionary<string, ITool>(_comparer);
        AddRange(tools);
    }

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Keys => _items.Keys.ToArray();

    /// <inheritdoc />
    public bool TryGet(string key, [MaybeNullWhen(false)] out ITool item)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _items.TryGetValue(key, out item!);
    }

    /// <inheritdoc />
    public bool Contains(ITool item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _items.ContainsKey(item.UniqueName);
    }

    /// <inheritdoc />
    public bool ContainsKey(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _items.ContainsKey(key);
    }

    /// <inheritdoc />
    public void Add(ITool item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!_items.TryAdd(item.UniqueName, item))
            throw new ArgumentException($"An item with the same key already exists. Key: '{item.UniqueName}'.", nameof(item));
    }

    /// <inheritdoc />
    public void AddRange(IEnumerable<ITool> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        foreach (var item in items)
            Add(item);
    }

    /// <inheritdoc />
#pragma warning disable CA1716
    public void Set(ITool item)
#pragma warning restore CA1716
    {
        ArgumentNullException.ThrowIfNull(item);
        _items[item.UniqueName] = item;
    }

    /// <inheritdoc />
    public void SetRange(IEnumerable<ITool> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        foreach (var item in items)
            Set(item);
    }

    /// <inheritdoc />
    public bool Remove(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (_items.TryRemove(key, out var item))
        {
            DisposalHelper.DisposeSafely(item);
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public bool Remove(ITool item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (((ICollection<KeyValuePair<string, ITool>>)_items)
            .Remove(new KeyValuePair<string, ITool>(item.UniqueName, item)))
        {
            DisposalHelper.DisposeSafely(item);
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public int RemoveAll(Predicate<ITool>? match = null)
    {
        match ??= (_ => true);
        var removed = 0;
        foreach (var kvp in _items.ToArray())
        {
            if (match(kvp.Value) && ((ICollection<KeyValuePair<string, ITool>>)_items).Remove(kvp))
            {
                DisposalHelper.DisposeSafely(kvp.Value);
                removed++;
            }
        }
        return removed;
    }

    /// <inheritdoc />
    public void Clear() => _items.Clear();

    /// <inheritdoc />
    public IEnumerator<ITool> GetEnumerator() => _items.Values.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public void CopyTo(ITool[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        if (arrayIndex > array.Length)
            throw new ArgumentException("arrayIndex is out of range.", nameof(arrayIndex));

        var snapshot = _items.Values.ToArray();
        if (array.Length - arrayIndex < snapshot.Length)
            throw new ArgumentException("The destination array has insufficient space.", nameof(array));

        snapshot.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public IToolCollection FilterBy(IEnumerable<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);
        var result = new ToolCollection();
        foreach (var name in new HashSet<string>(names, StringComparer.OrdinalIgnoreCase))
        {
            if (TryGet(name, out var tool))
                result.Set(tool);
        }
        return result;
    }
}

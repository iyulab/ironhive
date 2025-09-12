using IronHive.Abstractions;
using IronHive.Core.Utilities;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace IronHive.Core;

/// <inheritdoc />
public class KeyedCollection<TKey, TItem> : IKeyedCollection<TKey, TItem>
    where TKey : notnull
{
    private readonly Func<TItem, TKey> _keySelector;
    private readonly ConcurrentDictionary<TKey, TItem> _items;
    private readonly IEqualityComparer<TKey> _comparer;

    public KeyedCollection(
        Func<TItem, TKey> keySelector,
        IEnumerable<TItem>? items = null,
        IEqualityComparer<TKey>? comparer = null)
    {
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _comparer = comparer ?? EqualityComparer<TKey>.Default;
        _items = new ConcurrentDictionary<TKey, TItem>(_comparer);

        if (items != null)
        {
            AddRange(items);
        }
    }

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <inheritdoc />
    public IReadOnlyCollection<TKey> Keys => _items.Keys.ToArray();

    /// <inheritdoc />
    public virtual bool TryGet(TKey key, [MaybeNullWhen(false)] out TItem item)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _items.TryGetValue(key, out item!);
    }

    /// <inheritdoc />
    public virtual bool Contains(TItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var key = _keySelector(item);
        return _items.ContainsKey(key);
    }

    /// <inheritdoc />
    public virtual bool ContainsKey(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _items.ContainsKey(key);
    }

    /// <inheritdoc />
    public virtual void Add(TItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var key = _keySelector(item);
        if (!_items.TryAdd(key, item))
            throw new ArgumentException($"An item with the same key already exists. Key: '{key}'.");
    }

    /// <inheritdoc />
    public void AddRange(IEnumerable<TItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        foreach (var item in items)
            Add(item);
    }

    /// <inheritdoc />
    public virtual void Set(TItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var key = _keySelector(item);
        _items[key] = item;
    }

    /// <inheritdoc />
    public void SetRange(IEnumerable<TItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        foreach (var item in items)
            Set(item);
    }

    /// <inheritdoc />
    public virtual bool Remove(TKey key)
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
    public virtual bool Remove(TItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var key = _keySelector(item);

        // 정확한 (key, value) 쌍 제거 시도
        if (((ICollection<KeyValuePair<TKey, TItem>>)_items)
            .Remove(new KeyValuePair<TKey, TItem>(key, item)))
        {
            DisposalHelper.DisposeSafely(item);
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public virtual int RemoveAll(Predicate<TItem>? match = null)
    {
        match ??= (_ => true);

        var removed = 0;
        foreach (var kvp in _items.ToArray()) // 스냅샷 후 안전 순회
        {
            if (match(kvp.Value) && ((ICollection<KeyValuePair<TKey, TItem>>)_items).Remove(kvp))
            {
                DisposalHelper.DisposeSafely(kvp.Value);
                removed++;
            }
        }
        return removed;
    }

    /// <inheritdoc />
    public virtual void Clear() => _items.Clear();

    /// <inheritdoc />
    public virtual IEnumerator<TItem> GetEnumerator() => _items.Values.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public virtual void CopyTo(TItem[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        if (arrayIndex > array.Length) throw new ArgumentException("arrayIndex is out of range.");

        var snapshot = _items.Values.ToArray();
        if (array.Length - arrayIndex < snapshot.Length)
            throw new ArgumentException("The destination array has insufficient space.");

        snapshot.CopyTo(array, arrayIndex);
    }
}

using IronHive.Abstractions;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace IronHive.Core;

/// <summary>
/// 문자열 키로 아이템을 관리하는 스레드-세이프 컬렉션 구현입니다.
/// </summary>
public class KeyedCollection<T> : IKeyedCollection<T> where T : class
{
    private readonly Func<T, string> _keySelector;
    private readonly ConcurrentDictionary<string, T> _items;

    /// <summary>
    /// 항목에서 키를 추출하는 델리게이트와 키 비교자를 지정합니다.
    /// 비교자를 지정하지 않으면 <see cref="StringComparer.OrdinalIgnoreCase"/>가 사용됩니다.
    /// </summary>
    public KeyedCollection(Func<T, string> keySelector, StringComparer? comparer = null)
    {
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _items = new ConcurrentDictionary<string, T>(comparer ?? StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 아이템 컬렉션과 키 선택자, 그리고 선택적으로 키 비교자를 지정합니다.
    /// 비교자를 지정하지 않으면 <see cref="StringComparer.OrdinalIgnoreCase"/>가 사용됩니다.
    /// </summary>
    public KeyedCollection(IEnumerable<T> items, Func<T, string> keySelector, StringComparer? comparer = null)
    {
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _items = new ConcurrentDictionary<string, T>(comparer ?? StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            var key = NormalizeKey(_keySelector(item));
            if (!_items.TryAdd(key, item))
                throw new ArgumentException($"중복된 키가 존재합니다: '{key}'", nameof(items));
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> Keys => _items.Keys.ToArray();

    /// <inheritdoc />
    public IReadOnlyCollection<T> Values => _items.Values.ToArray();

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, T> GetAll()
    {
        // 라이브 뷰(스냅샷 아님). 필요 시 호출부에서 ToDictionary 사용.
        return new ReadOnlyDictionary<string, T>(_items);
    }

    /// <inheritdoc />
    public T? Get(string key)
    {
        return TryGet(key, out var item) ? item : null;
    }

    /// <inheritdoc />
    public bool TryGet(string key, out T item)
    {
        key = NormalizeKey(key);
        return _items.TryGetValue(key, out item!);
    }

    /// <inheritdoc />
    public bool ContainsKey(string key)
    {
        key = NormalizeKey(key);
        return _items.ContainsKey(key);
    }

    /// <inheritdoc />
    public bool Remove(string key)
    {
        key = NormalizeKey(key);
        return _items.TryRemove(key, out _);
    }

    /// <inheritdoc />
    public void Set(T item)
    {
        var key = NormalizeKey(_keySelector(item));
        _items[key] = item; // 추가 또는 교체
    }

    /// <inheritdoc />
    public void Add(T item)
    {
        var key = NormalizeKey(_keySelector(item));
        if (!_items.TryAdd(key, item))
            throw new ArgumentException($"이미 존재하는 키입니다: '{key}'", nameof(item));
    }

    /// <inheritdoc />
    public bool Contains(T item)
    {
        var key = NormalizeKey(_keySelector(item));
        return _items.TryGetValue(key, out var existing)
            && EqualityComparer<T>.Default.Equals(existing, item);
    }

    /// <inheritdoc />
    public bool Remove(T item)
    {
        var key = NormalizeKey(_keySelector(item));
        // 키와 값이 모두 일치할 때만 제거 (경쟁 조건 방지)
        return ((ICollection<KeyValuePair<string, T>>)_items)
            .Remove(new KeyValuePair<string, T>(key, item));
    }

    /// <inheritdoc />
    public void Clear() => _items.Clear();

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex)
    {
        // 스냅샷 후 복사
        var snapshot = _items.Values.ToArray();
        snapshot.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => _items.Values.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// 키를 정규화합니다.
    /// </summary>
    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("키는 null이거나 공백일 수 없습니다.", nameof(key));
        return key.Trim();
    }
}

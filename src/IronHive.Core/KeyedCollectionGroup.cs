using IronHive.Abstractions;
using System.Collections;
using System.Collections.Concurrent;

namespace IronHive.Core;

/// <summary>
/// <see cref="IKeyedCollectionGroup{TBase}"/>의 기본 구현체입니다.
/// 내부적으로 각 파생 타입별 <see cref="KeyedCollection{T}"/>을 유지합니다.
/// </summary>
public sealed class KeyedCollectionGroup<TBase> : IKeyedCollectionGroup<TBase>
    where TBase : class
{
    private readonly ConcurrentDictionary<Type, object> _groups = new();
    private readonly Func<TBase, string> _keySelector;
    private readonly StringComparer _comparer;

    public KeyedCollectionGroup(Func<TBase, string> keySelector, StringComparer? comparer = null)
    {
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _comparer = comparer ?? StringComparer.OrdinalIgnoreCase;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<Type> Types => 
        _groups.Keys.ToArray();

    /// <inheritdoc />
    public IKeyedCollection<TDerived> Of<TDerived>() where TDerived : class, TBase
    {
        var type = typeof(TDerived);
        if (_groups.TryGetValue(type, out var obj) && obj is KeyedCollection<TDerived> existing)
            return existing;

        var created = new KeyedCollection<TDerived>(_keySelector, _comparer);
        _groups[type] = created;
        return created;
    }

    /// <inheritdoc />
    public int Count<TDerived>() where TDerived : class, TBase =>
        Of<TDerived>().Count;

    /// <inheritdoc />
    public bool Any<TDerived>() where TDerived : class, TBase => 
        Of<TDerived>().Any();

    /// <inheritdoc />
    public IReadOnlyCollection<string> GetKeys<TDerived>() where TDerived : class, TBase =>
        Of<TDerived>().Keys;

    /// <inheritdoc />
    public IReadOnlyCollection<TDerived> GetValues<TDerived>() where TDerived : class, TBase =>
        Of<TDerived>().Values;

    /// <inheritdoc />
    public TDerived? Get<TDerived>(string key) where TDerived : class, TBase =>
        Of<TDerived>().Get(key);

    /// <inheritdoc />
    public bool TryGet<TDerived>(string key, out TDerived item) where TDerived : class, TBase =>
        Of<TDerived>().TryGet(key, out item!);

    /// <inheritdoc />
    public bool Contains<TDerived>(TDerived item) where TDerived : class, TBase =>
        Of<TDerived>().Contains(item);

    /// <inheritdoc />
    public bool ContainsKey<TDerived>(string key) where TDerived : class, TBase =>
        Of<TDerived>().ContainsKey(key);

    /// <inheritdoc />
    public void Add<TDerived>(TDerived item) where TDerived : class, TBase =>
        Of<TDerived>().Add(item);

    /// <inheritdoc />
    public void Set<TDerived>(TDerived item) where TDerived : class, TBase =>
        Of<TDerived>().Set(item);

    /// <inheritdoc />
    public bool Remove<TDerived>(string key) where TDerived : class, TBase =>
        Of<TDerived>().Remove(key);

    /// <inheritdoc />
    public int RemoveAll<TDerived>(Predicate<TDerived> match) where TDerived : class, TBase =>
        Of<TDerived>().RemoveAll(match);

    /// <inheritdoc />
    public void Clear<TDerived>() where TDerived : class, TBase =>
        Of<TDerived>().Clear();

    /// <inheritdoc />
    public IEnumerator<TBase> GetEnumerator()
    {
        // 스냅샷 후 순회(안전)
        var snapshot = _groups.Values.ToArray();
        foreach (var g in snapshot)
        {
            // g : KeyedCollection<TDerived> (TDerived는 매번 다름)
            // Values 프로퍼티를 reflection으로 꺼내 IEnumerable로 순회
            var valuesProp = g.GetType().GetProperty("Values");
            if (valuesProp?.GetValue(g) is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is TBase asBase)
                        yield return asBase;
                }
            }
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => 
        GetEnumerator();
}

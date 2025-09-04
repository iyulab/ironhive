using IronHive.Abstractions;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace IronHive.Core;

/// <inheritdoc />
public class KeyedServiceRegistry<TKey, TBase> : IKeyedServiceRegistry<TKey, TBase>
    where TKey : notnull
    where TBase : class
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<TKey, TBase>> _buckets = new();
    private readonly IEqualityComparer<TKey> _comparer;

    /// <summary>
    /// 기본 생성자입니다.
    /// </summary>
    public KeyedServiceRegistry() : this(EqualityComparer<TKey>.Default) 
    { }

    /// <summary>
    /// 저장소를 생성합니다.
    /// </summary>
    /// <param name="comparer">키 비교자. 지정하지 않으면 기본 비교자를 사용합니다.</param>
    public KeyedServiceRegistry(IEqualityComparer<TKey> comparer)
    {
        _comparer = comparer;
    }

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<TKey, TDerived>> Entries<TDerived>()
        where TDerived : class, TBase
    {
        if (!TryGetBucket<TDerived>(out var bucket))
            yield break;

        foreach (var kv in bucket)
        {
            if (kv.Value is not TDerived item)
                continue; // 타입 불일치

            yield return new KeyValuePair<TKey, TDerived>(kv.Key, item);
        }
    }

    /// <inheritdoc />
    public int Count<TDerived>()
        where TDerived : class, TBase
        => TryGetBucket<TDerived>(out var bucket) ? bucket.Count : 0;

    /// <inheritdoc />
    public bool Contains<TDerived>(TKey key)
        where TDerived : class, TBase
        => TryGetBucket<TDerived>(out var bucket) && bucket.ContainsKey(key);

    /// <inheritdoc />
    public bool TryGet<TDerived>(TKey key, [MaybeNullWhen(false)] out TDerived item)
        where TDerived : class, TBase
    {
        if (TryGetBucket<TDerived>(out var bucket) &&
            bucket.TryGetValue(key, out var obj) &&
            obj is TDerived value)
        {
            item = value; // 타입 검사
            return true;
        }

        item = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryAdd<TDerived>(TKey key, TBase item)
        where TDerived : class, TBase
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item is not TDerived)
            return false; // 타입 불일치

        var bucket = GetOrCreateBucket<TDerived>();
        return bucket.TryAdd(key, item);
    }

    /// <inheritdoc />
    public void Set<TDerived>(TKey key, TBase item)
        where TDerived : class, TBase
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item is not TDerived)
            throw new ArgumentException($"Item must be of type {typeof(TDerived).FullName}.", nameof(item));

        var bucket = GetOrCreateBucket<TDerived>();
        bucket[key] = item;
    }

    /// <inheritdoc />
    public bool Remove<TDerived>(TKey key) 
        where TDerived : class, TBase
    {
        if (!TryGetBucket<TDerived>(out var bucket))
            return false;

        if (!bucket.TryRemove(key, out var item))
            return false;

        DisposeSafely(item);
        return true;
    }

    /// <inheritdoc />
    public int RemoveAll(TKey key)
    {
        int removed = 0;
        foreach (var bucket in _buckets.Values)
        {
            if (bucket.TryRemove(key, out var item))
            {
                DisposeSafely(item);
                removed++;
            }
        }
        return removed;
    }

    /// <summary>
    /// 지정된 TDerived 타입의 버킷이 존재하는 경우 가져옵니다.
    /// 존재하지 않으면 false를 반환하고, 새로운 버킷은 만들지 않습니다.
    /// </summary>
    private bool TryGetBucket<TDerived>([NotNullWhen(true)] out ConcurrentDictionary<TKey, TBase>? bucket)
        where TDerived : class, TBase
        => _buckets.TryGetValue(typeof(TDerived), out bucket);

    /// <summary>
    /// 지정된 TDerived 타입의 버킷을 가져오거나, 없으면 새로 생성합니다.
    /// </summary>
    private ConcurrentDictionary<TKey, TBase> GetOrCreateBucket<TDerived>()
        where TDerived : class, TBase
        => _buckets.GetOrAdd(typeof(TDerived), _ => new ConcurrentDictionary<TKey, TBase>(_comparer));

    /// <summary>
    /// 아이템이 IDisposable 또는 IAsyncDisposable을 구현하는 경우 Dispose를 호출합니다.
    /// - IAsyncDisposable이 있으면 우선적으로 DisposeAsync 호출
    /// - 예외는 모두 무시 (silently)
    /// </summary>
    private static void DisposeSafely(TBase item)
    {
        try
        {
            if (item is IAsyncDisposable ad)
            {
                // 비차단: 흘려보내되, 예외는 내부에서 처리
                _ = ad.DisposeAsync().AsTask().ContinueWith(_ => { /* swallow */ });
                return;
            }

            if (item is IDisposable d)
            {
                d.Dispose();
            }
        }
        catch
        {
            // Silently
        }
    }
}

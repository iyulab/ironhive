using IronHive.Abstractions.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace IronHive.Core.Collections;

/// <inheritdoc />
public class KeyedCollection<TKey, TBase> : IKeyedCollection<TKey, TBase>
    where TKey : notnull
    where TBase : class
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<TKey, object>> _buckets = new();
    private readonly IEqualityComparer<TKey> _comparer;

    /// <summary>
    /// 기본 생성자입니다.
    /// </summary>
    public KeyedCollection() 
        : this(null) 
    { }

    /// <summary>
    /// 저장소를 생성합니다.
    /// </summary>
    /// <param name="comparer">키 비교자. 지정하지 않으면 기본 비교자를 사용합니다.</param>
    public KeyedCollection(IEqualityComparer<TKey>? comparer = null)
    {
        _comparer = comparer ?? EqualityComparer<TKey>.Default;
    }

    /// <summary>
    /// 지정된 카테고리 타입의 버킷을 가져오거나, 없으면 생성합니다.
    /// </summary>
    private ConcurrentDictionary<TKey, object> GetOrCreateBucket<TCategory>()
        where TCategory : class, TBase
        => _buckets.GetOrAdd(typeof(TCategory), _ => new ConcurrentDictionary<TKey, object>(_comparer));

    /// <inheritdoc />
    public int Count<TCategory>()
        where TCategory : class, TBase
    {
        var bucket = GetOrCreateBucket<TCategory>();
        return bucket.Count;
    }

    /// <inheritdoc />
    public bool Contains<TCategory>(TKey key)
        where TCategory : class, TBase
    {
        var bucket = GetOrCreateBucket<TCategory>();
        return bucket.ContainsKey(key);
    }

    /// <inheritdoc />
    public bool TryGet<TCategory>(TKey key, [MaybeNullWhen(false)] out TCategory item)
        where TCategory : class, TBase
    {
        var bucket = GetOrCreateBucket<TCategory>();
        if (bucket.TryGetValue(key, out var obj) && obj is TCategory category)
        {
            item = category;
            return true;
        }

        item = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryAdd<TCategory, TItem>(TKey key, TItem item)
        where TCategory : class, TBase
        where TItem : class, TCategory
    {
        ArgumentNullException.ThrowIfNull(item);
        var bucket = GetOrCreateBucket<TCategory>();
        return bucket.TryAdd(key, item);
    }

    /// <inheritdoc />
    public void Set<TCategory, TItem>(TKey key, TItem item)
        where TCategory : class, TBase
        where TItem : class, TCategory
    {
        ArgumentNullException.ThrowIfNull(item);
        var bucket = GetOrCreateBucket<TCategory>();
        bucket[key] = item;
    }

    /// <inheritdoc />
    public bool Remove<TCategory>(TKey key) 
        where TCategory : class, TBase
    {
        var bucket = GetOrCreateBucket<TCategory>();
        return bucket.TryRemove(key, out _);
    }

    /// <inheritdoc />
    public int RemoveAll<TCategory>(Predicate<TCategory>? match = null) 
        where TCategory : class, TBase
    {
        match ??= (_ => true);
        var bucket = GetOrCreateBucket<TCategory>();
        if (bucket.IsEmpty) return 0;

        var targets = new List<TKey>();
        foreach (var (k, v) in bucket)
            if (v is TCategory c && match(c)) targets.Add(k);

        var removed = 0;
        foreach (var key in targets)
            if (bucket.TryRemove(key, out _)) removed++;

        return removed;
    }
}

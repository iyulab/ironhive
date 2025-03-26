using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using IronHive.Abstractions;

namespace IronHive.Core;

public class HiveServiceStore : IHiveServiceStore
{
    // ConcurrentDictionary를 이용해 thread-safe하게 관리합니다.
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> _registry = new();

    /// <inheritdoc />
    public IReadOnlyDictionary<string, TService> GetServices<TService>()
    {
        if (_registry.TryGetValue(typeof(TService), out var serviceDict))
        {
            // ConcurrentDictionary의 내용을 새 Dictionary로 변환하여 반환합니다.
            return serviceDict.ToDictionary(kvp => kvp.Key, kvp => (TService)kvp.Value);
        }
        return new Dictionary<string, TService>();
    }

    /// <inheritdoc />
    public bool TryGetService<TService>(string key, [MaybeNullWhen(false)] out TService instance)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("서비스 키는 null이거나 공백일 수 없습니다.", nameof(key));

        instance = default;
        if (_registry.TryGetValue(typeof(TService), out var serviceDict))
        {
            if (serviceDict.TryGetValue(key, out var value) && value is TService service)
            {
                instance = service;
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc />
    public TService GetService<TService>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("서비스 키는 null이거나 공백일 수 없습니다.", nameof(key));

        if (TryGetService<TService>(key, out var service))
        {
            return service;
        }
        throw new KeyNotFoundException($"타입 {typeof(TService).FullName}의 '{key}' 서비스가 존재하지 않습니다.");
    }

    /// <inheritdoc />
    public bool TryAddService<TService>(string key, TService instance)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("서비스 키는 null이거나 공백일 수 없습니다.", nameof(key));

        // TService 타입에 대한 레지스트리가 없으면 생성하고, 있으면 가져옵니다.
        var serviceDict = _registry.GetOrAdd(typeof(TService), _ => new ConcurrentDictionary<string, object>());
        return serviceDict.TryAdd(key, instance);
    }

    /// <inheritdoc />
    public void AddService<TService>(string key, TService instance)
    {
        if (!TryAddService<TService>(key, instance))
        {
            throw new ArgumentException($"타입 {typeof(TService).FullName}의 '{key}' 서비스는 이미 등록되어 있습니다.", nameof(key));
        }
    }

    /// <inheritdoc />
    public bool TryRemoveService<TService>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("서비스 키는 null이거나 공백일 수 없습니다.", nameof(key));

        if (_registry.TryGetValue(typeof(TService), out var serviceDict))
        {
            return serviceDict.TryRemove(key, out _);
        }
        return false;
    }

    /// <inheritdoc />
    public void RemoveService<TService>(string key)
    {
        TryRemoveService<TService>(key);
    }

    /// <inheritdoc />
    public bool ContainsService<TService>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("서비스 키는 null이거나 공백일 수 없습니다.", nameof(key));

        return _registry.TryGetValue(typeof(TService), out var serviceDict) && serviceDict.ContainsKey(key);
    }
}

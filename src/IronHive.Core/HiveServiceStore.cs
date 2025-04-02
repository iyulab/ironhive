using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using IronHive.Abstractions;

namespace IronHive.Core;

/// <inheritdoc />
public class HiveServiceStore : IHiveServiceStore
{
    // 싱글 인스턴스 서비스를 저장하는 ConcurrentDictionary입니다.
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> _instances = new();

    // 서비스 팩토리 함수를 저장하는 ConcurrentDictionary입니다.
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Delegate>> _factories = new();

    #region Service 인스턴스 관련 메서드들

    /// <inheritdoc />
    public IReadOnlyDictionary<string, TService> GetServices<TService>()
    {
        if (_instances.TryGetValue(typeof(TService), out var serviceDict))
        {
            return serviceDict.ToDictionary(kvp => kvp.Key, kvp => (TService)kvp.Value);
        }
        return new Dictionary<string, TService>();
    }

    /// <inheritdoc />
    public bool TryGetService<TService>(string key, [MaybeNullWhen(false)] out TService instance)
    {
        ValidateKey(key);
        instance = default;
        if (_instances.TryGetValue(typeof(TService), out var serviceDict) &&
            serviceDict.TryGetValue(key, out var value) && value is TService service)
        {
            instance = service;
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public TService GetService<TService>(string key)
    {
        ValidateKey(key);
        if (TryGetService<TService>(key, out var service))
            return service;

        throw new KeyNotFoundException($"타입 {typeof(TService).FullName}의 '{key}' 서비스가 존재하지 않습니다.");
    }

    /// <inheritdoc />
    public bool TryAddService<TService>(string key, TService instance)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        ValidateKey(key);
        var serviceDict = GetOrAddDictionary(_instances, typeof(TService));
        return serviceDict.TryAdd(key, instance);
    }

    /// <inheritdoc />
    public void AddService<TService>(string key, TService instance)
    {
        if (!TryAddService(key, instance))
            throw new ArgumentException($"타입 {typeof(TService).FullName}의 '{key}' 서비스는 이미 등록되어 있습니다.", nameof(key));
    }

    /// <inheritdoc />
    public bool TryRemoveService<TService>(string key)
    {
        ValidateKey(key);
        if (_instances.TryGetValue(typeof(TService), out var serviceDict))
            return serviceDict.TryRemove(key, out _);
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
        ValidateKey(key);
        return _instances.TryGetValue(typeof(TService), out var serviceDict) && serviceDict.ContainsKey(key);
    }

    #endregion

    #region Factory 함수 관련 메서드들

    /// <inheritdoc />
    public IReadOnlyDictionary<string, Func<IServiceProvider, object?, TService>> GetFactories<TService>()
    {
        if (_factories.TryGetValue(typeof(TService), out var factoryDict))
        {
            return factoryDict.ToDictionary(kvp => kvp.Key, kvp => (Func<IServiceProvider, object?, TService>)kvp.Value);
        }
        return new Dictionary<string, Func<IServiceProvider, object?, TService>>();
    }

    /// <inheritdoc />
    public bool TryGetFactory<TService>(string key, [MaybeNullWhen(false)] out Func<IServiceProvider, object?, TService> factory)
    {
        ValidateKey(key);
        factory = default;
        if (_factories.TryGetValue(typeof(TService), out var factoryDict) &&
            factoryDict.TryGetValue(key, out var value) && value is Func<IServiceProvider, object?, TService> func)
        {
            factory = func;
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public Func<IServiceProvider, object?, TService> GetFactory<TService>(string key)
    {
        ValidateKey(key);
        if (_factories.TryGetValue(typeof(TService), out var factoryDict) &&
            factoryDict.TryGetValue(key, out var factory))
        {
            return (Func<IServiceProvider, object?, TService>)factory;
        }
        throw new KeyNotFoundException($"타입 {typeof(TService).FullName}의 '{key}' 서비스 팩토리가 존재하지 않습니다.");
    }

    /// <inheritdoc />
    public bool TryAddFactory<TService>(string key, Func<IServiceProvider, object?, TService> factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        ValidateKey(key);
        var factoryDict = GetOrAddDictionary(_factories, typeof(TService));
        return factoryDict.TryAdd(key, factory);
    }

    /// <inheritdoc />
    public void AddFactory<TService>(string key, Func<IServiceProvider, object?, TService> factory)
    {
        if (!TryAddFactory(key, factory))
            throw new ArgumentException($"타입 {typeof(TService).FullName}의 '{key}' 서비스 팩토리는 이미 등록되어 있습니다.", nameof(key));
    }

    /// <inheritdoc />
    public bool TryRemoveFactory<TService>(string key)
    {
        ValidateKey(key);
        if (_factories.TryGetValue(typeof(TService), out var factoryDict))
            return factoryDict.TryRemove(key, out _);
        return false;
    }

    /// <inheritdoc />
    public void RemoveFactory<TService>(string key)
    {
        TryRemoveFactory<TService>(key);
    }

    /// <inheritdoc />
    public bool ContainsFactory<TService>(string key)
    {
        ValidateKey(key);
        return _factories.TryGetValue(typeof(TService), out var factoryDict) && factoryDict.ContainsKey(key);
    }

    #endregion

    // 키 값에 대해 공통된 검증 로직을 수행합니다.
    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("서비스 키는 null이거나 공백일 수 없습니다.", nameof(key));
    }

    // 지정된 타입에 대한 내부 딕셔너리를 가져오거나 새로 생성합니다.
    private static ConcurrentDictionary<string, T> GetOrAddDictionary<T>(
        ConcurrentDictionary<Type, ConcurrentDictionary<string, T>> store, Type serviceType)
    {
        return store.GetOrAdd(serviceType, _ => new ConcurrentDictionary<string, T>());
    }
}

using Raggle.Abstractions;
using System.Collections.Concurrent;

namespace Raggle.Core;

public class HiveServiceRegistry : IHiveServiceRegistry
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> _services = new();

    public IReadOnlyDictionary<string, TService> GetKeyedServices<TService>()
        where TService : class
    {
        var serviceType = typeof(TService);
        if (_services.TryGetValue(serviceType, out var keyedServices))
        {
            return keyedServices.ToDictionary(kv => kv.Key, kv => (TService)kv.Value)
                                .AsReadOnly();
        }
        return new Dictionary<string, TService>().AsReadOnly();
    }

    public TService GetKeyedService<TService>(string serviceKey)
        where TService : class
    {
        if (string.IsNullOrEmpty(serviceKey))
            throw new ArgumentNullException(nameof(serviceKey));

        var serviceType = typeof(TService);
        if (_services.TryGetValue(serviceType, out var keyedServices) &&
            keyedServices.TryGetValue(serviceKey, out var service))
        {
            return (TService)service;
        }
        throw new KeyNotFoundException($"Service of type '{serviceType.Name}' with key '{serviceKey}' not found.");
    }

    public void RegisterKeyedService<TService>(string serviceKey, TService instance)
        where TService : class
    {
        if (string.IsNullOrEmpty(serviceKey))
            throw new ArgumentNullException(nameof(serviceKey));
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        var serviceType = typeof(TService);
        var keyedServices = _services.GetOrAdd(serviceType, _ => new ConcurrentDictionary<string, object>());

        if (!keyedServices.TryAdd(serviceKey, instance))
        {
            throw new InvalidOperationException($"A service with key '{serviceKey}' is already registered for type '{serviceType.Name}'.");
        }
    }

    public void UnregisterKeyedService<TService>(string serviceKey) 
        where TService : class
    {
        if (string.IsNullOrEmpty(serviceKey))
            throw new ArgumentNullException(nameof(serviceKey));

        var serviceType = typeof(TService);
        if (_services.TryGetValue(serviceType, out var keyedServices))
        {
            keyedServices.TryRemove(serviceKey, out _);
            // 비어 있으면 타입 자체를 제거
            if (keyedServices.IsEmpty)
            {
                _services.TryRemove(serviceType, out _);
            }
        }
    }

    public bool ContainsKeyedService<TService>(string serviceKey) 
        where TService : class
    {
        if (string.IsNullOrEmpty(serviceKey))
            throw new ArgumentNullException(nameof(serviceKey));

        var serviceType = typeof(TService);
        return _services.TryGetValue(serviceType, out var keyedServices) &&
               keyedServices.ContainsKey(serviceKey);
    }
}

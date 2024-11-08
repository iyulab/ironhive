using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace Raggle.Core;

public class RaggleServiceProvider : IServiceProvider, IKeyedServiceProvider
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> _services = new();

    public bool TryGetService<TService>(string name, out TService service)
    {
        service = default!;
        if (_services.TryGetValue(typeof(TService), out var serviceDict))
        {
            if (serviceDict.TryGetValue(name, out var obj) && obj is TService typedService)
            {
                service = typedService;
                return true;
            }
        }
        return false;
    }

    public bool TryAddService<TService>(string name, TService service)
    {
        var serviceDict = _services.GetOrAdd(typeof(TService), _ => new ConcurrentDictionary<string, object>());
        return serviceDict.TryAdd(name, service);
    }

    public bool TryRemoveService<TService>(string name, out TService service)
    {
        service = default!;
        if (_services.TryGetValue(typeof(TService), out var serviceDict))
        {
            if (serviceDict.TryRemove(name, out var obj) && obj is TService typedService)
            {
                service = typedService;
                return true;
            }
        }
        return false;
    }

    public object? GetService(Type serviceType)
    {
        if (_services.TryGetValue(serviceType, out var serviceDict))
        {
            return serviceDict.Values.FirstOrDefault();
        }
        return null;
    }

    public object? GetKeyedService(Type serviceType, object? serviceKey)
    {
        throw new NotImplementedException();
    }

    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
    {
        throw new NotImplementedException();
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core;

internal sealed class CompositeServiceProvider : IServiceProvider, IKeyedServiceProvider
{
    private readonly IServiceProvider[] _providers;

    public CompositeServiceProvider(params IServiceProvider[] providers)
    {
        _providers = providers;
    }

    public object? GetService(Type serviceType)
    {
        foreach (var p in _providers)
        {
            var s = p.GetService(serviceType);
            if (s != null) return s;
        }
        return null;
    }

    public object? GetKeyedService(Type serviceType, object? serviceKey)
    {
        foreach (var p in _providers)
        {
            if (p is IKeyedServiceProvider ksp)
            {
                var s = ksp.GetKeyedService(serviceType, serviceKey);
                if (s != null) return s;
            }
        }
        return null;
    }

    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
        => GetKeyedService(serviceType, serviceKey)
           ?? throw new InvalidOperationException(
               $"No keyed service of type '{serviceType.Name}' with key '{serviceKey}' was found.");
}

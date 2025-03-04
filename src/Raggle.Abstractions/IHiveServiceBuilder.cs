namespace Raggle.Abstractions;

public interface IHiveServiceBuilder
{
    /// <summary>
    /// Register a service as a singleton.
    /// </summary>
    IHiveServiceBuilder AddService<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;

    /// <summary>
    /// Register a keyed service to IHiveServiceRegistry.
    /// </summary>
    IHiveServiceBuilder AddKeyedService<TService, TImplementation>(string serviceKey, TImplementation instance)
        where TService : class
        where TImplementation : class, TService;
}

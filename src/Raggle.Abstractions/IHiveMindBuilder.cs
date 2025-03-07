namespace Raggle.Abstractions;

public interface IHiveMindBuilder
{
    /// <summary>
    /// Register a service as a singleton.
    /// </summary>
    IHiveMindBuilder AddService<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;

    /// <summary>
    /// Register a keyed service to IHiveServiceRegistry.
    /// </summary>
    IHiveMindBuilder AddKeyedService<TService, TImplementation>(string serviceKey, TImplementation instance)
        where TService : class
        where TImplementation : class, TService;

    /// <summary>
    /// Build the IHiveMind.
    /// </summary>
    IHiveMind Build();
}

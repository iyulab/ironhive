using IronHive.Abstractions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core;

/// <inheritdoc />
public class HiveService : IHiveService
{
    public HiveService(IServiceProvider services)
    {
        Services = services;

        Providers = services.GetRequiredService<IKeyedCollectionGroup<IKeyedProvider>>();
        Storages = services.GetRequiredService<IKeyedCollectionGroup<IKeyedStorage>>();

        Agents = services.GetRequiredService<IAgentService>();
        Files = services.GetRequiredService<IFileService>();
        Memory = services.GetRequiredService<IMemoryService>();
    }

    /// <inheritdoc />
    public IServiceProvider Services { get; }

    /// <inheritDoc />
    public IKeyedCollectionGroup<IKeyedProvider> Providers { get; }

    /// <inheritDoc />
    public IKeyedCollectionGroup<IKeyedStorage> Storages { get; }


    /// <inheritdoc />
    public IAgentService Agents { get; }    
    
    /// <inheritdoc />
    public IFileService Files { get; }

    /// <inheritdoc />
    public IMemoryService Memory { get; }
}

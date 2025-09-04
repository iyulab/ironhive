using IronHive.Abstractions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core;

/// <inheritdoc />
public class HiveService : IHiveService
{
    public HiveService(IServiceProvider services)
    {
        Services = services;

        Providers = services.GetRequiredService<IProviderRegistry>();
        Storages = services.GetRequiredService<IStorageRegistry>();
        Tools = services.GetRequiredService<IToolCollection>();

        Agent = services.GetRequiredService<IAgentService>();
        Memory = services.GetRequiredService<IMemoryService>();
    }

    /// <inheritdoc />
    public IServiceProvider Services { get; }

    /// <inheritDoc />
    public IProviderRegistry Providers { get; }

    /// <inheritDoc />
    public IStorageRegistry Storages { get; }

    /// <inheritdoc />
    public IToolCollection Tools { get; }


    /// <inheritdoc />
    public IAgentService Agent { get; }

    /// <inheritdoc />
    public IMemoryService Memory { get; }
}

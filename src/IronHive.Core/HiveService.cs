using IronHive.Abstractions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Collections;
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

        Providers = services.GetRequiredService<IProviderCollection>();
        Storages = services.GetRequiredService<IStorageCollection>();
        Tools = services.GetRequiredService<IToolCollection>();

        Agent = services.GetRequiredService<IAgentService>();
        File = services.GetRequiredService<IFileService>();
        Memory = services.GetRequiredService<IMemoryService>();
    }

    /// <inheritdoc />
    public IServiceProvider Services { get; }

    /// <inheritDoc />
    public IProviderCollection Providers { get; }

    /// <inheritDoc />
    public IStorageCollection Storages { get; }

    /// <inheritdoc />
    public IToolCollection Tools { get; }


    /// <inheritdoc />
    public IAgentService Agent { get; }    
    
    /// <inheritdoc />
    public IFileService File { get; }

    /// <inheritdoc />
    public IMemoryService Memory { get; }
}

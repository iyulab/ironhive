using IronHive.Abstractions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions.Workflow;
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
    public IMemoryService Memory { get; }

    /// <inheritdoc />
    public IAgent CreateAgentFrom(AgentCard card)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IAgent CreateAgentFromYaml(string yaml)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IAgent CreateMemoryWorker(string queueName, WorkflowDefinition definition)
    {
        throw new NotImplementedException();
    }
}

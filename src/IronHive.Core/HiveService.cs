using IronHive.Abstractions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions.Workflow;
using IronHive.Core.Memory;
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
    public IMemoryWorkerManager CreateMemoryWorkers(
        string queueStorageName,
        Action<WorkflowStepBuilder<MemoryContext>> action)
    {
        if (!Storages.TryGet<IQueueStorage>(queueStorageName, out var storage))
            throw new InvalidOperationException($"큐 스토리지 '{queueStorageName}'(이)가 등록되어 있지 않습니다.");

        var builder = new WorkflowFactory(Services).CreateBuilder().StartWith<MemoryContext>();
        action(builder);
        var workflow = builder.Build();
        return new MemoryWorkerManager(storage, workflow);
    }
}

using IronHive.Abstractions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions.Workflow;
using IronHive.Core.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

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
}

public static class HiveServiceExtensions
{
    /// <summary>
    /// 메모리 작업자를 생성합니다.
    /// </summary>
    public static IMemoryWorker CreateMemoryWorkerFrom(
        this IHiveService service,
        Func<MemoryWorkerBuilder, MemoryPipelineBuilder> configure)
    {
        var builder = new MemoryWorkerBuilder(service.Services);
        var pipelineBuilder = configure(builder);
        return pipelineBuilder.Build();
    }
}
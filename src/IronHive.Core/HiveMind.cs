using IronHive.Abstractions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Memory;
using IronHive.Core.Services;

namespace IronHive.Core;

public class HiveMind : IHiveMind
{
    public IServiceProvider Services { get; }

    public HiveMind(IServiceProvider services)
    {
        Services = services;
    }

    /// <inheritdoc />
    public IHiveSession CreateHiveSession(IHiveAgent master, IDictionary<string, IHiveAgent>? agents)
    {
        return new HiveSession(Services)
        {
            Master = master,
            Agents = agents ?? new Dictionary<string, IHiveAgent>(),
        };
    }

    /// <inheritdoc />
    public IVectorMemory CreateVectorMemory(VectorMemoryConfig config)
    {
        return new VectorMemory(Services, config);
    }

    /// <inheritdoc />
    public IPipelineWorker CreatePipelineWorker(PipelineWorkerConfig config)
    {
        return new PipelineWorker(Services, config);
    }
}

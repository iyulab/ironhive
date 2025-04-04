using IronHive.Abstractions;
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
    public IHiveMemory CreateHiveMemory(string embedProvider, string embedModel)
    {
        return new HiveMemory(Services)
        {
            EmbedProvider = embedProvider,
            EmbedModel = embedModel
        };
    }

    /// <inheritdoc />
    public IPipelineWorker CreatePipelineWorker(int maxExecutionSlots, TimeSpan pollingInterval)
    {
        return new PipelineWorker(Services)
        {
            MaxExecutionSlots = maxExecutionSlots,
            PollingInterval = pollingInterval
        };
    }
}

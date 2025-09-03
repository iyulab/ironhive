using IronHive.Abstractions.Agent;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core.Agent;

/// <inheritdoc />
public class AgentServiceBuilder : IAgentServiceBuilder
{
    private readonly IServiceCollection _services;

    public AgentServiceBuilder(IServiceCollection? services = null)
    {
        _services = services ?? new ServiceCollection();
    }

    /// <inheritdoc />
    public IAgentService Build()
    {
        var provider = _services.BuildServiceProvider();
        return provider.GetRequiredService<IAgentService>();
    }
}

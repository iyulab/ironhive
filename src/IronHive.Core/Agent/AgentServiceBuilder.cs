using IronHive.Abstractions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;
using IronHive.Core.Services;
using IronHive.Core.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IronHive.Core.Agent;

/// <inheritdoc />
public class AgentServiceBuilder : IAgentServiceBuilder
{
    private readonly IServiceCollection _services;
    private readonly ToolCollection _tools = new();

    public AgentServiceBuilder(IServiceCollection? services = null)
    {
        _services = services ?? new ServiceCollection();

        _services.TryAddSingleton<IAgentService>(sp =>
        {
            var providers = sp.GetRequiredService<IKeyedCollectionGroup<IKeyedProvider>>();
            var generators = providers.Of<IMessageGenerator>();
            return new AgentService(new MessageService(sp, generators, _tools), _tools);
        });
    }

    /// <inheritdoc />
    public IAgentServiceBuilder AddTool(ITool tool)
    {
        _tools.Add(tool);
        return this;
    }

    /// <inheritdoc />
    public IAgentService Build()
    {
        var provider = _services.BuildServiceProvider();
        return provider.GetRequiredService<IAgentService>();
    }
}

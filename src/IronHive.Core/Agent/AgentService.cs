using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Agent;

/// <inheritdoc />
public class AgentService : IAgentService
{
    private readonly IMessageService _messages;

    public AgentService(IMessageService message, IToolCollection tools)
    {
        _messages = message;
        Tools = tools;
    }

    /// <inheritdoc />
    public IToolCollection Tools { get; }

    /// <inheritdoc />
    public IAgent CreateAgentFromJson(string json)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IAgent CreateAgentFromToml(string toml)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IAgent CreateAgentFromYaml(string yaml)
    {
        throw new NotImplementedException();
    }
}

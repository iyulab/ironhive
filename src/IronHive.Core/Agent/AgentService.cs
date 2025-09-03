using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Collections;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Agent;

/// <inheritdoc />
public class AgentService : IAgentService
{
    private readonly IMessageService _messages;

    public AgentService(IMessageService message)
    {
        _messages = message;
    }

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

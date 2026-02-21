using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions.Agent.Orchestration;

/// <summary>
/// Scopes the messages that a sub-agent receives during orchestration.
/// Used to reduce context size and improve agent focus by filtering
/// the full message history before passing it to each agent.
/// </summary>
public interface IContextScope
{
    /// <summary>
    /// Scopes the messages for a specific agent.
    /// </summary>
    /// <param name="messages">The full message list available at this point in orchestration.</param>
    /// <param name="agentName">The name of the agent that will receive the scoped messages.</param>
    /// <returns>A scoped subset of messages for the agent.</returns>
    IReadOnlyList<Message> ScopeMessages(
        IReadOnlyList<Message> messages,
        string agentName);
}

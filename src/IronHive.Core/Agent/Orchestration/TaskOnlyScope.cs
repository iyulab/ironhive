using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// Context scope that isolates each agent to only the most recent user message.
/// Ideal for parallel orchestration where each agent should work independently
/// on the task without seeing the full conversation history.
/// </summary>
public class TaskOnlyScope : IContextScope
{
    /// <inheritdoc />
    public IReadOnlyList<Message> ScopeMessages(
        IReadOnlyList<Message> messages,
        string agentName)
    {
        // Find the last user message (the task)
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            if (messages[i] is UserMessage)
            {
                return [messages[i]];
            }
        }

        // Fallback: return all messages if no user message found
        return messages;
    }
}

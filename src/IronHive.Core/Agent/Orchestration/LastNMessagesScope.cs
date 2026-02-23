using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// Context scope that keeps the first message (task/goal) and the last N messages.
/// Useful for limiting context window usage while preserving the original task
/// and the most recent conversation context.
/// </summary>
public class LastNMessagesScope : IContextScope
{
    private readonly int _maxMessages;

    /// <summary>
    /// Creates a scope that keeps the first message and the last N messages.
    /// </summary>
    /// <param name="maxMessages">Maximum number of recent messages to keep (in addition to the first message). Default: 5.</param>
    public LastNMessagesScope(int maxMessages = 5)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxMessages);
        _maxMessages = maxMessages;
    }

    /// <inheritdoc />
    public IReadOnlyList<Message> ScopeMessages(
        IReadOnlyList<Message> messages,
        string agentName)
    {
        if (messages.Count <= _maxMessages + 1)
        {
            return messages;
        }

        var result = new List<Message>(_maxMessages + 1);

        // Always keep the first message (typically the task/goal)
        result.Add(messages[0]);

        // Keep the last N messages
        var startIndex = messages.Count - _maxMessages;
        for (var i = startIndex; i < messages.Count; i++)
        {
            // Avoid duplicating the first message if it falls within the tail
            if (i > 0)
            {
                result.Add(messages[i]);
            }
        }

        return result;
    }
}

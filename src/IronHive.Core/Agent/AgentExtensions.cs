using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;

namespace IronHive.Core.Agent;

/// <summary>
/// Extension methods for <see cref="IAgent"/>.
/// </summary>
public static class AgentExtensions
{
    /// <summary>
    /// Wraps the agent with a middleware chain.
    /// </summary>
    public static IAgent WithMiddleware(this IAgent agent, params IAgentMiddleware[] middlewares)
    {
        if (middlewares.Length == 0)
            return agent;

        return new MiddlewareAgent(agent, middlewares);
    }

    /// <summary>
    /// Invokes the agent with a plain text string converted to a <see cref="Message"/>.
    /// </summary>
    public static Task<MessageResponse> InvokeAsync(
        this IAgent agent,
        string userText,
        CancellationToken cancellationToken = default)
        => agent.InvokeAsync(
            [new Message { Role = MessageRole.User, Content = [new TextMessageContent { Value = userText }] }],
            cancellationToken);

    /// <summary>
    /// Invokes the agent in streaming mode with a plain text string converted to a <see cref="Message"/>.
    /// </summary>
    public static IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        this IAgent agent,
        string userText,
        CancellationToken cancellationToken = default)
        => agent.InvokeStreamingAsync(
            [new Message { Role = MessageRole.User, Content = [new TextMessageContent { Value = userText }] }],
            cancellationToken);
}

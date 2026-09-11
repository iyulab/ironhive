using System.Diagnostics;
using IronHive.Abstractions.Agent;

namespace IronHive.Core.Agent;

/// <summary>
/// Warns when a middleware that cannot take part in streaming calls is registered.
/// </summary>
/// <remarks>
/// A middleware implementing only <see cref="IAgentMiddleware"/> is filtered out of every streaming
/// call — the protection it was configured for is simply absent there, with no error and nothing in the
/// result to notice. Every built-in middleware implements both halves, so this concerns a caller's own;
/// the warning names it at registration, where the caller can still act on it, rather than leaving the
/// gap to be discovered in production.
/// </remarks>
internal static class MiddlewareStreamingDiagnostics
{
    /// <summary>
    /// Traces one warning per middleware that has no streaming half.
    /// </summary>
    /// <param name="middlewares">The middlewares being registered; null is ignored.</param>
    /// <param name="target">What they were registered on, named in the warning.</param>
    public static void WarnAboutBufferedOnly(IEnumerable<IAgentMiddleware>? middlewares, string target)
    {
        if (middlewares is null)
            return;

        foreach (var middleware in middlewares)
        {
            if (middleware is null or IStreamingAgentMiddleware)
                continue;

            Trace.TraceWarning(
                $"[IronHive.Agent] Middleware '{middleware.GetType().Name}' registered on {target} implements " +
                "IAgentMiddleware only, so it is skipped on streaming calls (InvokeStreamingAsync, " +
                "ExecuteStreamingAsync) and whatever it does will not apply there. Implement " +
                "IStreamingAgentMiddleware to take part in them.");
        }
    }
}

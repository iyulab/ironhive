using AwesomeAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Core.Agent;

namespace IronHive.Tests.Conventions;

// Teeth for a known limitation of the agent middleware pair (ironhive-umbrella docs/CONVENTIONS.md
// section 5).
//
// A middleware that implements IAgentMiddleware but not IStreamingAgentMiddleware is skipped on every
// streaming call: MiddlewareAgent.InvokeStreamingAsync and the orchestrators' streaming path both
// filter the configured list down to IStreamingAgentMiddleware. Nothing says so at run time -- a
// timeout, rate limit or circuit breaker configured for an agent applies to InvokeAsync and does
// nothing for InvokeStreamingAsync.
//
// Seven built-in middlewares were in that state until 0.26.0 (Timeout, Retry, RateLimit, CircuitBreaker,
// Bulkhead, Fallback, Caching); every one now implements both halves. The list below is empty and stays
// here as teeth: a new built-in middleware without a streaming half fails this test, so leaving a
// streaming half out has to be a deliberate, documented decision. See docs/MIDDLEWARE.md, "스트리밍 지원".
public class AgentMiddlewareStreamingRosterTests
{
    private static readonly string[] BufferedOnly = [];

    [Fact]
    public void BuiltInMiddlewareWithoutAStreamingHalf_MatchesTheKnownList()
    {
        Implementing(streaming: false).Should().Equal(
            BufferedOnly,
            "a middleware without IStreamingAgentMiddleware is silently skipped on every streaming call. "
            + "Implement the streaming half, or change this list as a deliberate decision and keep "
            + "docs/MIDDLEWARE.md in step with it.");
    }

    // Positive control: the detector must also see the types that do implement both halves, or an
    // empty result above would pass for the wrong reason.
    [Fact]
    public void BuiltInMiddlewareWithBothHalves_IsSeenByTheSameDetector()
    {
        Implementing(streaming: true).Should().Contain(["CompositeMiddleware", "LoggingMiddleware"]);
    }

    private static List<string> Implementing(bool streaming)
        => [.. typeof(MiddlewareAgent).Assembly.GetTypes()
            .Where(t => t.IsPublic && t.IsClass && !t.IsAbstract)
            .Where(t => typeof(IAgentMiddleware).IsAssignableFrom(t))
            .Where(t => typeof(IStreamingAgentMiddleware).IsAssignableFrom(t) == streaming)
            .Select(t => t.Name)
            .OrderBy(n => n, StringComparer.Ordinal)];
}

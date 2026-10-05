using Iyu.Conventions.Testing;
using Xunit;

namespace IronHive.Tests;

/// <summary>
/// The public surface follows the two API rules of the ecosystem: every public async method takes a
/// <see cref="CancellationToken"/>, and failure is reported by an exception rather than by a returned object carrying a
/// success flag and an error. The scans are <c>Iyu.Conventions.Testing</c>'s, over the same assemblies as the
/// operational-language scan.
/// </summary>
/// <remarks>
/// The rosters are the methods that break a rule today. Shrink them; never grow them silently. A change to a listed
/// method's parameters changes its entry, which is a roster change on purpose.
/// </remarks>
public class PublicApiConventionTests
{
    // Kept (2026-10-05): MapException awaits a task it is handed (the token belongs to whoever started it);
    // DisposeSafelyAsync wraps DisposeAsync, which takes no token.
    private static readonly string[] KnownUncancellable =
    [
        "IronHive.Abstractions.Extensions.ExceptionExtensions.MapException(Task<T>, Func<Exception, Exception>)",
        "IronHive.Core.Utilities.DisposalHelper.DisposeSafelyAsync(T)",
    ];

    // Kept (2026-10-05): an orchestration's result is the run's report (each agent's output, which steps failed), read
    // by the caller as data; the call throws for its own failures (cancellation, invalid configuration).
    private static readonly string[] KnownResultReturns =
    [
        "IronHive.Abstractions.Agent.Orchestration.IAgentOrchestrator.ExecuteAsync(IEnumerable<Message>, CancellationToken)",
        "IronHive.Core.Agent.Orchestration.OrchestratorBase.ExecuteAgentAsync(IAgent, IEnumerable<Message>, CancellationToken)",
    ];

    [Fact]
    public void PublicAsyncMethods_TakeACancellationToken() =>
        AsyncCancellation.Scan(OptionsReachabilityRosterTests.Libraries).ShouldMatchRoster(KnownUncancellable);

    [Fact]
    public void PublicMethods_DoNotReturnResultObjects() =>
        ResultReturns.Scan(OptionsReachabilityRosterTests.Libraries).ShouldMatchRoster(KnownResultReturns);

    // Positive control: an empty roster would also pass if the scan saw no public method at all.
    [Fact]
    public void Scan_SeesThePublicSurface() =>
        Assert.True(ResultReturns.Scan(OptionsReachabilityRosterTests.Libraries).MembersRead > 0, "the scan read too few public methods");
}

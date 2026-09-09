using AwesomeAssertions;
using IronHive.Plugins.MCP;

namespace IronHive.Tests.Plugins;

/// <summary>
/// Which utility counts as "alive" at a given negotiated revision.
///
/// <para>
/// The real-server fixture next door can only exercise the revision the SDK actually negotiates, which
/// today is 2026-07-28 or later. The initialize-era branch — a server old enough that <c>ping</c> is
/// still the mandatory utility — is not reachable from a current SDK server, so the rule itself is
/// pinned here instead. Stating that plainly is the point: the end-to-end test covers one branch, and
/// this covers the boundary the other branch sits behind.
/// </para>
/// </summary>
public class McpLivenessRevisionGateTests
{
    [Theory]
    [InlineData("2026-07-28", true)]   // the revision that removed ping
    [InlineData("2026-08-01", true)]
    [InlineData("2027-01-01", true)]
    [InlineData("2025-11-25", false)]  // initialize-era: ping is still mandatory
    [InlineData("2025-06-18", false)]
    public void TheGateFollowsTheRevisionDate(string negotiated, bool usesDiscover)
    {
        McpSession.UsesDiscoverLiveness(negotiated).Should().Be(usesDiscover);
    }

    [Fact]
    public void BeforeAnythingIsNegotiated_TheOlderUtilityIsUsed()
    {
        McpSession.UsesDiscoverLiveness(null).Should().BeFalse(
            "an unconnected session has agreed to nothing; assuming the newer revision would send a " +
            "request an initialize-era server has no method for");
    }
}

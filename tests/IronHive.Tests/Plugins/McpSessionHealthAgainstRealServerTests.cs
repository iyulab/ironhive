using AwesomeAssertions;
using IronHive.Plugins.MCP;
using IronHive.Plugins.MCP.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace IronHive.Tests.Plugins;

/// <summary>
/// <see cref="McpSession.HealthAsync"/> against a real MCP server.
///
/// <para>
/// The 2026-07-28 revision removed the <c>ping</c> utility and made <c>server/discover</c> mandatory.
/// The SDK negotiates that revision by default, yet the health check still judged liveness by ping —
/// so a server implementing exactly the current specification was reported unhealthy and moved to
/// <see cref="McpConnectionState.Errored"/> by the very call that asked whether it was alive.
/// </para>
/// <para>
/// This is asserted against a server hosted in-process on loopback rather than through a transport the
/// test hands the session. A transport seam would let the test decide what the handshake negotiated,
/// which is the one thing under test: whether the health check follows the revision the SDK actually
/// agreed on. Loopback keeps it runnable in CI — the external-process MCP fixtures elsewhere in this
/// ecosystem are skipped there, and a check CI never runs is not a check.
/// </para>
/// </summary>
public class McpSessionHealthAgainstRealServerTests : IAsyncLifetime
{
    private WebApplication? _app;
    private Uri _endpoint = null!;

    [McpServerToolType]
    private static class EchoTool
    {
        [McpServerTool(Name = "echo")]
        public static string Echo(string message) => message;
    }

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddMcpServer().WithHttpTransport().WithTools(typeof(EchoTool));

        _app = builder.Build();
        _app.MapMcp();
        await _app.StartAsync(TestContext.Current.CancellationToken);

        var address = _app.Services
            .GetRequiredService<global::Microsoft.AspNetCore.Hosting.Server.IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.First();
        _endpoint = new Uri(new Uri(address), "/");
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    private McpSession CreateSession() => new(new McpHttpClientConfig
    {
        ServerName = "health-fixture",
        Endpoint = _endpoint,
    });

    [Fact]
    public async Task AServerOnTheCurrentRevision_IsHealthy()
    {
        await using var session = CreateSession();

        await session.ConnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.State.Should().Be(McpConnectionState.Connected);

        var healthy = await session.HealthAsync(TestContext.Current.CancellationToken);

        healthy.Should().BeTrue(
            "a server implementing the negotiated revision must not be reported dead by the call that " +
            "asks whether it is alive");
        session.State.Should().Be(McpConnectionState.Connected,
            "and the check must not be what moves a healthy session to Errored");
    }

    [Fact]
    public async Task TheSessionReportsTheRevisionItNegotiated()
    {
        await using var session = CreateSession();

        session.NegotiatedProtocolVersion.Should().BeNull("nothing has been negotiated before connecting");

        await session.ConnectAsync(cancellationToken: TestContext.Current.CancellationToken);

        session.NegotiatedProtocolVersion.Should().NotBeNullOrWhiteSpace();
        McpSession.UsesDiscoverLiveness(session.NegotiatedProtocolVersion).Should().BeTrue(
            "the SDK negotiates 2026-07-28 or later by default -- if this ever goes false, the branch " +
            "the health check takes against a current server has changed and the test above is no " +
            "longer covering what it claims to");
    }

    [Fact]
    public async Task HealthAfterTheServerStops_IsFalseAndErrors()
    {
        await using var session = CreateSession();
        await session.ConnectAsync(cancellationToken: TestContext.Current.CancellationToken);

        await _app!.StopAsync(TestContext.Current.CancellationToken);

        var healthy = await session.HealthAsync(TestContext.Current.CancellationToken);

        healthy.Should().BeFalse("a server that is gone is not alive");
        session.State.Should().Be(McpConnectionState.Errored);
    }
}

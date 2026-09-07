using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using IronHive.Plugins.MCP;
using IronHive.Plugins.MCP.Configurations;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Protocol;

namespace IronHive.Tests.Plugins;

/// <summary>
/// What a session does with a server that completes the <c>initialize</c> handshake but then
/// misbehaves. The failure mode that matters is total and quiet: <see cref="McpClientManager"/>
/// registers a server's tools only from the <c>Connected</c> event and drops them on <c>Errored</c>,
/// so any post-handshake check that moves the session to <c>Errored</c> costs the consumer every
/// tool from that server with a state flag as the only signal. <c>initialize</c> already proves the
/// server answers requests; nothing sent right after it can prove more, so nothing sent right after
/// it may cost the session. Network-free: a fake JSON-RPC server behind
/// <see cref="StreamClientTransport"/> over in-process pipes.
/// </summary>
public sealed class McpSessionConnectPolicyTests
{
    private const string PingRejection = "The method 'ping' is not available on protocol version '2026-07-28'.";

    [Fact]
    public async Task Connect_ServerRejectsPingAfterInitialize_IsConnectedAndListsTools()
    {
        await using var server = new FakeMcpServer(rejectPing: true);
        await using var session = server.CreateSession();

        await session.ConnectAsync(cancellationToken: TestContext.Current.CancellationToken);

        session.State.Should().Be(McpConnectionState.Connected,
            "initialize completed; a utility the server lacks must not cost the session (message was: {0})", session.ErrorMessage);
        session.ErrorMessage.Should().BeNull();

        var tools = await session.ListToolsAsync(TestContext.Current.CancellationToken);
        tools.Select(t => t.Name).Should().Equal("echo");
        server.PingRequests.Should().Be(0, "connecting must not ping at all — initialize is the handshake");
    }

    [Fact]
    public async Task ListTools_ServerErrors_MovesSessionToErroredAndRaisesEvent()
    {
        await using var server = new FakeMcpServer(rejectPing: false, failToolsList: true);
        await using var session = server.CreateSession();
        McpErroredEventArgs? errored = null;
        session.Errored += (_, e) => errored = e;

        await session.ConnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.State.Should().Be(McpConnectionState.Connected);

        var act = () => session.ListToolsAsync(TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<Exception>("the caller must see the failure");
        session.State.Should().Be(McpConnectionState.Errored,
            "a server that cannot list its tools has failed the consumer as completely as one that cannot connect");
        session.ErrorMessage.Should().NotBeNullOrEmpty();
        errored.Should().NotBeNull("the manager removes this server's tools from the Errored event");
        errored!.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task Health_ServerRejectsPing_ReportsFalseAndErrored()
    {
        // HealthAsync is the explicit "is this server alive per spec" call; it keeps ping's verdict.
        await using var server = new FakeMcpServer(rejectPing: true);
        await using var session = server.CreateSession();
        await session.ConnectAsync(cancellationToken: TestContext.Current.CancellationToken);

        var healthy = await session.HealthAsync(TestContext.Current.CancellationToken);

        healthy.Should().BeFalse();
        session.State.Should().Be(McpConnectionState.Errored);
        session.ErrorMessage.Should().Contain("ping");
    }

    /// <summary>
    /// Minimal JSON-RPC 2.0 MCP server over newline-delimited JSON: answers <c>initialize</c> (echoing
    /// the client's protocol version), ignores <c>notifications/initialized</c>, serves one tool on
    /// <c>tools/list</c>, and either answers or rejects <c>ping</c> with the exact text a real
    /// non-compliant server produced. Everything else is "method not found".
    /// </summary>
    private sealed class FakeMcpServer : IAsyncDisposable
    {
        private readonly Pipe _clientToServer = new();
        private readonly Pipe _serverToClient = new();
        private readonly bool _rejectPing;
        private readonly bool _failToolsList;
        private readonly Task _loop;
        private int _pingRequests;

        public FakeMcpServer(bool rejectPing, bool failToolsList = false)
        {
            _rejectPing = rejectPing;
            _failToolsList = failToolsList;
            _loop = Task.Run(ServeAsync);
        }

        public int PingRequests => _pingRequests;

        public McpSession CreateSession() => new(
            new McpStdioClientConfig { ServerName = "fake", Command = "unused" },
            _ => new StreamClientTransport(
                serverInput: _clientToServer.Writer.AsStream(),
                serverOutput: _serverToClient.Reader.AsStream(),
                NullLoggerFactory.Instance));

        private async Task ServeAsync()
        {
            using var reader = new StreamReader(_clientToServer.Reader.AsStream(), Encoding.UTF8);
            await using var writer = new StreamWriter(_serverToClient.Writer.AsStream(), new UTF8Encoding(false)) { AutoFlush = true };

            while (await reader.ReadLineAsync() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                using var request = JsonDocument.Parse(line);
                var root = request.RootElement;
                var method = root.GetProperty("method").GetString();
                if (!root.TryGetProperty("id", out var id))
                    continue; // notification — nothing to answer

                var idJson = id.GetRawText();
                var response = method switch
                {
                    "initialize" => Result(idJson,
                        $$$"""{"protocolVersion":{{{root.GetProperty("params").GetProperty("protocolVersion").GetRawText()}}},"capabilities":{"tools":{}},"serverInfo":{"name":"fake","version":"0"}}"""),
                    "ping" when Interlocked.Increment(ref _pingRequests) > 0 && _rejectPing => Error(idJson, -32601, PingRejection),
                    "ping" => Result(idJson, "{}"),
                    "tools/list" when _failToolsList => Error(idJson, -32603, "tools/list exploded"),
                    "tools/list" => Result(idJson,
                        """{"tools":[{"name":"echo","description":"echoes","inputSchema":{"type":"object","properties":{}}}]}"""),
                    _ => Error(idJson, -32601, $"The method '{method}' is not available."),
                };
                await writer.WriteLineAsync(response);
            }
        }

        private static string Result(string id, string result) =>
            $$"""{"jsonrpc":"2.0","id":{{id}},"result":{{result}}}""";

        private static string Error(string id, int code, string message) =>
            $$$"""{"jsonrpc":"2.0","id":{{{id}}},"error":{"code":{{{code}}},"message":{{{JsonSerializer.Serialize(message)}}}}}""";

        public async ValueTask DisposeAsync()
        {
            await _clientToServer.Writer.CompleteAsync();
            await _serverToClient.Writer.CompleteAsync();
            try { await _loop.WaitAsync(TimeSpan.FromSeconds(5)); } catch { /* loop ends on EOF; a stuck fake must not hang the suite */ }
        }
    }
}

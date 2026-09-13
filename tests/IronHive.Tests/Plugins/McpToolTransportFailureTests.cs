using AwesomeAssertions;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Plugins.MCP;
using IronHive.Plugins.MCP.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace IronHive.Tests.Plugins;

/// <summary>
/// An MCP server that dies while a tool call is in flight, or is gone before it, is a tool failure
/// the model gets to read — the same outcome <c>FunctionTool</c> gives a throwing method — not an
/// exception that ends the whole answer. Since 0.26.0 the tool loop lets an escaping exception fail
/// the call, so the two built-in tools have to agree on what escapes: only the caller's own
/// cancellation.
///
/// <para>
/// Asserted against a server hosted in-process on loopback and stopped mid-call, with a short
/// shutdown timeout so the in-flight request is aborted rather than drained — the transport failure
/// a stdio server's death produces, reachable in CI.
/// </para>
/// </summary>
public class McpToolTransportFailureTests : IAsyncLifetime
{
    private WebApplication? _app;
    private Uri _endpoint = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddMcpServer().WithHttpTransport().WithTools<McpTransportFixtureTools>();
        // Abort in-flight requests promptly on stop instead of draining them: the death of a server.
        builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromMilliseconds(200));

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
        ServerName = "transport-fixture",
        Endpoint = _endpoint,
    });

    private static async Task<McpTool> ToolAsync(McpSession session, string name)
    {
        await session.ConnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        var tools = await session.ListToolsAsync(TestContext.Current.CancellationToken);
        return tools.Single(t => t.Name == name);
    }

    [Fact]
    public async Task AServerThatDiesMidCall_IsAFailedToolResult_NotAnEscapingException()
    {
        await using var session = CreateSession();
        var hang = await ToolAsync(session, "hang");

        var call = hang.InvokeAsync(new ToolInput(), TestContext.Current.CancellationToken);
        await McpTransportFixtureTools.HangStarted.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        await _app!.StopAsync(TestContext.Current.CancellationToken);

        var output = await call;

        output.IsSuccess.Should().BeFalse("the server went away before it answered");
        output.Content.Should().ContainSingle().Which.Should().BeOfType<TextMessageContent>()
            .Which.Value.Should().Contain("transport-fixture").And.Contain("hang",
                "the model has to be told which server and which tool failed to answer");
    }

    [Fact]
    public async Task AServerThatIsGoneBeforeTheCall_IsAFailedToolResult()
    {
        await using var session = CreateSession();
        var echo = await ToolAsync(session, "echo");
        await _app!.StopAsync(TestContext.Current.CancellationToken);

        var output = await echo.InvokeAsync(new ToolInput(new { message = "hi" }), TestContext.Current.CancellationToken);

        output.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task TheCallersOwnCancellation_StillEscapes()
    {
        await using var session = CreateSession();
        var hang = await ToolAsync(session, "hang");
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        var act = () => hang.InvokeAsync(new ToolInput(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>(
            "cancellation the caller asked for is the caller's to handle, as it is for FunctionTool");
    }

    [Fact]
    public async Task ALiveServer_StillAnswers()
    {
        await using var session = CreateSession();
        var echo = await ToolAsync(session, "echo");

        var output = await echo.InvokeAsync(new ToolInput(new { message = "hi" }), TestContext.Current.CancellationToken);

        output.IsSuccess.Should().BeTrue();
        output.Content.Should().ContainSingle().Which.Should().BeOfType<TextMessageContent>().Which.Value.Should().Be("hi");
    }
}

/// <summary>Tools of the loopback fixture: one that answers, one that never does.</summary>
[McpServerToolType]
public sealed class McpTransportFixtureTools
{
    public static readonly TaskCompletionSource HangStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [McpServerTool(Name = "echo")]
    public static string Echo(string message) => message;

    [McpServerTool(Name = "hang")]
    public static async Task<string> Hang(CancellationToken cancellationToken)
    {
        HangStarted.TrySetResult();
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        return "never";
    }
}

using System.Text.RegularExpressions;
using AwesomeAssertions;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
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
/// An MCP server may name a tool <c>message.send</c> - legal in MCP, but not a function name a provider accepts. The
/// name the model is offered (<see cref="ITool.UniqueName"/>) must match <c>[a-zA-Z0-9_-]{1,64}</c>, and the model's call by
/// that name must still reach the tool. Asserted against a server hosted in-process on loopback.
/// </summary>
public partial class McpToolUniqueNameTests : IAsyncLifetime
{
    [GeneratedRegex("^[a-zA-Z0-9_-]{1,64}$")]
    private static partial Regex ProviderPattern();

    private WebApplication? _app;
    private Uri _endpoint = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddMcpServer().WithHttpTransport().WithTools<McpDottedNameFixtureTools>();
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

    private static async Task<IReadOnlyList<McpTool>> ToolsAsync(McpSession session)
    {
        await session.ConnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (await session.ListToolsAsync(TestContext.Current.CancellationToken)).ToList();
    }

    private McpSession CreateSession() => new(new McpHttpClientConfig
    {
        ServerName = "chat",
        Endpoint = _endpoint,
    });

    [Fact]
    public async Task A_dotted_tool_is_offered_under_a_name_the_providers_accept_and_the_call_by_that_name_reaches_it()
    {
        await using var session = CreateSession();
        var tools = await ToolsAsync(session);
        var dotted = tools.Single(t => t.Name == "message.send");

        dotted.UniqueName.Should().MatchRegex(ProviderPattern().ToString()).And.StartWith("mcp_chat_message_send_");

        // The collection key is the name the model sees and the name its call is looked up by - one string.
        var collection = new ToolCollection(tools);
        collection.TryGet(dotted.UniqueName, out var found).Should().BeTrue();
        var output = await found!.InvokeAsync(new ToolInput(new { text = "hi" }), TestContext.Current.CancellationToken);
        output.IsSuccess.Should().BeTrue();
        output.Content.Should().ContainSingle().Which.Should().BeOfType<TextMessageContent>().Which.Value.Should().Be("sent: hi");
    }

    [Fact]
    public async Task A_plain_tool_keeps_the_name_it_always_had()
    {
        await using var session = CreateSession();
        var tools = await ToolsAsync(session);

        tools.Single(t => t.Name == "echo").UniqueName.Should().Be("mcp_chat_echo");
    }
}

/// <summary>Tools of the loopback fixture: one with a dot in its name, one without.</summary>
[McpServerToolType]
public sealed class McpDottedNameFixtureTools
{
    [McpServerTool(Name = "message.send")]
    public static string Send(string text) => $"sent: {text}";

    [McpServerTool(Name = "echo")]
    public static string Echo(string text) => text;
}

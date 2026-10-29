using AwesomeAssertions;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using IronHive.Plugins.OpenAPI;

namespace IronHive.Tests.Plugins;

/// <summary>
/// Registering an OpenAPI client lists its operations as tools. The listing used to run in a fire-and-forget
/// continuation that read <c>task.Result</c> without checking for a fault: a listing that failed (here: cancelled)
/// left the client registered with no tools and nobody told.
/// </summary>
public class OpenApiClientManagerTests
{
    private const string WithServers = """
        {
          "openapi": "3.0.1",
          "info": { "title": "pets", "version": "1" },
          "servers": [ { "url": "https://example.test" } ],
          "paths": {
            "/pets": { "get": { "operationId": "listPets", "responses": { "200": { "description": "ok" } } } }
          }
        }
        """;

    private const string WithoutServers = """
        {
          "openapi": "3.0.1",
          "info": { "title": "pets", "version": "1" },
          "paths": {
            "/pets": { "get": { "operationId": "listPets", "responses": { "200": { "description": "ok" } } } }
          }
        }
        """;

    // A spec without servers used to register fine and fail on the first tool call; it now fails at registration.
    [Fact]
    public async Task AddOrUpdateAsync_ASpecWithNoAbsoluteServerUrl_FailsAtRegistration()
    {
        var tools = new ToolCollection();
        var manager = new OpenApiClientManager(tools);

        var act = () => manager.AddOrUpdateAsync(OpenApiClientFactory.CreateFromString("pets", WithoutServers), TestContext.Current.CancellationToken);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Contain("listPets");
        manager.TryGetClient("pets", out _).Should().BeFalse();
    }

    private static CancellationToken Cancelled() => new(canceled: true);

    private static int ToolsOf(IToolCollection tools, string client) =>
        tools.Count(t => t is OpenApiTool ot && ot.ClientName == client);

    [Fact]
    public async Task AddOrUpdateAsync_RegistersTheSpecsOperations_BeforeReturning()
    {
        var tools = new ToolCollection();
        var manager = new OpenApiClientManager(tools);

        await manager.AddOrUpdateAsync(OpenApiClientFactory.CreateFromString("pets", WithServers), TestContext.Current.CancellationToken);

        ToolsOf(tools, "pets").Should().Be(1);
        manager.TryGetClient("pets", out _).Should().BeTrue();
    }

    [Fact]
    public async Task AddOrUpdateAsync_AClientWhoseToolsCannotBeListed_Throws_AndRegistersNothing()
    {
        var tools = new ToolCollection();
        var manager = new OpenApiClientManager(tools);

        var act = () => manager.AddOrUpdateAsync(OpenApiClientFactory.CreateFromString("pets", WithServers), Cancelled());

        await act.Should().ThrowAsync<OperationCanceledException>();
        manager.TryGetClient("pets", out _).Should().BeFalse();
        ToolsOf(tools, "pets").Should().Be(0);
    }

    [Fact]
    public async Task AddOrUpdateAsync_AFailedReplacement_KeepsThePreviousClientAndItsTools()
    {
        var tools = new ToolCollection();
        var manager = new OpenApiClientManager(tools);
        var first = OpenApiClientFactory.CreateFromString("pets", WithServers);
        await manager.AddOrUpdateAsync(first, TestContext.Current.CancellationToken);

        var act = () => manager.AddOrUpdateAsync(OpenApiClientFactory.CreateFromString("pets", WithServers), Cancelled());

        await act.Should().ThrowAsync<OperationCanceledException>();
        manager.TryGetClient("pets", out var kept).Should().BeTrue();
        kept.Should().BeSameAs(first);
        ToolsOf(tools, "pets").Should().Be(1);
    }

    [Fact]
    public async Task AddOrUpdateAsync_AReplacement_SwapsTheTools_WithoutDuplicates()
    {
        var tools = new ToolCollection();
        var manager = new OpenApiClientManager(tools);
        await manager.AddOrUpdateAsync(OpenApiClientFactory.CreateFromString("pets", WithServers), TestContext.Current.CancellationToken);

        var second = OpenApiClientFactory.CreateFromString("pets", WithServers);
        await manager.AddOrUpdateAsync(second, TestContext.Current.CancellationToken);

        ToolsOf(tools, "pets").Should().Be(1);
        manager.TryGetClient("pets", out var current).Should().BeTrue();
        current.Should().BeSameAs(second);
    }
}

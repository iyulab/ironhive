using AwesomeAssertions;
using IronHive.Plugins.MCP;
using IronHive.Plugins.MCP.Configurations;
using ModelContextProtocol.Client;

namespace IronHive.Tests.Plugins;

/// <summary>
/// Applies to the plugin transports the question already answered for the providers and the storages: does
/// each configuration field reach its own slot? The risk indicator that found real pairs there — adjacent
/// parameters of the same type, where a swap compiles — is present here too. The stdio transport takes a
/// server name, a command and a working directory as three strings, so a swap launches the wrong process or
/// launches the right one from the wrong place; the OAuth options take a client id and a client secret as
/// two strings, so a swap sends the secret as the public identifier.
/// </summary>
public class McpConfigMappingTests
{
    private static McpStdioClientConfig Stdio() => new()
    {
        ServerName = "files",
        Command = "npx",
        Arguments = ["-y", "@modelcontextprotocol/server-filesystem"],
        EnvironmentVariables = new Dictionary<string, string?> { ["ROOT"] = "/data" },
        ShutdownTimeout = TimeSpan.FromSeconds(9),
        WorkingDirectory = "/srv/app",
    };

    private static McpHttpClientConfig Http() => new()
    {
        ServerName = "remote",
        Endpoint = new Uri("https://mcp.example/sse"),
        AdditionalHeaders = new Dictionary<string, string> { ["X-Tenant"] = "acme" },
        ConnectionTimeout = TimeSpan.FromSeconds(45),
    };

    [Fact]
    public void Stdio_ServerNameCommandAndWorkingDirectory_StayInTheirOwnSlots()
    {
        var options = McpSession.BuildStdioOptions(Stdio());

        options.Name.Should().Be("files");
        options.Command.Should().Be("npx");
        options.WorkingDirectory.Should().Be("/srv/app");
        options.Command.Should().NotBe("files", "the server name must never be executed as the command");
        options.Command.Should().NotBe("/srv/app");
    }

    [Fact]
    public void Stdio_ArgumentsEnvironmentAndShutdown_ReachTheirOwnSlots()
    {
        var options = McpSession.BuildStdioOptions(Stdio());

        options.Arguments.Should().Equal("-y", "@modelcontextprotocol/server-filesystem");
        options.EnvironmentVariables.Should().ContainKey("ROOT").WhoseValue.Should().Be("/data");
        options.ShutdownTimeout.Should().Be(TimeSpan.FromSeconds(9));
    }

    [Fact]
    public void Stdio_AbsentOptionalFields_StayAbsent()
    {
        var options = McpSession.BuildStdioOptions(new McpStdioClientConfig
        {
            ServerName = "files",
            Command = "npx",
        });

        options.Arguments.Should().BeNull("an absent argument list must not become an empty one");
        options.EnvironmentVariables.Should().BeNull();
        options.WorkingDirectory.Should().BeNull();
    }

    [Fact]
    public void Http_NameEndpointHeadersAndTimeout_ReachTheirOwnSlots()
    {
        var options = McpSession.BuildHttpOptions(Http());

        options.Name.Should().Be("remote");
        options.Endpoint.Should().Be(new Uri("https://mcp.example/sse"));
        options.AdditionalHeaders.Should().ContainKey("X-Tenant").WhoseValue.Should().Be("acme");
        options.ConnectionTimeout.Should().Be(TimeSpan.FromSeconds(45));
    }

    [Fact]
    public void Http_TransportModeIsAdapterPolicy_NotLeftToTheVendorDefault()
    {
        McpSession.BuildHttpOptions(Http()).TransportMode.Should().Be(HttpTransportMode.AutoDetect);
    }

    [Fact]
    public void Http_WithoutOAuth_LeavesTheOptionsUnset()
    {
        McpSession.BuildHttpOptions(Http()).OAuth
            .Should().BeNull("no OAuth configuration must not become an empty credential set");
    }

    [Fact]
    public void Http_OAuthClientIdAndSecret_StayInTheirOwnSlots()
    {
        var config = Http();
        config.OAuth = new McpHttpOAuthConfig
        {
            RedirectUri = new Uri("https://app.example/callback"),
            ClientId = "public-id",
            ClientSecret = "secret-value",
            Scopes = ["tools.read"],
        };

        var oauth = McpSession.BuildHttpOptions(config).OAuth;

        oauth.Should().NotBeNull();
        oauth!.ClientId.Should().Be("public-id");
        oauth.ClientSecret.Should().Be("secret-value");
        oauth.ClientId.Should().NotBe("secret-value", "the secret must never be sent as the public identifier");
        oauth.RedirectUri.Should().Be(new Uri("https://app.example/callback"));
        oauth.Scopes.Should().Equal("tools.read");
    }

    [Fact]
    public void Http_OAuthWithoutAdditionalParameters_BecomesEmptyRatherThanNull()
    {
        var config = Http();
        config.OAuth = new McpHttpOAuthConfig { RedirectUri = new Uri("https://app.example/callback") };

        McpSession.BuildHttpOptions(config).OAuth!
            .AdditionalAuthorizationParameters.Should().BeEmpty();
    }

    /// <summary>
    /// The equality override omitted <see cref="McpHttpClientConfig.OAuth"/>, so two configurations that
    /// differed only in their credentials compared equal. Nothing gates on that today — the client manager
    /// reconnects unconditionally — but an incomplete override is a trap for whatever starts to.
    /// </summary>
    [Fact]
    public void HttpConfigs_DifferingOnlyInCredentials_AreNotEqual()
    {
        var left = Http();
        left.OAuth = new McpHttpOAuthConfig
        {
            RedirectUri = new Uri("https://app.example/callback"),
            ClientId = "id-a",
        };

        var right = Http();
        right.OAuth = new McpHttpOAuthConfig
        {
            RedirectUri = new Uri("https://app.example/callback"),
            ClientId = "id-b",
        };

        left.Should().NotBe(right);
        left.GetHashCode().Should().NotBe(right.GetHashCode());
    }

    [Fact]
    public void HttpConfigs_WithTheSameCredentials_AreEqual()
    {
        static McpHttpOAuthConfig OAuth() => new()
        {
            RedirectUri = new Uri("https://app.example/callback"),
            ClientId = "id",
            ClientSecret = "secret",
            Scopes = ["tools.read"],
        };

        var left = Http();
        left.OAuth = OAuth();
        var right = Http();
        right.OAuth = OAuth();

        left.Should().Be(right);
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void HttpConfig_WithAndWithoutOAuth_AreNotEqual()
    {
        var withOAuth = Http();
        withOAuth.OAuth = new McpHttpOAuthConfig { RedirectUri = new Uri("https://app.example/callback") };

        withOAuth.Should().NotBe(Http());
    }

    /// <summary>
    /// The stdio override compared its argument list and environment by reference, so the same settings
    /// deserialised twice produced unequal configurations — the opposite failure to the HTTP one above, and
    /// from the same cause: a collection's default comparer is identity.
    /// </summary>
    [Fact]
    public void StdioConfigs_WithTheSameArgumentsAndEnvironment_AreEqual()
    {
        Stdio().Should().Be(Stdio());
        Stdio().GetHashCode().Should().Be(Stdio().GetHashCode());
    }

    [Fact]
    public void StdioConfigs_DifferingInArgumentOrder_AreNotEqual()
    {
        var reordered = Stdio();
        reordered.Arguments = ["@modelcontextprotocol/server-filesystem", "-y"];

        reordered.Should().NotBe(Stdio(), "command-line arguments are position-sensitive");
    }

    [Fact]
    public void StdioConfigs_DifferingInEnvironmentValue_AreNotEqual()
    {
        var other = Stdio();
        other.EnvironmentVariables = new Dictionary<string, string?> { ["ROOT"] = "/elsewhere" };

        other.Should().NotBe(Stdio());
    }
}

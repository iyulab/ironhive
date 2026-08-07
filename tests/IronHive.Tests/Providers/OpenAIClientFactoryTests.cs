using System.ClientModel.Primitives;
using FluentAssertions;
using IronHive.Abstractions;
using IronHive.Core;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;

namespace IronHive.Tests.Providers;

/// <summary>
/// The factory maps <see cref="OpenAIConfig"/> onto the vendor client's options. The constructed
/// client exposes none of those values, so a field routed to the wrong slot is invisible at compile
/// time and produces an error naming neither the field nor the factory. The mapping is therefore
/// asserted rather than inferred from a successful construction.
/// </summary>
public class OpenAIClientFactoryTests
{
    private const string Gateway = "https://gateway.example/v1";

    [Fact]
    public void BaseUrl_ReachesTheEndpoint_WithATrailingSeparator()
    {
        var options = OpenAIClientFactory.BuildOptions(new OpenAIConfig { BaseUrl = Gateway });

        options.Endpoint.Should().Be(new Uri(Gateway + "/"));
    }

    [Fact]
    public void BaseUrl_AlreadyEndingInASeparator_IsNotDoubled()
    {
        var options = OpenAIClientFactory.BuildOptions(new OpenAIConfig { BaseUrl = Gateway + "/" });

        options.Endpoint.Should().Be(new Uri(Gateway + "/"));
    }

    [Fact]
    public void NoBaseUrl_LeavesTheVendorDefaultInPlace()
    {
        var options = OpenAIClientFactory.BuildOptions(new OpenAIConfig { ApiKey = "sk-test" });

        options.Endpoint.Should().BeNull("an unset base URL must not become an empty endpoint");
    }

    [Fact]
    public void OrganizationAndProject_ReachTheirOwnSlots()
    {
        var options = OpenAIClientFactory.BuildOptions(new OpenAIConfig
        {
            ApiKey = "sk-test",
            Organization = "org-1",
            Project = "proj-1",
        });

        options.OrganizationId.Should().Be("org-1");
        options.ProjectId.Should().Be("proj-1");
    }

    [Fact]
    public void BaseUrlWithOrganizationAndProject_KeepsAllThreeSeparate()
    {
        var options = OpenAIClientFactory.BuildOptions(new OpenAIConfig
        {
            BaseUrl = Gateway,
            Organization = "org-1",
            Project = "proj-1",
        });

        options.Endpoint.Should().Be(new Uri(Gateway + "/"));
        options.OrganizationId.Should().Be("org-1");
        options.ProjectId.Should().Be("proj-1");
        options.OrganizationId.Should().NotBe(Gateway, "a base URL must never land in another slot");
        options.ProjectId.Should().NotBe(Gateway);
    }

    [Fact]
    public void Timeout_ReachesTheNetworkBudget()
    {
        var options = OpenAIClientFactory.BuildOptions(new OpenAIConfig
        {
            ApiKey = "sk-test",
            TimeOut = TimeSpan.FromMinutes(3),
        });

        options.NetworkTimeout.Should().Be(TimeSpan.FromMinutes(3));
    }

    [Fact]
    public void DefaultTimeout_IsCarriedRatherThanLeftToTheSdk()
    {
        var options = OpenAIClientFactory.BuildOptions(new OpenAIConfig { ApiKey = "sk-test" });

        options.NetworkTimeout.Should().Be(new OpenAIConfig().TimeOut);
    }

    [Fact]
    public void InjectedHttpClient_BecomesTheTransport()
    {
        using var http = new HttpClient();

        var options = OpenAIClientFactory.BuildOptions(new OpenAIConfig
        {
            ApiKey = "sk-test",
            HttpClient = http,
        });

        options.Transport.Should().BeOfType<HttpClientPipelineTransport>();
    }

    [Fact]
    public void NoInjectedHttpClient_LeavesTheSdkTransportInPlace()
    {
        var options = OpenAIClientFactory.BuildOptions(new OpenAIConfig { ApiKey = "sk-test" });

        options.Transport.Should().BeNull("the SDK's own transport disables the request timeout");
    }

    /// <summary>
    /// A keyless server is a first-class configuration for the OpenAI-compatible provider — locally
    /// hosted runtimes routinely require no credential, and <c>OpenAICompatibleConfig</c> declares the
    /// key optional. That config is converted to an <see cref="OpenAIConfig"/> and handed to this
    /// factory at registration time, so the keyless path has to survive client construction.
    /// </summary>
    [Fact]
    public void KeylessConfig_StillProducesAClient()
    {
        var act = () => OpenAIClientFactory.Create(new OpenAIConfig { BaseUrl = "http://localhost:1234/v1" });

        act.Should().NotThrow("a keyless local server must not fail at client construction");
    }

    [Fact]
    public void KeylessCompatibleConfig_SurvivesTheConversionAndConstruction()
    {
        var compatible = new OpenAICompatibleConfig { BaseUrl = "http://localhost:1234" };

        var act = () => OpenAIClientFactory.Create(compatible.ToOpenAI());

        act.Should().NotThrow("OpenAICompatibleConfig declares the API key optional");
    }

    /// <summary>
    /// Registration is where a consumer meets this: the model finder and the embedding generator are
    /// both constructed eagerly by the provider registration helpers, so an absent key aborted
    /// <c>Build()</c> rather than surfacing at first use.
    /// </summary>
    [Fact]
    public void KeylessConfig_ConstructsTheEagerlyBuiltServices()
    {
        var config = new OpenAICompatibleConfig { BaseUrl = "http://localhost:1234" }.ToOpenAI();

        var finder = () => new OpenAIModelFinder(config);
        var embeddings = () => new OpenAIEmbeddingGenerator(config);

        finder.Should().NotThrow();
        embeddings.Should().NotThrow();
    }

    /// <summary>
    /// The shipped samples read credentials from the environment and fall back to
    /// <see cref="string.Empty"/>. With a variable unset, that empty key reached this factory through
    /// provider registration and threw before any request was made.
    /// </summary>
    [Fact]
    public void RegisteringProvidersWithAnUnsetEnvironmentKey_DoesNotAbortTheBuild()
    {
        var absent = Environment.GetEnvironmentVariable("IRONHIVE_TEST_KEY_THAT_IS_NOT_SET") ?? string.Empty;

        var act = () => new HiveServiceBuilder()
            .AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = absent })
            .AddOpenAICompatibleProviders("compatible", new OpenAICompatibleConfig
            {
                BaseUrl = "http://localhost:1234",
                ApiKey = absent,
            })
            .Build();

        act.Should().NotThrow("a missing credential must surface as an API error, not a registration crash");
    }

    /// <summary>
    /// The registration examples in <c>docs/PROVIDERS.md</c> for the OpenAI-compatible provider omit the
    /// API key, because the servers that section covers require none. Those exact forms are pinned here
    /// so the documented call and the code that has to accept it cannot drift apart.
    /// </summary>
    [Theory]
    [InlineData("ollama", "http://localhost:11434")]
    [InlineData("lmstudio", "http://localhost:1234")]
    [InlineData("llamacpp", "http://localhost:8080")]
    public void TheDocumentedKeylessRegistration_Builds(string providerName, string baseUrl)
    {
        var act = () => new HiveServiceBuilder()
            .AddOpenAICompatibleProviders(providerName, new OpenAICompatibleConfig { BaseUrl = baseUrl })
            .Build();

        act.Should().NotThrow();
    }
}

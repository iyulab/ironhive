using FluentAssertions;
using Google.Apis.Auth.OAuth2;
using NSubstitute;
using IronHive.Providers.GoogleAI;

namespace IronHive.Tests.Providers;

/// <summary>
/// Each configuration field has to reach its own argument of the vendor client. The constructed client
/// exposes none of them, so a field routed to the wrong parameter compiles and — for a credential —
/// produces an error naming neither the field nor the factory. That is exactly how a base URL reached the
/// credential slot in the Anthropic adapter and survived several releases, so the mapping is asserted
/// here rather than inferred from a successful construction.
/// <para>
/// The existing <see cref="GoogleAIClientFactoryTests"/> covers the timeout. These cover everything else,
/// and the fact that Gemini-only and Vertex-only fields stay out of each other's slots.
/// </para>
/// </summary>
public class GoogleAISlotMappingTests
{
    [Fact]
    public void GoogleAI_ApiKey_ReachesTheKeySlot_AndNotTheProjectOrLocation()
    {
        var arguments = GoogleAIClientFactory.BuildArguments(new GoogleAIConfig { ApiKey = "AIza-test" });

        arguments.ApiKey.Should().Be("AIza-test");
        arguments.Project.Should().BeNull("project addresses Vertex, not Gemini");
        arguments.Location.Should().BeNull();
        arguments.Credential.Should().BeNull("Gemini authenticates with a key, not a credential");
    }

    [Fact]
    public void GoogleAI_TargetsTheGeminiSurface_NotVertex()
    {
        GoogleAIClientFactory.BuildArguments(new GoogleAIConfig { ApiKey = "k" })
            .VertexAI.Should().BeFalse();
    }

    [Fact]
    public void VertexAI_ProjectAndLocation_ReachTheirOwnSlots()
    {
        var arguments = GoogleAIClientFactory.BuildArguments(new VertexAIConfig
        {
            Project = "my-project",
            Location = "us-central1",
        });

        arguments.Project.Should().Be("my-project");
        arguments.Location.Should().Be("us-central1");
        arguments.Project.Should().NotBe("us-central1", "the two must not be swapped");
        arguments.ApiKey.Should().BeNull("Vertex has no API key slot to leak into");
    }

    [Fact]
    public void VertexAI_TargetsTheVertexSurface()
    {
        GoogleAIClientFactory.BuildArguments(VertexBase()).VertexAI.Should().BeTrue();
    }

    [Fact]
    public void VertexAI_Credential_ReachesTheCredentialSlot()
    {
        var credential = Substitute.For<ICredential>();
        var config = VertexBase();
        config.Credential = credential;

        var arguments = GoogleAIClientFactory.BuildArguments(config);

        arguments.Credential.Should().BeSameAs(credential);
        arguments.ApiKey.Should().BeNull("a credential must never be reduced to a key string");
    }

    [Fact]
    public void VertexAI_WithoutACredential_LeavesTheSlotEmptyForApplicationDefaultCredentials()
    {
        GoogleAIClientFactory.BuildArguments(VertexBase())
            .Credential.Should().BeNull("the vendor falls back to Application Default Credentials");
    }

    [Fact]
    public void GoogleAI_HttpClientFactory_ReachesTheClientOptions()
    {
        Func<HttpClient> factory = static () => new HttpClient();

        var arguments = GoogleAIClientFactory.BuildArguments(new GoogleAIConfig
        {
            ApiKey = "k",
            HttpClientFactory = factory,
        });

        arguments.ClientOptions.Should().NotBeNull();
        arguments.ClientOptions!.HttpClientFactory.Should().BeSameAs(factory);
    }

    [Fact]
    public void VertexAI_HttpClientFactory_ReachesTheClientOptions()
    {
        Func<HttpClient> factory = static () => new HttpClient();
        var config = VertexBase();
        config.HttpClientFactory = factory;

        GoogleAIClientFactory.BuildArguments(config).ClientOptions!
            .HttpClientFactory.Should().BeSameAs(factory);
    }

    [Fact]
    public void NoHttpClientFactory_LeavesTheClientOptionsUnset()
    {
        GoogleAIClientFactory.BuildArguments(new GoogleAIConfig { ApiKey = "k" })
            .ClientOptions.Should().BeNull("an absent factory must not become an empty options object");
    }

    [Fact]
    public void BothConfigurations_CarryTheResolvedTimeoutIntoTheSameSlot()
    {
        var gemini = GoogleAIClientFactory.BuildArguments(
            new GoogleAIConfig { ApiKey = "k", Timeout = TimeSpan.FromMinutes(3) });

        var vertex = VertexBase();
        vertex.Timeout = TimeSpan.FromMinutes(3);

        gemini.HttpOptions.Timeout.Should().Be(180_000);
        GoogleAIClientFactory.BuildArguments(vertex).HttpOptions.Timeout.Should().Be(180_000);
    }

    [Fact]
    public void CreateGoesThroughTheAssertedMapping()
    {
        // Construction must succeed for the mapping above to be the mapping actually used.
        var act = () => GoogleAIClientFactory.Create(new GoogleAIConfig { ApiKey = "AIza-test" });

        act.Should().NotThrow();
    }

    private static VertexAIConfig VertexBase() => new()
    {
        Project = "p",
        Location = "us-central1",
    };
}

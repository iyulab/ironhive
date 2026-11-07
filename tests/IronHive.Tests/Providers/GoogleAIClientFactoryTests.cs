using AwesomeAssertions;
using Google.GenAI.Types;
using IronHive.Providers.GoogleAI;
using System.Threading;

namespace IronHive.Tests.Providers;

/// <summary>
/// The timeout a consumer configures has to reach the vendor client, and the vendor's own default —
/// a bare HttpClient's 100 seconds — must never be inherited silently. Both are asserted against the
/// options the factory hands to the client rather than inferred from a successful construction.
/// </summary>
public class GoogleAIClientFactoryTests
{
    // The vendor Client exposes none of this once constructed, so the value tests target the mapping
    // the factory hands straight to `new Client(httpOptions: …)`. The guard tests below go through
    // Create itself, so the wiring between the two is exercised rather than assumed.
    private static HttpOptions Resolve(GoogleAIConfig config)
        => GoogleAIClientFactory.ResolveHttpOptions(config.HttpOptions, config.Timeout, nameof(GoogleAIConfig));

    private static HttpOptions Resolve(VertexAIConfig config)
        => GoogleAIClientFactory.ResolveHttpOptions(config.HttpOptions, config.Timeout, nameof(VertexAIConfig));

    private static VertexAIConfig VertexBase() => new()
    {
        Project = "p",
        Location = "us-central1",
    };

    [Theory]
    [InlineData("https://gateway.example/", "https://gateway.example")]
    [InlineData("https://gateway.example/gemini//", "https://gateway.example/gemini")]
    [InlineData("https://gateway.example/gemini", "https://gateway.example/gemini")]
    public void BaseUrl_TrailingSlash_IsDropped(string configured, string expected)
    {
        // The SDK adds its own slash before the API version; «https://gateway.example/» used to reach «//v1beta».
        var options = Resolve(new GoogleAIConfig { ApiKey = "k", HttpOptions = new HttpOptions { BaseUrl = configured } });

        options.BaseUrl.Should().Be(expected);
    }

    // Google.GenAI 2.0.0 moved the Gemini Developer API default from v1beta to v1, where the *-latest model aliases do not exist.
    // The provider keeps what it always sent unless the configuration or the environment names a version.
    [Fact]
    public void GoogleAI_NoApiVersionAnywhere_KeepsV1Beta()
    {
        var options = GoogleAIClientFactory.KeepDeveloperApiVersion(new HttpOptions(), environmentApiVersion: null);

        options.ApiVersion.Should().Be("v1beta");
    }

    [Theory]
    [InlineData("v1")]
    [InlineData("v1alpha")]
    public void GoogleAI_ConfiguredApiVersion_IsNotReplaced(string configured)
    {
        var options = GoogleAIClientFactory.KeepDeveloperApiVersion(new HttpOptions { ApiVersion = configured }, environmentApiVersion: null);

        options.ApiVersion.Should().Be(configured);
    }

    [Theory]
    [InlineData("v1")]
    [InlineData("beta")]
    public void GoogleAI_EnvironmentApiVersion_IsLeftToTheSdk(string fromEnvironment)
    {
        // GOOGLE_GENAI_API_VERSION is the operator's switch; a value written into the options would outrank it.
        var options = GoogleAIClientFactory.KeepDeveloperApiVersion(new HttpOptions(), fromEnvironment);

        options.ApiVersion.Should().BeNull();
    }

    [Fact]
    public void GoogleAI_NoTimeoutConfigured_LeavesTheRequestUnbounded()
    {
        // No HttpOptions.Timeout is imposed — the HttpClient built by ResolveHttpClientFactory already
        // has an unbounded Timeout, so the vendor's bare-HttpClient 100-second default is never reached.
        var options = Resolve(new GoogleAIConfig { ApiKey = "k" });

        options.Timeout.Should().BeNull();
    }

    [Fact]
    public void VertexAI_NoTimeoutConfigured_LeavesTheRequestUnbounded()
    {
        var options = Resolve(VertexBase());

        options.Timeout.Should().BeNull();
    }

    [Fact]
    public void GoogleAI_NoHttpClientFactoryConfigured_DefaultFactoryDisablesItsOwnRequestTimeout()
    {
        var factory = GoogleAIClientFactory.ResolveHttpClientFactory(null, TimeSpan.FromSeconds(5));

        using var http = factory();
        http.Timeout.Should().Be(Timeout.InfiniteTimeSpan);
    }

    [Fact]
    public void GoogleAI_HttpClientFactoryConfigured_IsUsedAsIs()
    {
        using var expected = new HttpClient();
        var factory = GoogleAIClientFactory.ResolveHttpClientFactory(() => expected, TimeSpan.FromSeconds(5));

        factory().Should().BeSameAs(expected);
    }

    [Fact]
    public void GoogleAI_Timeout_ReachesTheClientInMilliseconds()
    {
        var options = Resolve(new GoogleAIConfig { ApiKey = "k", Timeout = TimeSpan.FromMinutes(3) });

        options.Timeout.Should().Be(180_000);
    }

    [Fact]
    public void VertexAI_Timeout_ReachesTheClientInMilliseconds()
    {
        var config = VertexBase();
        config.Timeout = TimeSpan.FromMinutes(3);

        Resolve(config).Timeout.Should().Be(180_000);
    }

    [Fact]
    public void GoogleAI_Timeout_DoesNotDiscardOtherHttpOptions()
    {
        var options = Resolve(new GoogleAIConfig
        {
            ApiKey = "k",
            Timeout = TimeSpan.FromMinutes(2),
            HttpOptions = new HttpOptions { BaseUrl = "https://gateway.example" },
        });

        options.Timeout.Should().Be(120_000);
        options.BaseUrl.Should().Be("https://gateway.example");
    }

    [Fact]
    public void GoogleAI_HttpOptionsTimeoutAlone_IsStillHonoured()
    {
        // The vendor options remain a valid way to set this; adding the dedicated property must not
        // break a consumer who already reached through them.
        var options = Resolve(new GoogleAIConfig
        {
            ApiKey = "k",
            HttpOptions = new HttpOptions { Timeout = 45_000 },
        });

        options.Timeout.Should().Be(45_000);
    }

    [Fact]
    public void GoogleAI_BothTimeoutsSet_ThrowsRatherThanPickingAWinner()
    {
        var config = new GoogleAIConfig
        {
            ApiKey = "k",
            Timeout = TimeSpan.FromMinutes(5),
            HttpOptions = new HttpOptions { Timeout = 45_000 },
        };

        var act = () => GoogleAIClientFactory.Create(config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Timeout*HttpOptions.Timeout*");
    }

    [Fact]
    public void VertexAI_BothTimeoutsSet_ThrowsRatherThanPickingAWinner()
    {
        var config = VertexBase();
        config.Timeout = TimeSpan.FromMinutes(5);
        config.HttpOptions = new HttpOptions { Timeout = 45_000 };

        var act = () => GoogleAIClientFactory.Create(config);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GoogleAI_NonPositiveTimeout_Throws(int minutes)
    {
        var config = new GoogleAIConfig { ApiKey = "k", Timeout = TimeSpan.FromMinutes(minutes) };

        var act = () => GoogleAIClientFactory.Create(config);

        act.Should().Throw<InvalidOperationException>();
    }
}

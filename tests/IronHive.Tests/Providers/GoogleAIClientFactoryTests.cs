using AwesomeAssertions;
using Google.GenAI.Types;
using IronHive.Providers.GoogleAI;

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

    [Fact]
    public void GoogleAI_NoTimeoutConfigured_AppliesTheAdapterDefaultRatherThanTheVendorsHundredSeconds()
    {
        var options = Resolve(new GoogleAIConfig { ApiKey = "k" });

        options.Timeout.Should().Be((int)GoogleAIDefaults.Timeout.TotalMilliseconds);
        options.Timeout.Should().NotBe(100_000, "the bare HttpClient default must never be inherited");
    }

    [Fact]
    public void VertexAI_NoTimeoutConfigured_AppliesTheSameDefault()
    {
        var options = Resolve(VertexBase());

        options.Timeout.Should().Be((int)GoogleAIDefaults.Timeout.TotalMilliseconds);
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

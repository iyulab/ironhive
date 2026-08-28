using Anthropic;
using AwesomeAssertions;
using IronHive.Providers.Anthropic;

namespace IronHive.Tests.Providers;

/// <summary>
/// The factory maps <see cref="AnthropicConfig"/> onto the vendor client's options. A field routed
/// to the wrong slot is invisible at compile time and, for credentials, invisible in the resulting
/// error too — so the mapping is asserted rather than assumed.
/// </summary>
public class AnthropicClientFactoryTests
{
    private const string Gateway = "https://gateway.example/v1";

    private static AnthropicClient Create(AnthropicConfig config)
        => (AnthropicClient)AnthropicClientFactory.Create(config);

    [Fact]
    public void Create_BaseUrlOnly_ReachesBaseUrlAndNotTheCredential()
    {
        var client = Create(new AnthropicConfig { BaseUrl = Gateway });

        client.BaseUrl.Should().Be(Gateway);
        client.ApiKey.Should().NotBe(Gateway, "a base URL must never be used as a credential");
    }

    [Fact]
    public void Create_BaseUrlAndApiKey_KeepsThemInSeparateSlots()
    {
        var client = Create(new AnthropicConfig { BaseUrl = Gateway, ApiKey = "sk-test" });

        client.BaseUrl.Should().Be(Gateway);
        client.ApiKey.Should().Be("sk-test");
    }

    [Fact]
    public void Create_AuthTokenWithBaseUrl_LeavesTheTokenPathIntact()
    {
        var client = Create(new AnthropicConfig { BaseUrl = Gateway, AuthToken = "bearer-test" });

        client.BaseUrl.Should().Be(Gateway);
        client.AuthToken.Should().Be("bearer-test");
        client.ApiKey.Should().NotBe(Gateway);
    }

    [Fact]
    public void Create_TimeoutAndMaxRetries_ReachTheClient()
    {
        var client = Create(new AnthropicConfig
        {
            ApiKey = "sk-test",
            Timeout = TimeSpan.FromMinutes(10),
            MaxRetries = 7,
        });

        client.Timeout.Should().Be(TimeSpan.FromMinutes(10));
        client.MaxRetries.Should().Be(7);
    }
}

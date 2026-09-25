using System.Diagnostics;
using AwesomeAssertions;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.GpuStack;

namespace IronHive.Tests.Providers;

/// <summary>
/// A bare <see cref="HttpClient"/> carries a 100-second default timeout, applied ahead of any per-request budget, so it
/// caps time-to-first-byte no matter what <c>OpenAIConfig.Timeout</c> says. Locally hosted servers exceed that while
/// loading a model, and the resulting cancellation names neither the handler nor the configured timeout.
/// <para>
/// Until 0.39.0 the compatible configs built that client themselves and handed it over through
/// <c>OpenAIConfig.HttpClient</c>. They now hand over none (an injected client is never disposed, so theirs leaked), and
/// the client that receives the config creates and owns it — so the guarantee is checked on that client.
/// </para>
/// </summary>
public class InjectedHttpClientTimeoutTests
{
    private static HttpClient OwnedClientFor(IronHive.Providers.OpenAI.OpenAIConfig config)
        => new ProviderHttpClient(config, "chat/completions", defaultBaseUrl: null, sendAccountHeaders: true).Http;

    [Fact]
    public void OpenAICompatible_TheClientThatSends_DoesNotImposeItsOwnRequestTimeout()
    {
        var config = new OpenAICompatibleConfig().ToOpenAI();

        config.HttpClient.Should().BeNull("the config hands over no client of its own");
        OwnedClientFor(config).Timeout.Should().Be(Timeout.InfiniteTimeSpan);
    }

    [Fact]
    public void GpuStack_TheClientThatSends_DoesNotImposeItsOwnRequestTimeout()
    {
        var config = new GpuStackConfig().ToOpenAI();

        config.HttpClient.Should().BeNull("the config hands over no client of its own");
        OwnedClientFor(config).Timeout.Should().Be(Timeout.InfiniteTimeSpan);
    }

    [Fact]
    public async Task OpenAICompatible_ConnectTimeout_StillBoundsAnUnreachableHost()
    {
        // Disabling the request timeout must not disable the connect budget. Without one, a client
        // whose request timeout is infinite would wait indefinitely on a host that never answers.
        // 203.0.113.0/24 is TEST-NET-3 (RFC 5737) — reserved for documentation, so nothing routes
        // there and no external service is contacted.
        var config = new OpenAICompatibleConfig
        {
            BaseUrl = "http://203.0.113.1:9",
            ConnectTimeout = TimeSpan.FromSeconds(2),
        };
        var http = OwnedClientFor(config.ToOpenAI());
        http.Timeout.Should().Be(Timeout.InfiniteTimeSpan);

        var elapsed = Stopwatch.StartNew();
        // The exception type depends on how the host is unreachable — a connect budget expiring
        // surfaces as a cancellation wrapping TimeoutException, no route surfaces as
        // HttpRequestException. The guarantee under test is that one of them arrives promptly
        // rather than the call hanging on the now-infinite request timeout.
        var act = async () => await http.GetAsync(new Uri("http://203.0.113.1:9/"));

        await act.Should().ThrowAsync<Exception>();
        elapsed.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30),
            "the connect budget, not the request timeout, is what bounds an unreachable host");
    }
}

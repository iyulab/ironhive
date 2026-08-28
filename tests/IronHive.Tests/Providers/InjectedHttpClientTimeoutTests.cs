using System.Diagnostics;
using AwesomeAssertions;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.GpuStack;

namespace IronHive.Tests.Providers;

/// <summary>
/// An injected <see cref="HttpClient"/> carries its own 100-second default timeout, which is applied
/// ahead of the SDK's per-read budget and therefore caps time-to-first-byte no matter what
/// <c>OpenAIConfig.TimeOut</c> says. Locally hosted servers exceed that while loading a model, and the
/// resulting cancellation names neither the handler nor the configured timeout. These configs build
/// the client themselves, so the disabling is theirs to guarantee.
/// </summary>
public class InjectedHttpClientTimeoutTests
{
    [Fact]
    public void OpenAICompatible_InjectedClient_DoesNotImposeItsOwnRequestTimeout()
    {
        var http = new OpenAICompatibleConfig().ToOpenAI().HttpClient;

        http.Should().NotBeNull();
        http!.Timeout.Should().Be(Timeout.InfiniteTimeSpan);
    }

    [Fact]
    public void GpuStack_InjectedClient_DoesNotImposeItsOwnRequestTimeout()
    {
        var http = new GpuStackConfig().ToOpenAI().HttpClient;

        http.Should().NotBeNull();
        http!.Timeout.Should().Be(Timeout.InfiniteTimeSpan);
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
        var http = config.ToOpenAI().HttpClient!;
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

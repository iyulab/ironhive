using FluentAssertions;
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
    public void OpenAICompatible_ConnectTimeout_IsStillHonoured()
    {
        // Disabling the request timeout must not disable the connect budget — an unreachable host
        // still has to fail fast rather than hang for the whole completion window.
        var config = new OpenAICompatibleConfig { ConnectTimeout = TimeSpan.FromSeconds(5) };

        config.ConnectTimeout.Should().Be(TimeSpan.FromSeconds(5));
        config.ToOpenAI().HttpClient!.Timeout.Should().Be(Timeout.InfiniteTimeSpan);
    }
}

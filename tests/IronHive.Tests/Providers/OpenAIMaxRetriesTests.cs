using System.Net;
using AwesomeAssertions;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.GpuStack;

namespace IronHive.Tests.Providers;

/// <summary>
/// <see cref="OpenAIConfig.MaxRetries"/> is the number of retries the SDK makes, counted on the wire: a server that
/// keeps answering 503 receives exactly 1 + MaxRetries requests. The OpenAI-compatible and GPUStack configurations
/// pass it through to the configuration they convert to.
/// </summary>
public class OpenAIMaxRetriesTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    public async Task MaxRetries_SetsHowManyRequestsAFailingCallSends(int maxRetries, int expectedRequests)
    {
        var handler = new CountingHandler(HttpStatusCode.ServiceUnavailable);
        using var http = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        using var finder = new OpenAIModelFinder(new OpenAIConfig
        {
            BaseUrl = "https://gateway.example/v1",
            ApiKey = "sk-test",
            HttpClient = http,
            MaxRetries = maxRetries,
        });

        var list = () => finder.ListModelsAsync(TestContext.Current.CancellationToken);

        await list.Should().ThrowAsync<Exception>();
        handler.Requests.Should().Be(expectedRequests);
    }

    [Fact]
    public void MaxRetries_Negative_IsRefused()
    {
        var build = () => OpenAIClientFactory.BuildOptions(new OpenAIConfig { ApiKey = "sk-test", MaxRetries = -1 });

        build.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*MaxRetries*");
    }

    [Fact]
    public void Compatible_PassesMaxRetriesThrough()
    {
        new OpenAICompatibleConfig { MaxRetries = 0 }.ToOpenAI().MaxRetries.Should().Be(0);
        new OpenAICompatibleConfig().ToOpenAI().MaxRetries.Should().BeNull("unset keeps the SDK default");
    }

    [Fact]
    public void GpuStack_PassesMaxRetriesThrough_ToEveryConversion()
    {
        var config = new GpuStackConfig { MaxRetries = 0 };

        config.ToOpenAICompatible().MaxRetries.Should().Be(0);
        config.ToOpenAI().MaxRetries.Should().Be(0);
        config.ToOpenAICompatible().ToOpenAI().MaxRetries.Should().Be(0);
    }

    private sealed class CountingHandler(HttpStatusCode status) : HttpMessageHandler
    {
        private int _requests;

        public int Requests => Volatile.Read(ref _requests);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _requests);
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent("{\"error\":{\"message\":\"down\"}}") });
        }
    }
}

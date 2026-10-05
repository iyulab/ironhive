using AwesomeAssertions;
using IronHive.Abstractions.Models;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;

namespace IronHive.Tests.Providers;

/// <summary>
/// <c>FindModelAsync</c> answers «not found» (null) for a failed lookup. A lookup the caller cancelled is not a failed lookup:
/// it throws <see cref="OperationCanceledException"/>, so a cancelled caller is never told the model does not exist.
/// </summary>
public class ModelFinderCancellationTests
{
    public static TheoryData<string> Finders => new() { "openai", "anthropic", "googleai" };

    [Theory]
    [MemberData(nameof(Finders))]
    public async Task FindModelAsync_ThrowsWhenTheCallerCancels(string provider)
    {
        using var http = new HttpClient(new HangingHandler()) { Timeout = Timeout.InfiniteTimeSpan };
        using IModelFinder finder = provider switch
        {
            "openai" => new OpenAIModelFinder(new OpenAIConfig { BaseUrl = "https://gateway.example/v1", ApiKey = "sk-test", HttpClient = http, MaxRetries = 0 }),
            "anthropic" => new AnthropicModelFinder(new AnthropicConfig { ApiKey = "sk-test", HttpClient = http }),
            _ => new GoogleAIModelFinder(new GoogleAIConfig { ApiKey = "test", HttpClientFactory = () => http }),
        };
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        var find = () => finder.FindModelAsync("some-model", cts.Token);

        await find.Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>Never answers; only the request's cancellation ends it.</summary>
    private sealed class HangingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("unreachable");
        }
    }
}

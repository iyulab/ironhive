using System.Net;
using System.Text;
using AwesomeAssertions;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using OpenAIConfig = IronHive.Providers.OpenAI.OpenAIConfig;

namespace IronHive.Tests.Providers;

/// <summary>
/// <see cref="OpenAIConfig.HttpClient"/> is documented as the slot for a consumer's own client — one from
/// <c>IHttpClientFactory</c>, shared with other callers. Until 0.39.0 the Chat Completions path treated it as its
/// own: it set <c>BaseAddress</c>, <c>Timeout</c> and default headers on it (which throws once the client has sent
/// anything) and disposed it with the generator. An injected client is now used as given and never disposed;
/// the endpoint, credentials and <see cref="OpenAIConfig.Timeout"/> travel with each request.
/// </summary>
public class ChatCompletionInjectedHttpClientTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private const string CompletionJson =
        """{"id":"x","object":"chat.completion","created":0,"model":"m","choices":[{"index":0,"message":{"role":"assistant","content":"hi"},"finish_reason":"stop"}]}""";

    private sealed class RecordingHandler(TimeSpan delay = default) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(CompletionJson, Encoding.UTF8, "application/json"),
            };
        }
    }

    private static ChatCompletionRequest Request() => new() { Model = "m", Messages = [] };

    [Fact]
    public async Task AClientThatHasAlreadySentRequests_CanBeInjected_AndItsSettingsAreLeftAlone()
    {
        var handler = new RecordingHandler();
        var shared = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
        await shared.GetAsync("http://other.invalid/ping", Ct);

        using var client = new ChatCompletionHttpClient(new OpenAIConfig
        {
            HttpClient = shared,
            BaseUrl = "http://a.invalid/v1",
            ApiKey = "sk-test",
            Organization = "org-1",
            Project = "proj-1",
        });
        await client.PostAsync(Request(), Ct);

        shared.Timeout.Should().Be(TimeSpan.FromSeconds(30), "the consumer's timeout is the consumer's");
        shared.BaseAddress.Should().BeNull();
        shared.DefaultRequestHeaders.Should().BeEmpty();

        var sent = handler.Requests[^1];
        sent.RequestUri.Should().Be(new Uri("http://a.invalid/v1/chat/completions"));
        sent.Headers.Authorization!.Scheme.Should().Be("Bearer");
        sent.Headers.Authorization.Parameter.Should().Be("sk-test");
        sent.Headers.GetValues("OpenAI-Organization").Should().Equal("org-1");
        sent.Headers.GetValues("OpenAI-Project").Should().Equal("proj-1");
    }

    [Fact]
    public async Task DisposingTheGenerator_LeavesAnInjectedClientUsable()
    {
        var handler = new RecordingHandler();
        var shared = new HttpClient(handler);

        new ChatCompletionMessageGenerator(new OpenAIConfig { HttpClient = shared, BaseUrl = "http://a.invalid/v1" }).Dispose();

        var act = () => shared.GetAsync("http://other.invalid/ping", Ct);
        await act.Should().NotThrowAsync("an injected client belongs to whoever injected it");
    }

    [Fact]
    public async Task TwoGeneratorsCanShareOneInjectedClient()
    {
        // The IHttpClientFactory case: the same client handed to generators for two endpoints.
        var handler = new RecordingHandler();
        var shared = new HttpClient(handler);
        using var a = new ChatCompletionHttpClient(new OpenAIConfig { HttpClient = shared, BaseUrl = "http://a.invalid/v1" });
        using var b = new ChatCompletionHttpClient(new OpenAIConfig { HttpClient = shared, BaseUrl = "http://b.invalid/v1/" });

        await a.PostAsync(Request(), Ct);
        await b.PostAsync(Request(), Ct);

        handler.Requests.Select(r => r.RequestUri!.Host).Should().Equal("a.invalid", "b.invalid");
    }

    [Fact]
    public async Task ConfigTimeout_StillBoundsARequest_OnAnInjectedClient()
    {
        var shared = new HttpClient(new RecordingHandler(delay: TimeSpan.FromSeconds(10))) { Timeout = Timeout.InfiniteTimeSpan };
        using var client = new ChatCompletionHttpClient(new OpenAIConfig
        {
            HttpClient = shared,
            BaseUrl = "http://a.invalid/v1",
            Timeout = TimeSpan.FromMilliseconds(200),
        });

        var act = () => client.PostAsync(Request(), Ct);

        await act.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public void CompatibleConfigs_HandOverNoClientOfTheirOwn_SoTheReceivingClientOwnsAndDisposesIt()
    {
        // Before 0.39.0 ToOpenAI() put a fresh HttpClient into the injection slot — harmless while every client
        // disposed whatever it held, a leak per generator once an injected client is (rightly) never disposed.
        var compatible = new IronHive.Providers.OpenAI.Compatible.OpenAICompatibleConfig
        {
            BaseUrl = "http://localhost:11434",
            ConnectTimeout = TimeSpan.FromSeconds(3),
        }.ToOpenAI();
        compatible.HttpClient.Should().BeNull();
        compatible.ConnectTimeout.Should().Be(TimeSpan.FromSeconds(3));

        var gpuStack = new IronHive.Providers.OpenAI.Compatible.GpuStack.GpuStackConfig
        {
            BaseUrl = "http://localhost:8080",
            ConnectTimeout = TimeSpan.FromSeconds(4),
        }.ToOpenAI();
        gpuStack.HttpClient.Should().BeNull();
        gpuStack.ConnectTimeout.Should().Be(TimeSpan.FromSeconds(4));
    }

    [Fact]
    public async Task RerankClient_FollowsTheSameRule()
    {
        const string rerankJson = """{"results":[{"index":0,"relevance_score":0.9}]}""";
        var handler = new StubHandler(rerankJson);
        var shared = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
        await shared.GetAsync("http://other.invalid/ping", Ct);

        var reranker = new IronHive.Providers.OpenAI.Compatible.Reranking.CohereDocumentReranker(new OpenAIConfig
        {
            HttpClient = shared,
            BaseUrl = "http://r.invalid/v1",
            ApiKey = "k",
        });
        var results = await reranker.RerankAsync("m", "q", ["doc"], cancellationToken: Ct);
        reranker.Dispose();

        results.Should().ContainSingle();
        handler.Requests[^1].RequestUri.Should().Be(new Uri("http://r.invalid/v1/rerank"));
        handler.Requests[^1].Headers.Authorization!.Parameter.Should().Be("k");
        shared.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        var act = () => shared.GetAsync("http://other.invalid/ping", Ct);
        await act.Should().NotThrowAsync();
    }

    private sealed class StubHandler(string json) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });
        }
    }
}

using IronHive.Abstractions.Http;
using IronHive.Abstractions.Messages;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;

namespace IronHive.Tests.Conventions.Providers;

/// <summary>
/// One <c>Headers</c> slot on every provider config, with one precedence rule enforced for all of
/// them. Wire-level: the stub handler records what each vendor SDK actually sent, so these facts see
/// the request after the SDK's own pipeline ran - which is where a header applied at the wrong
/// position silently loses to the credential policy.
/// </summary>
public class ProviderRequestHeadersTests
{
    private const string Gateway = "X-Gateway-Key";

    // ── the shared rules ──────────────────────────────────────────────────────

    [Fact]
    public void Resolve_MergesSources_CaseInsensitively_AndReturnsNullForNothing()
    {
        Assert.Null(ProviderRequestHeaders.Resolve("Cfg", "ApiKey", ["Authorization"], null, new Dictionary<string, string>()));

        var merged = ProviderRequestHeaders.Resolve("Cfg", "ApiKey", ["Authorization"],
            new Dictionary<string, string> { ["x-a"] = "1" },
            new Dictionary<string, string> { ["X-A"] = "1", ["x-b"] = "2" });

        Assert.NotNull(merged);
        Assert.Equal(2, merged.Count);
        Assert.Equal("1", merged["X-A"]);
    }

    [Theory]
    [InlineData("Authorization")]
    [InlineData("authorization")]
    [InlineData("X-API-KEY")]
    public void Resolve_RefusesTheCredentialHeader_NamingTheSlot(string name)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ProviderRequestHeaders.Resolve("AnthropicConfig", "ApiKey", ["Authorization", "x-api-key"],
                new Dictionary<string, string> { [name] = "v" }));

        Assert.Contains(name, ex.Message);
        Assert.Contains("AnthropicConfig.ApiKey", ex.Message);
    }

    [Fact]
    public void Resolve_RefusesTheSameHeaderWithTwoValues()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ProviderRequestHeaders.Resolve("Cfg", "ApiKey", [],
                new Dictionary<string, string> { ["x-a"] = "1" },
                new Dictionary<string, string> { ["X-A"] = "2" }));

        Assert.Contains("x-a", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── OpenAI (Responses SDK path) ───────────────────────────────────────────

    [Fact]
    public async Task OpenAI_SendsTheConfiguredHeader_AndKeepsTheCredential_AfterTheSdkPipeline()
    {
        var handler = new StubHttpHandler("{}", "");
        using var generator = new OpenAIMessageGenerator(new OpenAIConfig
        {
            ApiKey = "test-key",
            HttpClient = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan },
            // User-Agent is a header the SDK sets itself: a configured value arriving on the wire
            // proves the policy runs after the SDK's own, not before it (the PerCall trap).
            Headers = new Dictionary<string, string> { [Gateway] = "g1", ["User-Agent"] = "gateway-ua" },
        });

        await SendIgnoringResponseAsync(() => generator.GenerateMessageAsync(Request(), TestContext.Current.CancellationToken));

        var sent = Assert.Single(handler.Headers);
        Assert.Equal("g1", sent[Gateway]);
        Assert.Equal("Bearer test-key", sent["Authorization"]);
        Assert.Equal("gateway-ua", sent["User-Agent"]);
    }

    [Fact]
    public void OpenAI_RefusesAuthorizationInHeaders_AtConstruction()
    {
        var ex = Assert.Throws<ArgumentException>(() => new OpenAIMessageGenerator(new OpenAIConfig
        {
            ApiKey = "test-key",
            Headers = new Dictionary<string, string> { ["Authorization"] = "Bearer other" },
        }));
        Assert.Contains("OpenAIConfig.ApiKey", ex.Message);
    }

    // ── OpenAI-compatible (chat completions, library-owned client) ────────────

    [Fact]
    public async Task ChatCompletions_SendsTheConfiguredHeader_PerRequest_AndKeepsTheCredential()
    {
        var handler = new StubHttpHandler("{}", "");
        var config = new OpenAICompatibleConfig
        {
            BaseUrl = "https://compatible.invalid",
            ApiKey = "test-key",
            Headers = new Dictionary<string, string> { [Gateway] = "g1" },
        }.ToOpenAI();
        config.HttpClient = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        using var generator = new ChatCompletionMessageGenerator(config);

        await SendIgnoringResponseAsync(() => generator.GenerateMessageAsync(Request(), TestContext.Current.CancellationToken));

        var sent = Assert.Single(handler.Headers);
        Assert.Equal("g1", sent[Gateway]);
        Assert.Equal("Bearer test-key", sent["Authorization"]);
        // The consumer's client was not mutated: the header lives on the request, not on DefaultRequestHeaders.
        Assert.False(config.HttpClient.DefaultRequestHeaders.Contains(Gateway));
    }

    [Fact]
    public void ChatCompletions_RefusesAuthorizationInHeaders_AtConstruction()
    {
        Assert.Throws<ArgumentException>(() => new ChatCompletionMessageGenerator(new OpenAIConfig
        {
            BaseUrl = "https://compatible.invalid/v1",
            Headers = new Dictionary<string, string> { ["authorization"] = "x" },
        }));
    }

    // ── Anthropic ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Anthropic_SendsTheConfiguredHeader_UnionedWithExtraHeaders_AndKeepsTheCredential()
    {
        var handler = new StubHttpHandler("{}", "");
        using var generator = new AnthropicMessageGenerator(new AnthropicConfig
        {
            ApiKey = "test-key",
            HttpClient = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan },
            ExtraHeaders = new Dictionary<string, string> { ["anthropic-workspace-id"] = "ws" },
            Headers = new Dictionary<string, string> { [Gateway] = "g1" },
        });

        await SendIgnoringResponseAsync(() => generator.GenerateMessageAsync(Request(maxTokens: 16), TestContext.Current.CancellationToken));

        var sent = Assert.Single(handler.Headers);
        Assert.Equal("g1", sent[Gateway]);
        Assert.Equal("ws", sent["anthropic-workspace-id"]);
        Assert.Equal("test-key", sent["x-api-key"]);
    }

    [Fact]
    public void Anthropic_RefusesTheCredentialHeader_AndDisagreeingSlots_AtConstruction()
    {
        Assert.Throws<ArgumentException>(() => new AnthropicMessageGenerator(new AnthropicConfig
        {
            ApiKey = "test-key",
            Headers = new Dictionary<string, string> { ["X-Api-Key"] = "other" },
        }));
        Assert.Throws<ArgumentException>(() => new AnthropicMessageGenerator(new AnthropicConfig
        {
            ApiKey = "test-key",
            ExtraHeaders = new Dictionary<string, string> { [Gateway] = "a" },
            Headers = new Dictionary<string, string> { [Gateway] = "b" },
        }));
    }

    // ── Google AI ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GoogleAI_SendsTheConfiguredHeader_MergedIntoHttpOptions_AndKeepsTheCredential()
    {
        var handler = new StubHttpHandler("{}", "");
        var httpClient = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        using var generator = new GoogleAIMessageGenerator(new GoogleAIConfig
        {
            ApiKey = "test-key",
            HttpClientFactory = () => httpClient,
            HttpOptions = new Google.GenAI.Types.HttpOptions { Headers = new Dictionary<string, string> { ["x-vendor-slot"] = "v" } },
            Headers = new Dictionary<string, string> { [Gateway] = "g1" },
        });

        await SendIgnoringResponseAsync(() => generator.GenerateMessageAsync(Request(), TestContext.Current.CancellationToken));

        var sent = Assert.Single(handler.Headers);
        Assert.Equal("g1", sent[Gateway]);
        Assert.Equal("v", sent["x-vendor-slot"]);
        Assert.Equal("test-key", sent["x-goog-api-key"]);
    }

    [Fact]
    public void GoogleAI_RefusesTheCredentialHeader_AtConstruction()
    {
        var ex = Assert.Throws<ArgumentException>(() => new GoogleAIMessageGenerator(new GoogleAIConfig
        {
            ApiKey = "test-key",
            Headers = new Dictionary<string, string> { ["x-goog-api-key"] = "other" },
        }));
        Assert.Contains("GoogleAIConfig.ApiKey", ex.Message);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static MessageGenerationRequest Request(int? maxTokens = null) => new()
    {
        Model = "test-model",
        MaxTokens = maxTokens,
        Messages = [Message.User("Hi")],
    };

    /// <summary>
    /// The stub answers with an empty body, so the SDK's response parsing fails after the request has
    /// gone out. What these facts look at is the request the handler recorded; a request that was never
    /// sent shows up as an empty <c>Headers</c> list, which the assertions above refuse.
    /// </summary>
    private static async Task SendIgnoringResponseAsync(Func<Task> send)
    {
        try { await send(); }
        catch (Exception) { }
    }
}

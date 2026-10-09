using IronHive.Abstractions.Messages;
using IronHive.Extensions.AI;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using Microsoft.Extensions.AI;
using Message = IronHive.Abstractions.Messages.Message;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace IronHive.Tests.Conventions.Providers;

/// <summary>
/// <see cref="MessageGenerationRequest.Headers"/> — one request's own headers, for what a gateway reads per call when
/// one client serves many callers. Every provider must put them on the wire, buffered and streaming, on top of the
/// configured headers (the request value wins for a shared name), and refuse the credential header as the configured
/// slot does. Wire-level: the stub handler records what each vendor SDK actually sent.
/// </summary>
public class RequestHeadersWireTests
{
    private const string Tag = "X-Gateway-Tag";
    private const string Keep = "X-Gateway-App";

    public static TheoryData<string, bool> Providers()
    {
        var data = new TheoryData<string, bool>();
        foreach (var provider in new[] { "openai", "compatible", "anthropic", "google" })
        {
            data.Add(provider, false);
            data.Add(provider, true);
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Request_headers_reach_the_wire_over_the_configured_ones(string provider, bool streaming)
    {
        var handler = new StubHttpHandler("{}", "");
        using var generator = Create(provider, handler);

        await SendAsync(generator, Request(new Dictionary<string, string> { [Tag] = "unit-7" }), streaming);

        var sent = Assert.Single(handler.Headers);
        Assert.Equal("unit-7", sent[Tag]);
        Assert.Equal("app", sent[Keep]);
        AssertCredential(provider, sent);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_request_without_headers_sends_only_the_configured_ones(string provider, bool streaming)
    {
        // Positive control for the scope: the first call's value must not outlive it (the OpenAI path carries it on
        // the async flow).
        var handler = new StubHttpHandler("{}", "");
        using var generator = Create(provider, handler);

        await SendAsync(generator, Request(new Dictionary<string, string> { [Tag] = "unit-7" }), streaming);
        await SendAsync(generator, Request(headers: null), streaming);

        Assert.Equal(2, handler.Headers.Count);
        Assert.Equal("unit-7", handler.Headers[0][Tag]);
        Assert.Equal("config", handler.Headers[1][Tag]);
    }

    [Theory]
    [InlineData("openai", "Authorization")]
    [InlineData("compatible", "authorization")]
    [InlineData("anthropic", "x-api-key")]
    [InlineData("google", "x-goog-api-key")]
    public async Task A_request_header_naming_the_credential_is_refused(string provider, string credentialHeader)
    {
        var handler = new StubHttpHandler("{}", "");
        using var generator = Create(provider, handler);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => generator.GenerateMessageAsync(
            Request(new Dictionary<string, string> { [credentialHeader] = "other" }), TestContext.Current.CancellationToken));

        Assert.Contains(credentialHeader, ex.Message);
        Assert.Empty(handler.Headers);
    }

    [Fact]
    public async Task ChatOptions_key_carries_request_headers_through_the_MEAI_adapter()
    {
        var handler = new StubHttpHandler("{}", "");
        using var generator = Create("compatible", handler);
        using var client = new ChatClientAdapter(generator, "test-model");
        var options = new ChatOptions
        {
            AdditionalProperties = new AdditionalPropertiesDictionary
            {
                [ChatClientAdapter.RequestHeadersKey] = new Dictionary<string, string> { [Tag] = "unit-9" },
            },
        };

        await IgnoreResponse(() => client.GetResponseAsync([new ChatMessage(ChatRole.User, "Hi")], options, TestContext.Current.CancellationToken));

        Assert.Equal("unit-9", Assert.Single(handler.Headers)[Tag]);
    }

    [Fact]
    public async Task ChatOptions_key_with_another_value_type_is_refused()
    {
        var handler = new StubHttpHandler("{}", "");
        using var generator = Create("compatible", handler);
        using var client = new ChatClientAdapter(generator, "test-model");
        var options = new ChatOptions
        {
            AdditionalProperties = new AdditionalPropertiesDictionary { [ChatClientAdapter.RequestHeadersKey] = "X-Gateway-Tag: unit-9" },
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.GetResponseAsync([new ChatMessage(ChatRole.User, "Hi")], options, TestContext.Current.CancellationToken));
        Assert.Empty(handler.Headers);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static readonly Dictionary<string, string> Configured = new() { [Tag] = "config", [Keep] = "app" };

    private static IMessageGenerator Create(string provider, StubHttpHandler handler)
    {
        var http = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        return provider switch
        {
            "openai" => new OpenAIMessageGenerator(new OpenAIConfig { ApiKey = "test-key", HttpClient = http, Headers = Configured }),
            "compatible" => CreateCompatible(http),
            "anthropic" => new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key", HttpClient = http, Headers = Configured }),
            "google" => new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key", HttpClientFactory = () => http, Headers = Configured }),
            _ => throw new ArgumentOutOfRangeException(nameof(provider)),
        };
    }

    private static ChatCompletionMessageGenerator CreateCompatible(HttpClient http)
    {
        var config = new OpenAICompatibleConfig
        {
            BaseUrl = "https://compatible.invalid",
            ApiKey = "test-key",
            Headers = Configured,
        }.ToOpenAI();
        config.HttpClient = http;
        return new ChatCompletionMessageGenerator(config);
    }

    private static void AssertCredential(string provider, Dictionary<string, string> sent)
    {
        switch (provider)
        {
            case "openai" or "compatible":
                Assert.Equal("Bearer test-key", sent["Authorization"]);
                break;
            case "anthropic":
                Assert.Equal("test-key", sent["x-api-key"]);
                break;
            case "google":
                Assert.Equal("test-key", sent["x-goog-api-key"]);
                break;
        }
    }

    private static MessageGenerationRequest Request(IDictionary<string, string>? headers) => new()
    {
        Model = "test-model",
        MaxTokens = 16,
        Messages = [Message.User("Hi")],
        Headers = headers,
    };

    private static Task SendAsync(IMessageGenerator generator, MessageGenerationRequest request, bool streaming)
        => IgnoreResponse(async () =>
        {
            if (!streaming)
            {
                await generator.GenerateMessageAsync(request, TestContext.Current.CancellationToken);
                return;
            }
            await foreach (var _ in generator.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken))
            { }
        });

    /// <summary>The stub answers with an empty body, so parsing fails after the request went out — what these facts read
    /// is the request the handler recorded.</summary>
    private static async Task IgnoreResponse(Func<Task> send)
    {
        try { await send(); }
        catch (Exception ex) when (ex is not ArgumentException) { }
    }
}

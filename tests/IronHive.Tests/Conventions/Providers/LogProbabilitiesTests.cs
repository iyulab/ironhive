using System.Text.Json.Nodes;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;

namespace IronHive.Tests.Conventions.Providers;

/// <summary>
/// Token log probabilities on the OpenAI-compatible generator: the request asks with <c>logprobs</c>/<c>top_logprobs</c>,
/// the buffered response and each streamed text delta carry them, and the done frame carries the whole list — the same
/// list the buffered call returns. A judge that reads one token's distribution is the case in point.
/// </summary>
public class LogProbabilitiesTests
{
    private const string Model = "local-model";
    private const string Usage = """{"prompt_tokens":11,"completion_tokens":2,"total_tokens":13}""";

    private static string TokenJson(string token, double logprob, params (string Token, double LogProb)[] top)
        => $$"""{"token":"{{token}}","logprob":{{logprob.ToString(System.Globalization.CultureInfo.InvariantCulture)}},"bytes":null,"top_logprobs":[{{string.Join(",", top.Select(t => $$"""{"token":"{{t.Token}}","logprob":{{t.LogProb.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}"""))}}]}""";

    [Fact]
    public async Task Request_AsksForLogprobs_OnlyWhenTheCallerDoes()
    {
        var json = Buffered(content: "yes", logprobs: null);
        var (generator, handler) = Create(json, sse: "");
        using var _g = generator;

        await generator.GenerateMessageAsync(Request(), TestContext.Current.CancellationToken);
        await generator.GenerateMessageAsync(Request(new LogProbabilityOptions { TopAlternatives = 0 }), TestContext.Current.CancellationToken);
        await generator.GenerateMessageAsync(Request(new LogProbabilityOptions { TopAlternatives = 20 }), TestContext.Current.CancellationToken);

        var bodies = handler.Requests.Select(r => JsonNode.Parse(r.Body)!.AsObject()).ToList();
        bodies[0].ContainsKey("logprobs").Should().BeFalse();
        bodies[0].ContainsKey("top_logprobs").Should().BeFalse();
        bodies[1]["logprobs"]!.GetValue<bool>().Should().BeTrue();
        bodies[1].ContainsKey("top_logprobs").Should().BeFalse("0 alternatives is the chosen token only");
        bodies[2]["top_logprobs"]!.GetValue<int>().Should().Be(20);
    }

    [Fact]
    public async Task Buffered_ChoiceLogprobs_ReachTheResponse()
    {
        var json = Buffered("yes", $$"""{"content":[{{TokenJson("yes", -0.1, ("yes", -0.1), ("no", -2.4))}}]}""");
        var (generator, _) = Create(json, sse: "");
        using var _g = generator;

        var response = await generator.GenerateMessageAsync(Request(new LogProbabilityOptions { TopAlternatives = 2 }), TestContext.Current.CancellationToken);

        var token = response.LogProbabilities!.Single();
        token.Token.Should().Be("yes");
        token.LogProbability.Should().Be(-0.1);
        token.Alternatives.Select(a => (a.Token, a.LogProbability)).Should().Equal(("yes", -0.1), ("no", -2.4));
    }

    [Fact]
    public async Task Buffered_NotRequested_IsNull_AndRequestedButAbsent_IsEmpty()
    {
        var (generator, _) = Create(Buffered("yes", logprobs: null), sse: "");
        using var _g = generator;

        (await generator.GenerateMessageAsync(Request(), TestContext.Current.CancellationToken)).LogProbabilities.Should().BeNull();
        (await generator.GenerateMessageAsync(Request(new LogProbabilityOptions()), TestContext.Current.CancellationToken)).LogProbabilities
            .Should().BeEmpty("a server that does not implement logprobs answers without them; asked-for and none is an empty list");
    }

    [Fact]
    public async Task Streaming_EachDeltaCarriesItsTokens_AndTheDoneFrameAgreesWithBuffered()
    {
        var sse = StubHttpHandler.SseData(
            Chunk("""{"role":"assistant","content":"ye"}""", $$"""{"content":[{{TokenJson("ye", -0.2, ("ye", -0.2))}}]}""", null),
            Chunk("""{"content":"s"}""", $$"""{"content":[{{TokenJson("s", -0.3, ("s", -0.3))}}]}""", null),
            Chunk("""{}""", "null", "stop"),
            "[DONE]");
        var buffered = Buffered("yes", $$"""{"content":[{{TokenJson("ye", -0.2, ("ye", -0.2))}},{{TokenJson("s", -0.3, ("s", -0.3))}}]}""");
        var (generator, _) = Create(buffered, sse);
        using var _g = generator;
        var request = Request(new LogProbabilityOptions { TopAlternatives = 1 });

        var bufferedResponse = await generator.GenerateMessageAsync(request, TestContext.Current.CancellationToken);
        var frames = new List<StreamingMessageResponse>();
        await foreach (var frame in generator.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken))
            frames.Add(frame);

        var deltas = frames.OfType<StreamingContentDeltaResponse>().Where(f => f.Delta is TextDeltaContent).ToList();
        deltas.Select(d => d.LogProbabilities!.Single().Token).Should().Equal("ye", "s");
        var done = frames.OfType<StreamingMessageDoneResponse>().Single();
        done.LogProbabilities.Should().BeEquivalentTo(bufferedResponse.LogProbabilities, o => o.WithStrictOrdering());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(21)]
    public void TopAlternatives_OutsideWhatProvidersReturn_IsRefused(int value)
    {
        var act = () => new LogProbabilityOptions { TopAlternatives = value };
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("OpenAI")]
    [InlineData("Anthropic")]
    [InlineData("GoogleAI")]
    public async Task SdkProviders_RefuseLogprobs_InsteadOfAnsweringWithout(string provider)
    {
        using IMessageGenerator generator = provider switch
        {
            "OpenAI" => new OpenAIMessageGenerator(new OpenAIConfig { ApiKey = "test-key" }),
            "Anthropic" => new IronHive.Providers.Anthropic.AnthropicMessageGenerator(new IronHive.Providers.Anthropic.AnthropicConfig { ApiKey = "test-key" }),
            _ => new IronHive.Providers.GoogleAI.GoogleAIMessageGenerator(new IronHive.Providers.GoogleAI.GoogleAIConfig { ApiKey = "test-key" }),
        };

        var act = () => generator.GenerateMessageAsync(Request(new LogProbabilityOptions()), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotSupportedException>().WithMessage("*LogProbabilities*");
    }

    private static MessageGenerationRequest Request(LogProbabilityOptions? logProbabilities = null)
        => new() { Model = Model, Messages = [Message.User("Hi")], LogProbabilities = logProbabilities };

    private static string Buffered(string content, string? logprobs)
        => $$$"""{"id":"c1","object":"chat.completion","created":1,"model":"{{{Model}}}","choices":[{"index":0,"message":{"role":"assistant","content":"{{{content}}}"},"finish_reason":"stop","logprobs":{{{logprobs ?? "null"}}}}],"usage":{{{Usage}}}}""";

    private static string Chunk(string delta, string logprobs, string? finishReason)
        => $$$"""{"id":"c1","object":"chat.completion.chunk","created":1,"model":"{{{Model}}}","choices":[{"index":0,"delta":{{{delta}}},"finish_reason":{{{(finishReason is null ? "null" : $"\"{finishReason}\"")}}},"logprobs":{{{logprobs}}}}]}""";

    private static (ChatCompletionMessageGenerator Generator, StubHttpHandler Handler) Create(string json, string sse)
    {
        var handler = new StubHttpHandler(json, sse);
        var generator = new ChatCompletionMessageGenerator(new OpenAIConfig
        {
            ApiKey = "test-key",
            BaseUrl = "https://compatible.invalid/v1",
            HttpClient = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan },
        });
        return (generator, handler);
    }
}

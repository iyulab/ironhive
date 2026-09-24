using System.Text.Json.Nodes;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;

namespace IronHive.Tests.Conventions.Providers;

/// <summary>
/// The provider-specific passthrough on the OpenAI-compatible generator: response fields no typed member maps reach
/// <see cref="MessageResponse.ExtraBody"/> and the done frame (llama.cpp's <c>timings</c> is the case in point), and the
/// caller's <see cref="MessageGenerationRequest.ExtraBody"/> is merged into the request body.
/// </summary>
public class ExtraBodyPassthroughTests
{
    private const string Model = "local-model";
    private const string Usage = """{"prompt_tokens":11,"completion_tokens":2,"total_tokens":13}""";
    private const string Timings = """{"prompt_n":11,"prompt_ms":12.5,"predicted_n":2,"predicted_ms":7.25}""";

    [Fact]
    public async Task Buffered_ResponseTimings_ReachExtraBody_WithoutTheChoices()
    {
        var json = $$$"""{"id":"c1","object":"chat.completion","created":1,"model":"{{{Model}}}","choices":[{"index":0,"message":{"role":"assistant","content":"yes","reasoning_content":"r"},"finish_reason":"stop"}],"usage":{{{Usage}}},"timings":{{{Timings}}},"system_fingerprint":"b1234"}""";
        var (generator, _) = Create(json, sse: "");
        using var _g = generator;

        var response = await generator.GenerateMessageAsync(Request(), TestContext.Current.CancellationToken);

        response.ExtraBody.Should().NotBeNull();
        response.ExtraBody!["timings"]!["predicted_ms"]!.GetValue<double>().Should().Be(7.25);
        response.ExtraBody.ContainsKey("choices").Should().BeFalse("per-choice fields are content, not response metadata");
    }

    [Fact]
    public async Task Streaming_TimingsOnTheLastChunk_ReachTheDoneFrame()
    {
        var sse = StubHttpHandler.SseData(
            $$$"""{"id":"c1","object":"chat.completion.chunk","created":1,"model":"{{{Model}}}","choices":[{"index":0,"delta":{"role":"assistant","content":"yes","reasoning_content":"r"},"finish_reason":null}]}""",
            $$$"""{"id":"c1","object":"chat.completion.chunk","created":1,"model":"{{{Model}}}","choices":[{"index":0,"delta":{},"finish_reason":"stop"}],"usage":{{{Usage}}},"timings":{{{Timings}}}}""",
            "[DONE]");
        var (generator, _) = Create(json: "{}", sse);
        using var _g = generator;

        var frames = new List<StreamingMessageResponse>();
        await foreach (var frame in generator.GenerateStreamingMessageAsync(Request(), TestContext.Current.CancellationToken))
            frames.Add(frame);

        var done = frames.OfType<StreamingMessageDoneResponse>().Single();
        done.ExtraBody!["timings"]!["prompt_ms"]!.GetValue<double>().Should().Be(12.5);
        done.ExtraBody.ContainsKey("choices").Should().BeFalse("per-token deltas stay in the content stream");
    }

    [Fact]
    public async Task Response_WithoutUnmappedFields_HasNoExtraBody()
    {
        var json = $$$"""{"id":"c1","object":"chat.completion","created":1,"model":"{{{Model}}}","choices":[{"index":0,"message":{"role":"assistant","content":"yes"},"finish_reason":"stop"}],"usage":{{{Usage}}}}""";
        var (generator, _) = Create(json, sse: "");
        using var _g = generator;

        var response = await generator.GenerateMessageAsync(Request(), TestContext.Current.CancellationToken);

        response.ExtraBody.Should().BeNull();
    }

    [Fact]
    public async Task RequestExtraBody_IsMergedIntoTheWireBody_OverThisLibrarysFields()
    {
        var json = $$$"""{"id":"c1","object":"chat.completion","created":1,"model":"{{{Model}}}","choices":[{"index":0,"message":{"role":"assistant","content":"yes"},"finish_reason":"stop"}],"usage":{{{Usage}}}}""";
        var (generator, handler) = Create(json, sse: "");
        using var _g = generator;

        var request = Request();
        request.ThinkingEffort = MessageThinkingEffort.Low;
        request.ExtraBody = new JsonObject
        {
            ["n_probs"] = 20,
            ["chat_template_kwargs"] = new JsonObject { ["enable_thinking"] = false },
        };

        await generator.GenerateMessageAsync(request, TestContext.Current.CancellationToken);

        var body = JsonNode.Parse(handler.Requests.Single().Body)!.AsObject();
        body["n_probs"]!.GetValue<int>().Should().Be(20);
        body["chat_template_kwargs"]!["enable_thinking"]!.GetValue<bool>().Should().BeFalse("the caller's value wins");
        body["chat_template_kwargs"]!["thinking"]!.GetValue<bool>().Should().BeTrue("fields the caller did not name are kept");
        body["thinking_token_budget"]!.GetValue<int>().Should().Be(512);
        request.ExtraBody!["chat_template_kwargs"]!["enable_thinking"]!.GetValue<bool>().Should().BeFalse("the caller's object is not modified");
    }

    [Theory]
    [InlineData("OpenAI")]
    [InlineData("Anthropic")]
    [InlineData("GoogleAI")]
    public async Task SdkProviders_RejectExtraBody_InsteadOfDroppingIt(string provider)
    {
        using IMessageGenerator generator = provider switch
        {
            "OpenAI" => new OpenAIMessageGenerator(new OpenAIConfig { ApiKey = "test-key" }),
            "Anthropic" => new IronHive.Providers.Anthropic.AnthropicMessageGenerator(new IronHive.Providers.Anthropic.AnthropicConfig { ApiKey = "test-key" }),
            _ => new IronHive.Providers.GoogleAI.GoogleAIMessageGenerator(new IronHive.Providers.GoogleAI.GoogleAIConfig { ApiKey = "test-key" }),
        };
        var request = Request();
        request.ExtraBody = new JsonObject { ["x"] = 1 };

        var buffered = () => generator.GenerateMessageAsync(request, TestContext.Current.CancellationToken);
        await buffered.Should().ThrowAsync<NotSupportedException>().WithMessage("*ExtraBody*");

        var streaming = async () =>
        {
            await foreach (var _ in generator.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken)) { }
        };
        await streaming.Should().ThrowAsync<NotSupportedException>().WithMessage("*ExtraBody*");
    }

    private static MessageGenerationRequest Request() => new() { Model = Model, Messages = [Message.User("Hi")] };

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

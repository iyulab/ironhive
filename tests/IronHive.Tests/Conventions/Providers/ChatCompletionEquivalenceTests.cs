using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;

namespace IronHive.Tests.Conventions.Providers;

// docs/CONVENTIONS.md section 5 at the provider layer, for the Chat Completions generator — the raw
// HTTP/JSON client behind every OpenAI-compatible server (OpenAICompatibleMessageGenerator delegates
// both halves to it, so this covers that generator too). Same harness, same definition of "agree".
//
// Chat Completions streams data-only SSE chunks that end with `data: [DONE]`; a usage chunk with an
// empty choices array arrives last when the caller asked for it.
public class ChatCompletionEquivalenceTests
{
    private const string Model = "gpt-4o-mini";
    private const long Created = 1757635200;

    [Fact]
    public async Task StopText_BothHalvesCarryTheSameResult()
    {
        var response = ResponseJson("""{"role":"assistant","content":"Hello world"}""", "stop");
        var sse = StubHttpHandler.SseData(
            Chunk("""{"role":"assistant","content":"Hello "}""", finishReason: null),
            Chunk("""{"content":"world"}""", finishReason: null),
            Chunk("""{}""", finishReason: "stop"),
            UsageChunk(),
            "[DONE]");

        var (buffered, frames) = await RunBothAsync(response, sse);

        buffered.DoneReason.Should().Be(MessageDoneReason.EndTurn);
        AssertEnvelopesAgree(buffered, frames);
        TextOf(frames).Should().Be(TextOf(buffered.Message), "the streamed deltas must add up to what the buffered call returns");
    }

    [Fact]
    public async Task Length_BothHalvesCarryTheSameResult()
    {
        var response = ResponseJson("""{"role":"assistant","content":"Hello"}""", "length");
        var sse = StubHttpHandler.SseData(
            Chunk("""{"role":"assistant","content":"Hello"}""", finishReason: null),
            Chunk("""{}""", finishReason: "length"),
            UsageChunk(),
            "[DONE]");

        var (buffered, frames) = await RunBothAsync(response, sse);

        buffered.DoneReason.Should().Be(MessageDoneReason.MaxTokens, "the fixture must exercise the length finish reason");
        AssertEnvelopesAgree(buffered, frames);
        TextOf(frames).Should().Be(TextOf(buffered.Message));
    }

    [Fact]
    public async Task ToolCalls_BothHalvesCarryTheSameToolCall()
    {
        var response = ResponseJson("""{"role":"assistant","content":null,"tool_calls":[{"id":"call_1","type":"function","function":{"name":"get_weather","arguments":"{\"city\":\"Seoul\"}"}}]}""", "tool_calls");
        var sse = StubHttpHandler.SseData(
            Chunk("""{"role":"assistant","tool_calls":[{"index":0,"id":"call_1","type":"function","function":{"name":"get_weather","arguments":""}}]}""", finishReason: null),
            Chunk("""{"tool_calls":[{"index":0,"function":{"arguments":"{\"city\":"}}]}""", finishReason: null),
            Chunk("""{"tool_calls":[{"index":0,"function":{"arguments":"\"Seoul\"}"}}]}""", finishReason: null),
            Chunk("""{}""", finishReason: "tool_calls"),
            UsageChunk(),
            "[DONE]");

        var (buffered, frames) = await RunBothAsync(response, sse);

        buffered.DoneReason.Should().Be(MessageDoneReason.ToolCall);
        AssertEnvelopesAgree(buffered, frames);

        var bufferedTool = buffered.Message!.Content.OfType<ToolMessageContent>().Single();
        var streamedTool = frames.OfType<StreamingContentAddedResponse>().Select(f => f.Content).OfType<ToolMessageContent>().Single();
        streamedTool.Id.Should().Be(bufferedTool.Id);
        streamedTool.Name.Should().Be(bufferedTool.Name);
        var streamedArguments = (streamedTool.Input ?? string.Empty) + string.Concat(
            frames.OfType<StreamingContentDeltaResponse>().Select(f => f.Delta).OfType<ToolDeltaContent>().Select(d => d.Input));
        streamedArguments.Should().Be(bufferedTool.Input, "the argument deltas must add up to the buffered arguments");
    }

    // ---- the two halves, side by side ----

    private static async Task<(MessageResponse Buffered, List<StreamingMessageResponse> Frames)> RunBothAsync(string json, string sse)
    {
        var handler = new StubHttpHandler(json, sse);
        using var generator = new ChatCompletionMessageGenerator(new OpenAIConfig
        {
            ApiKey = "test-key",
            BaseUrl = "https://compatible.invalid/v1",
            HttpClient = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan },
        });

        var request = new MessageGenerationRequest { Model = Model, Messages = [Message.User("Hi")] };

        var buffered = await generator.GenerateMessageAsync(request, TestContext.Current.CancellationToken);

        var frames = new List<StreamingMessageResponse>();
        await foreach (var frame in generator.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken))
        {
            frames.Add(frame);
        }

        handler.Requests.Select(r => r.Streaming).Should().Equal([false, true], "the buffered call must be served JSON and the streaming call SSE");

        return (buffered, frames);
    }

    private static void AssertEnvelopesAgree(MessageResponse buffered, List<StreamingMessageResponse> frames)
    {
        frames.OfType<StreamingMessageErrorResponse>().Should().BeEmpty();

        var done = frames.OfType<StreamingMessageDoneResponse>().LastOrDefault();
        done.Should().NotBeNull("the streaming half must terminate with a done frame");

        done!.DoneReason.Should().Be(buffered.DoneReason);
        done.TokenUsage.Should().BeEquivalentTo(buffered.TokenUsage);
        buffered.ResponseId.Should().Be("chatcmpl-1", "the vendor's id must reach the consumer");
        done.ResponseId.Should().Be(buffered.ResponseId, "MessageService prefixes the id on both paths, so both must carry the same raw id");
        buffered.Model.Should().Be(Model, "the model the vendor reports must reach the consumer");
        done.Model.Should().Be(buffered.Model);
    }

    private static string TextOf(Message? message)
        => string.Concat((message?.Content ?? []).OfType<TextMessageContent>().Select(c => c.Value));

    private static string TextOf(IEnumerable<StreamingMessageResponse> frames)
    {
        var text = new System.Text.StringBuilder();
        foreach (var frame in frames)
        {
            switch (frame)
            {
                case StreamingContentAddedResponse { Content: TextMessageContent added }:
                    text.Append(added.Value);
                    break;
                case StreamingContentDeltaResponse { Delta: TextDeltaContent delta }:
                    text.Append(delta.Value);
                    break;
            }
        }
        return text.ToString();
    }

    // ---- recorded vendor bodies ----

    private const string UsageJson = """{"prompt_tokens":11,"completion_tokens":7,"total_tokens":18}""";

    private static string ResponseJson(string message, string finishReason)
        => $$$"""{"id":"chatcmpl-1","object":"chat.completion","created":{{{Created}}},"model":"{{{Model}}}","choices":[{"index":0,"message":{{{message}}},"finish_reason":"{{{finishReason}}}","logprobs":null}],"usage":{{{UsageJson}}}}""";

    private static string Chunk(string delta, string? finishReason)
        => $$$"""{"id":"chatcmpl-1","object":"chat.completion.chunk","created":{{{Created}}},"model":"{{{Model}}}","choices":[{"index":0,"delta":{{{delta}}},"finish_reason":{{{(finishReason is null ? "null" : $"\"{finishReason}\"")}}},"logprobs":null}]}""";

    private static string UsageChunk()
        => $$$"""{"id":"chatcmpl-1","object":"chat.completion.chunk","created":{{{Created}}},"model":"{{{Model}}}","choices":[],"usage":{{{UsageJson}}}}""";
}

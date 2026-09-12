using System.Text.Json.Nodes;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Providers.Anthropic;

namespace IronHive.Tests.Conventions.Providers;

// docs/CONVENTIONS.md section 5 at the provider layer, for the Anthropic Messages generator — the
// same harness and the same definition of "agree" as OpenAIResponsesEquivalenceTests: the buffered
// call and the stream, driven through the real SDK by recorded vendor bodies, must produce the same
// done reason, usage, id and model, and deltas that add up to the buffered content.
//
// Anthropic streams a tool call's input as partial_json fragments while the buffered call hands the
// SDK a parsed object that the generator re-serialises, so tool arguments are compared as JSON, not
// as strings: the two halves must mean the same call, not spell it identically.
public class AnthropicMessagesEquivalenceTests
{
    private const string Model = "claude-sonnet-4-5";

    [Fact]
    public async Task EndTurnText_BothHalvesCarryTheSameResult()
    {
        var response = MessageJson(
            content: """[{"type":"text","text":"Hello world"}]""",
            stopReason: "end_turn",
            outputTokens: 7);

        var sse = StubHttpHandler.Sse(
            ("message_start", $$$"""{"type":"message_start","message":{{{MessageJson("[]", null, 1)}}}}"""),
            ("content_block_start", """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}"""),
            ("ping", """{"type":"ping"}"""),
            ("content_block_delta", """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello "}}"""),
            ("content_block_delta", """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"world"}}"""),
            ("content_block_stop", """{"type":"content_block_stop","index":0}"""),
            ("message_delta", """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":7}}"""),
            ("message_stop", """{"type":"message_stop"}"""));

        var (buffered, frames) = await RunBothAsync(response, sse);

        buffered.DoneReason.Should().Be(MessageDoneReason.EndTurn);
        AssertEnvelopesAgree(buffered, frames);
        TextOf(frames).Should().Be(TextOf(buffered.Message), "the streamed deltas must add up to what the buffered call returns");
    }

    [Fact]
    public async Task MaxTokens_BothHalvesCarryTheSameResult()
    {
        var response = MessageJson(
            content: """[{"type":"text","text":"Hello"}]""",
            stopReason: "max_tokens",
            outputTokens: 1);

        var sse = StubHttpHandler.Sse(
            ("message_start", $$$"""{"type":"message_start","message":{{{MessageJson("[]", null, 1)}}}}"""),
            ("content_block_start", """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}"""),
            ("content_block_delta", """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}}"""),
            ("content_block_stop", """{"type":"content_block_stop","index":0}"""),
            ("message_delta", """{"type":"message_delta","delta":{"stop_reason":"max_tokens","stop_sequence":null},"usage":{"output_tokens":1}}"""),
            ("message_stop", """{"type":"message_stop"}"""));

        var (buffered, frames) = await RunBothAsync(response, sse);

        buffered.DoneReason.Should().Be(MessageDoneReason.MaxTokens, "the fixture must exercise the max_tokens stop, not end_turn");
        AssertEnvelopesAgree(buffered, frames);
        TextOf(frames).Should().Be(TextOf(buffered.Message));
    }

    [Fact]
    public async Task ToolUse_BothHalvesCarryTheSameToolCall()
    {
        var response = MessageJson(
            content: """[{"type":"tool_use","id":"toolu_1","name":"get_weather","input":{"city":"Seoul"}}]""",
            stopReason: "tool_use",
            outputTokens: 9);

        var sse = StubHttpHandler.Sse(
            ("message_start", $$$"""{"type":"message_start","message":{{{MessageJson("[]", null, 1)}}}}"""),
            ("content_block_start", """{"type":"content_block_start","index":0,"content_block":{"type":"tool_use","id":"toolu_1","name":"get_weather","input":{}}}"""),
            ("content_block_delta", """{"type":"content_block_delta","index":0,"delta":{"type":"input_json_delta","partial_json":"{\"city\":"}}"""),
            ("content_block_delta", """{"type":"content_block_delta","index":0,"delta":{"type":"input_json_delta","partial_json":"\"Seoul\"}"}}"""),
            ("content_block_stop", """{"type":"content_block_stop","index":0}"""),
            ("message_delta", """{"type":"message_delta","delta":{"stop_reason":"tool_use","stop_sequence":null},"usage":{"output_tokens":9}}"""),
            ("message_stop", """{"type":"message_stop"}"""));

        var (buffered, frames) = await RunBothAsync(response, sse);

        buffered.DoneReason.Should().Be(MessageDoneReason.ToolCall, "the fixture must exercise the tool_use stop");
        AssertEnvelopesAgree(buffered, frames);

        var bufferedTool = buffered.Message!.Content.OfType<ToolMessageContent>().Single();
        var streamedTool = frames.OfType<StreamingContentAddedResponse>().Select(f => f.Content).OfType<ToolMessageContent>().Single();
        streamedTool.Id.Should().Be(bufferedTool.Id);
        streamedTool.Name.Should().Be(bufferedTool.Name);

        var streamedArguments = (streamedTool.Input ?? string.Empty) + string.Concat(
            frames.OfType<StreamingContentDeltaResponse>().Select(f => f.Delta).OfType<ToolDeltaContent>().Select(d => d.Input));
        bufferedTool.Input.Should().NotBeNullOrEmpty("the buffered call must carry the tool arguments");
        JsonNode.DeepEquals(JsonNode.Parse(streamedArguments), JsonNode.Parse(bufferedTool.Input!))
            .Should().BeTrue($"the partial_json fragments must add up to the buffered arguments: streamed '{streamedArguments}', buffered '{bufferedTool.Input}'");
    }

    // ---- the two halves, side by side ----

    private static async Task<(MessageResponse Buffered, List<StreamingMessageResponse> Frames)> RunBothAsync(string json, string sse)
    {
        var handler = new StubHttpHandler(json, sse);
        using var generator = new AnthropicMessageGenerator(new AnthropicConfig
        {
            ApiKey = "test-key",
            HttpClient = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan },
        });

        var request = new MessageGenerationRequest { Model = Model, MaxTokens = 64, Messages = [Message.User("Hi")] };

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
        done.ResponseId.Should().Be(buffered.ResponseId);
        buffered.Model.Should().Be(Model, "the model must come through as the vendor's plain string, not a wrapper's rendering");
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

    private static string MessageJson(string content, string? stopReason, int outputTokens)
        => $$$"""
        {"id":"msg_1","type":"message","role":"assistant","model":"{{{Model}}}","content":{{{content}}},"stop_reason":{{{(stopReason is null ? "null" : $"\"{stopReason}\"")}}},"stop_sequence":null,"usage":{"input_tokens":11,"output_tokens":{{{outputTokens}}},"cache_creation_input_tokens":0,"cache_read_input_tokens":0}}
        """;
}

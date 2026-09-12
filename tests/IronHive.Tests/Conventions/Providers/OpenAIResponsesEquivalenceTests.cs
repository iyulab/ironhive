using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Providers.OpenAI;

namespace IronHive.Tests.Conventions.Providers;

// docs/CONVENTIONS.md section 5, at the provider layer: IMessageGenerator.GenerateMessage is one
// operation exposed twice, and each provider reconstructs the result separately for the buffered
// call and for the stream. StreamingPairRosterTests pins the pair; this asserts that the two
// halves agree for the OpenAI Responses generator, driven through the real SDK and the real
// mapping by recorded vendor bodies (StubHttpHandler).
//
// "Agree" here means: the same DoneReason, the same usage, the same response id, model and
// timestamp, and deltas that add up to the buffered content - text and tool calls alike.
public class OpenAIResponsesEquivalenceTests
{
    private const string Model = "gpt-4o";
    private const long CreatedAt = 1757635200; // 2025-09-12T00:00:00Z

    [Fact]
    public async Task CompletedText_BothHalvesCarryTheSameResult()
    {
        var response = ResponseJson(
            status: "completed",
            output: """[{"type":"message","id":"msg_1","status":"completed","role":"assistant","content":[{"type":"output_text","text":"Hello world","annotations":[]}]}]""",
            usage: Usage(11, 7));

        var sse = StubHttpHandler.Sse(
            ("response.created", Event("response.created", 0, $"\"response\":{ResponseJson("in_progress", "[]", null)}")),
            ("response.output_item.added", Event("response.output_item.added", 1, "\"output_index\":0,\"item\":{\"type\":\"message\",\"id\":\"msg_1\",\"status\":\"in_progress\",\"role\":\"assistant\",\"content\":[]}")),
            ("response.content_part.added", Event("response.content_part.added", 2, "\"item_id\":\"msg_1\",\"output_index\":0,\"content_index\":0,\"part\":{\"type\":\"output_text\",\"text\":\"\",\"annotations\":[]}")),
            ("response.output_text.delta", Event("response.output_text.delta", 3, "\"item_id\":\"msg_1\",\"output_index\":0,\"content_index\":0,\"delta\":\"Hello \"")),
            ("response.output_text.delta", Event("response.output_text.delta", 4, "\"item_id\":\"msg_1\",\"output_index\":0,\"content_index\":0,\"delta\":\"world\"")),
            ("response.output_text.done", Event("response.output_text.done", 5, "\"item_id\":\"msg_1\",\"output_index\":0,\"content_index\":0,\"text\":\"Hello world\"")),
            ("response.content_part.done", Event("response.content_part.done", 6, "\"item_id\":\"msg_1\",\"output_index\":0,\"content_index\":0,\"part\":{\"type\":\"output_text\",\"text\":\"Hello world\",\"annotations\":[]}")),
            ("response.output_item.done", Event("response.output_item.done", 7, "\"output_index\":0,\"item\":{\"type\":\"message\",\"id\":\"msg_1\",\"status\":\"completed\",\"role\":\"assistant\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hello world\",\"annotations\":[]}]}")),
            ("response.completed", Event("response.completed", 8, $"\"response\":{response}")));

        var (buffered, frames) = await RunBothAsync(response, sse);

        buffered.DoneReason.Should().Be(MessageDoneReason.EndTurn, "the fixture is a completed response with no tool call");
        AssertEnvelopesAgree(buffered, frames);
        TextOf(frames).Should().Be(TextOf(buffered.Message), "the streamed deltas must add up to what the buffered call returns");
    }

    [Fact]
    public async Task IncompleteMaxOutputTokens_BothHalvesCarryTheSameResult()
    {
        var response = ResponseJson(
            status: "incomplete",
            output: """[{"type":"message","id":"msg_1","status":"incomplete","role":"assistant","content":[{"type":"output_text","text":"Hello","annotations":[]}]}]""",
            usage: Usage(11, 1),
            incompleteDetails: """{"reason":"max_output_tokens"}""");

        var sse = StubHttpHandler.Sse(
            ("response.created", Event("response.created", 0, $"\"response\":{ResponseJson("in_progress", "[]", null)}")),
            ("response.output_item.added", Event("response.output_item.added", 1, "\"output_index\":0,\"item\":{\"type\":\"message\",\"id\":\"msg_1\",\"status\":\"in_progress\",\"role\":\"assistant\",\"content\":[]}")),
            ("response.content_part.added", Event("response.content_part.added", 2, "\"item_id\":\"msg_1\",\"output_index\":0,\"content_index\":0,\"part\":{\"type\":\"output_text\",\"text\":\"\",\"annotations\":[]}")),
            ("response.output_text.delta", Event("response.output_text.delta", 3, "\"item_id\":\"msg_1\",\"output_index\":0,\"content_index\":0,\"delta\":\"Hello\"")),
            ("response.output_item.done", Event("response.output_item.done", 4, "\"output_index\":0,\"item\":{\"type\":\"message\",\"id\":\"msg_1\",\"status\":\"incomplete\",\"role\":\"assistant\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hello\",\"annotations\":[]}]}")),
            ("response.incomplete", Event("response.incomplete", 5, $"\"response\":{response}")));

        var (buffered, frames) = await RunBothAsync(response, sse);

        buffered.DoneReason.Should().Be(MessageDoneReason.MaxTokens, "the fixture must exercise the incomplete path, not the completed one");
        AssertEnvelopesAgree(buffered, frames);
        TextOf(frames).Should().Be(TextOf(buffered.Message));
    }

    [Fact]
    public async Task FunctionCall_BothHalvesCarryTheSameToolCall()
    {
        var response = ResponseJson(
            status: "completed",
            output: """[{"type":"function_call","id":"fc_1","call_id":"call_1","name":"get_weather","arguments":"{\"city\":\"Seoul\"}","status":"completed"}]""",
            usage: Usage(20, 9));

        var sse = StubHttpHandler.Sse(
            ("response.created", Event("response.created", 0, $"\"response\":{ResponseJson("in_progress", "[]", null)}")),
            ("response.output_item.added", Event("response.output_item.added", 1, "\"output_index\":0,\"item\":{\"type\":\"function_call\",\"id\":\"fc_1\",\"call_id\":\"call_1\",\"name\":\"get_weather\",\"arguments\":\"\",\"status\":\"in_progress\"}")),
            ("response.function_call_arguments.delta", Event("response.function_call_arguments.delta", 2, "\"item_id\":\"fc_1\",\"output_index\":0,\"delta\":\"{\\\"city\\\":\"")),
            ("response.function_call_arguments.delta", Event("response.function_call_arguments.delta", 3, "\"item_id\":\"fc_1\",\"output_index\":0,\"delta\":\"\\\"Seoul\\\"}\"")),
            ("response.function_call_arguments.done", Event("response.function_call_arguments.done", 4, "\"item_id\":\"fc_1\",\"output_index\":0,\"arguments\":\"{\\\"city\\\":\\\"Seoul\\\"}\"")),
            ("response.output_item.done", Event("response.output_item.done", 5, "\"output_index\":0,\"item\":{\"type\":\"function_call\",\"id\":\"fc_1\",\"call_id\":\"call_1\",\"name\":\"get_weather\",\"arguments\":\"{\\\"city\\\":\\\"Seoul\\\"}\",\"status\":\"completed\"}")),
            ("response.completed", Event("response.completed", 6, $"\"response\":{response}")));

        var (buffered, frames) = await RunBothAsync(response, sse);

        buffered.DoneReason.Should().Be(MessageDoneReason.ToolCall, "the fixture must exercise the tool-call path");
        AssertEnvelopesAgree(buffered, frames);

        var bufferedTool = buffered.Message!.Content.OfType<ToolMessageContent>().Single();
        var streamedTool = frames.OfType<StreamingContentAddedResponse>().Select(f => f.Content).OfType<ToolMessageContent>().Single();
        streamedTool.Id.Should().Be(bufferedTool.Id);
        streamedTool.Name.Should().Be(bufferedTool.Name);

        var streamedArguments = streamedTool.Input + string.Concat(
            frames.OfType<StreamingContentDeltaResponse>().Select(f => f.Delta).OfType<ToolDeltaContent>().Select(d => d.Input));
        streamedArguments.Should().Be(bufferedTool.Input, "the argument deltas must add up to the buffered arguments");
    }

    // ---- the two halves, side by side ----

    private static async Task<(MessageResponse Buffered, List<StreamingMessageResponse> Frames)> RunBothAsync(string json, string sse)
    {
        var handler = new StubHttpHandler(json, sse);
        using var generator = new OpenAIMessageGenerator(new OpenAIConfig
        {
            ApiKey = "test-key",
            HttpClient = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan },
        });

        var request = new MessageGenerationRequest { Model = Model, Messages = [Message.User("Hi")] };

        var buffered = await generator.GenerateMessageAsync(request, TestContext.Current.CancellationToken);

        var frames = new List<StreamingMessageResponse>();
        await foreach (var frame in generator.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken))
        {
            frames.Add(frame);
        }

        // Fixture premise: the stub must have served one JSON body and one SSE body, in that order.
        // If the selection is wrong the two halves are compared against different recordings and
        // any agreement below is meaningless.
        handler.Requests.Select(r => r.Streaming).Should().Equal([false, true], "the buffered call must be served JSON and the streaming call SSE");

        return (buffered, frames);
    }

    private static void AssertEnvelopesAgree(MessageResponse buffered, List<StreamingMessageResponse> frames)
    {
        frames.OfType<StreamingMessageErrorResponse>().Should().BeEmpty("a recorded success must not surface as a streamed error");

        var done = frames.OfType<StreamingMessageDoneResponse>().LastOrDefault();
        done.Should().NotBeNull("the streaming half must terminate with a done frame");

        done!.DoneReason.Should().Be(buffered.DoneReason);
        done.TokenUsage.Should().BeEquivalentTo(buffered.TokenUsage, "usage aggregated on one path and dropped on the other is one of the defects this convention exists for");
        done.ResponseId.Should().Be(buffered.ResponseId, "MessageService prefixes the id with the provider name on both paths, so the generator must hand back the same raw id on both");
        done.Model.Should().Be(buffered.Model);
        done.Timestamp.Should().Be(buffered.Timestamp);
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

    private static string Usage(int input, int output)
        => $$$"""{"input_tokens":{{{input}}},"input_tokens_details":{"cached_tokens":0},"output_tokens":{{{output}}},"output_tokens_details":{"reasoning_tokens":0},"total_tokens":{{{input + output}}}}""";

    private static string ResponseJson(string status, string output, string? usage, string? incompleteDetails = null)
        => $$$"""
        {"id":"resp_1","object":"response","created_at":{{{CreatedAt}}},"status":"{{{status}}}","error":null,"incomplete_details":{{{incompleteDetails ?? "null"}}},"instructions":null,"max_output_tokens":null,"model":"{{{Model}}}","output":{{{output}}},"parallel_tool_calls":true,"previous_response_id":null,"reasoning":{"effort":null,"summary":null},"store":true,"temperature":1.0,"text":{"format":{"type":"text"}},"tool_choice":"auto","tools":[],"top_p":1.0,"truncation":"disabled","usage":{{{usage ?? "null"}}},"user":null,"metadata":{}}
        """;

    private static string Event(string type, int sequence, string payload)
        => $$$"""{"type":"{{{type}}}","sequence_number":{{{sequence}}},{{{payload}}}}""";
}

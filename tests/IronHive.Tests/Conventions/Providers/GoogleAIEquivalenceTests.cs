using System.Text.Json.Nodes;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Providers.GoogleAI;

namespace IronHive.Tests.Conventions.Providers;

// docs/CONVENTIONS.md section 5 at the provider layer, for the Gemini generator — the same harness
// and the same definition of "agree" as the OpenAI and Anthropic tests. Gemini streams whole parts
// per chunk (a function call arrives complete, text arrives as successive parts), and its stream
// carries no event names, so the recorded body is data-only SSE.
//
// Two shapes are deliberately covered because the two halves computed them differently when this
// test was written: a function call followed by text (which reason wins), and a function call the
// vendor sent without an id (what id the consumer gets).
public class GoogleAIEquivalenceTests
{
    private const string Model = "gemini-2.5-flash";

    [Fact]
    public async Task StopText_BothHalvesCarryTheSameResult()
    {
        var response = ResponseJson("""[{"text":"Hello world"}]""", "STOP", usage: true);
        var sse = StubHttpHandler.SseData(
            Chunk("""[{"text":"Hello "}]""", finishReason: null, usage: false),
            Chunk("""[{"text":"world"}]""", finishReason: "STOP", usage: true));

        var (buffered, frames) = await RunBothAsync(response, sse);

        buffered.DoneReason.Should().Be(MessageDoneReason.EndTurn);
        AssertEnvelopesAgree(buffered, frames);
        TextOf(frames).Should().Be(TextOf(buffered.Message), "the streamed parts must add up to what the buffered call returns");
    }

    [Fact]
    public async Task MaxTokens_BothHalvesCarryTheSameResult()
    {
        var response = ResponseJson("""[{"text":"Hello"}]""", "MAX_TOKENS", usage: true);
        var sse = StubHttpHandler.SseData(
            Chunk("""[{"text":"Hello"}]""", finishReason: "MAX_TOKENS", usage: true));

        var (buffered, frames) = await RunBothAsync(response, sse);

        buffered.DoneReason.Should().Be(MessageDoneReason.MaxTokens, "the fixture must exercise the MAX_TOKENS finish reason");
        AssertEnvelopesAgree(buffered, frames);
        TextOf(frames).Should().Be(TextOf(buffered.Message));
    }

    [Fact]
    public async Task FunctionCall_BothHalvesCarryTheSameToolCall()
    {
        var response = ResponseJson("""[{"functionCall":{"id":"fc_1","name":"get_weather","args":{"city":"Seoul"}}}]""", "STOP", usage: true);
        var sse = StubHttpHandler.SseData(
            Chunk("""[{"functionCall":{"id":"fc_1","name":"get_weather","args":{"city":"Seoul"}}}]""", finishReason: "STOP", usage: true));

        var (buffered, frames) = await RunBothAsync(response, sse);

        buffered.DoneReason.Should().Be(MessageDoneReason.ToolCall);
        AssertEnvelopesAgree(buffered, frames);
        AssertToolCallsAgree(buffered, frames);
    }

    [Fact]
    public async Task FunctionCallFollowedByText_BothHalvesReportAToolCall()
    {
        // Gemini may narrate after calling a tool. The buffered mapping reports ToolCall whenever a
        // function call is present; the streaming mapping must reach the same answer regardless of
        // which part happened to arrive last.
        var response = ResponseJson("""[{"functionCall":{"id":"fc_1","name":"get_weather","args":{"city":"Seoul"}}},{"text":"Looking that up."}]""", "STOP", usage: true);
        var sse = StubHttpHandler.SseData(
            Chunk("""[{"functionCall":{"id":"fc_1","name":"get_weather","args":{"city":"Seoul"}}}]""", finishReason: null, usage: false),
            Chunk("""[{"text":"Looking that up."}]""", finishReason: "STOP", usage: true));

        var (buffered, frames) = await RunBothAsync(response, sse);

        buffered.DoneReason.Should().Be(MessageDoneReason.ToolCall, "the fixture must carry a function call");
        AssertEnvelopesAgree(buffered, frames);
        AssertToolCallsAgree(buffered, frames);
        TextOf(frames).Should().Be(TextOf(buffered.Message));
    }

    [Fact]
    public async Task FunctionCallWithoutId_BothHalvesMintTheSameShapeOfId()
    {
        // Gemini often sends a function call with no id. Each half then mints one; the consumer must
        // not be able to tell which half it came from by the id's shape, because the agent loop
        // matches tool results back to calls by that id on both paths.
        var response = ResponseJson("""[{"functionCall":{"name":"get_weather","args":{"city":"Seoul"}}}]""", "STOP", usage: true);
        var sse = StubHttpHandler.SseData(
            Chunk("""[{"functionCall":{"name":"get_weather","args":{"city":"Seoul"}}}]""", finishReason: "STOP", usage: true));

        var (buffered, frames) = await RunBothAsync(response, sse);

        var bufferedTool = buffered.Message!.Content.OfType<ToolMessageContent>().Single();
        var streamedTool = frames.OfType<StreamingContentAddedResponse>().Select(f => f.Content).OfType<ToolMessageContent>().Single();
        bufferedTool.Id.Should().NotBeNullOrEmpty();
        streamedTool.Id.Should().NotBeNullOrEmpty();
        Prefix(streamedTool.Id!).Should().Be(Prefix(bufferedTool.Id!), $"a minted id must have the same shape on both paths: buffered '{bufferedTool.Id}', streamed '{streamedTool.Id}'");
    }

    // ---- the two halves, side by side ----

    private static async Task<(MessageResponse Buffered, List<StreamingMessageResponse> Frames)> RunBothAsync(string json, string sse)
    {
        var handler = new StubHttpHandler(json, sse);
        var httpClient = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        using var generator = new GoogleAIMessageGenerator(new GoogleAIConfig
        {
            ApiKey = "test-key",
            HttpClientFactory = () => httpClient,
        });

        var request = new MessageGenerationRequest { Model = Model, Messages = [Message.User("Hi")] };

        var buffered = await generator.GenerateMessageAsync(request, TestContext.Current.CancellationToken);

        var frames = new List<StreamingMessageResponse>();
        await foreach (var frame in generator.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken))
        {
            frames.Add(frame);
        }

        handler.Requests.Select(r => r.Streaming).Should().Equal([false, true], "the buffered call must be served JSON and the streaming call SSE (Gemini switches the URL to streamGenerateContent)");

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
        buffered.Model.Should().Be(Model, "the model version the vendor reports must reach the consumer");
        done.Model.Should().Be(buffered.Model);
    }

    private static void AssertToolCallsAgree(MessageResponse buffered, List<StreamingMessageResponse> frames)
    {
        var bufferedTool = buffered.Message!.Content.OfType<ToolMessageContent>().Single();
        var streamedTool = frames.OfType<StreamingContentAddedResponse>().Select(f => f.Content).OfType<ToolMessageContent>().Single();
        streamedTool.Id.Should().Be(bufferedTool.Id);
        streamedTool.Name.Should().Be(bufferedTool.Name);
        bufferedTool.Input.Should().NotBeNullOrEmpty();
        JsonNode.DeepEquals(JsonNode.Parse(streamedTool.Input ?? "null"), JsonNode.Parse(bufferedTool.Input!))
            .Should().BeTrue($"both halves must carry the same arguments: buffered '{bufferedTool.Input}', streamed '{streamedTool.Input}'");
    }

    private static string Prefix(string id)
    {
        var underscore = id.IndexOf('_');
        return underscore < 0 ? string.Empty : id[..(underscore + 1)];
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

    private const string UsageJson = """{"promptTokenCount":11,"candidatesTokenCount":7,"totalTokenCount":18}""";

    private static string ResponseJson(string parts, string finishReason, bool usage)
        => Chunk(parts, finishReason, usage);

    private static string Chunk(string parts, string? finishReason, bool usage)
        => $$$"""
        {"candidates":[{"content":{"parts":{{{parts}}},"role":"model"},{{{(finishReason is null ? "" : $"\"finishReason\":\"{finishReason}\",")}}}"index":0}],{{{(usage ? $"\"usageMetadata\":{UsageJson}," : "")}}}"modelVersion":"{{{Model}}}","responseId":"resp_1"}
        """;
}

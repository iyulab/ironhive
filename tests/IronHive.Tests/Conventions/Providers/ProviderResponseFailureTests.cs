using System.Net;
using AwesomeAssertions;
using IronHive.Abstractions.Exceptions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;

namespace IronHive.Tests.Conventions.Providers;

// A stream that started with 200 can still fail: the vendor sends an error inside it, or the connection ends before the
// vendor's completion signal. Either way the text that arrived is not the whole answer, and a consumer that reads the
// stream to its end must not be able to mistake it for a completed one. Each case serves a recorded body through the
// real SDK and generator (StubHttpHandler) and checks two things together: the stream throws a typed exception, and the
// text streamed before the failure still reached the consumer.
public class ProviderResponseFailureTests
{
    // ---- OpenAI Responses ----

    [Fact]
    public async Task OpenAIResponses_ErrorEvent_ThrowsProviderResponseException()
    {
        var sse = OpenAIPrefix() + StubHttpHandler.Sse(
            ("error", """{"type":"error","sequence_number":4,"code":"server_error","message":"The server had an error while processing your request.","param":null}"""));

        var (frames, error) = await RunAsync(OpenAI(sse));

        var thrown = error.Should().BeOfType<ProviderResponseException>().Which;
        thrown.ErrorCode.Should().Be("server_error");
        thrown.EquivalentStatusCode.Should().Be(HttpStatusCode.InternalServerError, "OpenAI documents server_error as 500");
        TextOf(frames).Should().Be("Hel", "the text streamed before the error is still delivered");
        frames.OfType<StreamingMessageDoneResponse>().Should().BeEmpty("a done frame would read as a completed response");
    }

    [Fact]
    public async Task OpenAIResponses_FailedEvent_KeepsItsRateLimitType()
    {
        var failed = OpenAIResponse("failed").Replace(
            "\"error\":null",
            "\"error\":{\"code\":\"rate_limit_exceeded\",\"message\":\"Rate limit reached for requests\"}",
            StringComparison.Ordinal);
        var sse = OpenAIPrefix() + StubHttpHandler.Sse(
            ("response.failed", OpenAIEvent("response.failed", 4, $"\"response\":{failed}")));

        var (frames, error) = await RunAsync(OpenAI(sse));

        error.Should().BeOfType<RateLimitException>("a rate limit inside a stream is still a rate limit");
        TextOf(frames).Should().Be("Hel");
    }

    [Fact]
    public async Task OpenAIResponses_StreamEndingWithoutCompletion_Throws()
    {
        var (frames, error) = await RunAsync(OpenAI(OpenAIPrefix()));

        var thrown = error.Should().BeOfType<ProviderResponseException>().Which;
        thrown.ErrorCode.Should().BeNull("no error was sent — the stream just stopped");
        thrown.EquivalentStatusCode.Should().BeNull("no vendor error, so no documented status");
        TextOf(frames).Should().Be("Hel");
    }

    [Fact]
    public async Task OpenAIResponses_FailedBufferedResponse_ThrowsWhatTheStreamThrows()
    {
        // The buffered half of response.failed: 200 with status «failed» and an error object.
        var failed = OpenAIResponse("failed").Replace(
            "\"error\":null",
            "\"error\":{\"code\":\"server_error\",\"message\":\"The server had an error.\"}",
            StringComparison.Ordinal);
        using var generator = new OpenAIMessageGenerator(new OpenAIConfig
        {
            ApiKey = "test-key",
            HttpClient = new HttpClient(new StubHttpHandler(failed, "")) { Timeout = Timeout.InfiniteTimeSpan },
        });

        var act = () => generator.GenerateMessageAsync(Request(), TestContext.Current.CancellationToken);

        (await act.Should().ThrowAsync<ProviderResponseException>()).Which.ErrorCode.Should().Be("server_error");
    }

    // ---- Chat Completions (OpenAI-compatible servers) ----

    [Fact]
    public async Task ChatCompletions_ErrorDataLine_ThrowsProviderResponseException()
    {
        var sse = StubHttpHandler.SseData(
            ChatChunk("""{"role":"assistant","content":"Hel"}""", finishReason: null),
            """{"error":{"message":"Upstream overloaded","type":"server_error","code":503}}""");

        var (frames, error) = await RunAsync(Chat(sse));

        var thrown = error.Should().BeOfType<ProviderResponseException>().Which;
        thrown.Message.Should().Be("Upstream overloaded");
        thrown.ErrorCode.Should().Be("503", "a numeric code is kept as the server wrote it");
        thrown.EquivalentStatusCode.Should().Be(HttpStatusCode.ServiceUnavailable, "a numeric code in the error range is the status");
        TextOf(frames).Should().Be("Hel");
        frames.OfType<StreamingMessageDoneResponse>().Should().BeEmpty();
    }

    [Fact]
    public async Task ChatCompletions_BillingErrorDataLine_IsBillingWithoutStatus()
    {
        var sse = StubHttpHandler.SseData(
            ChatChunk("""{"role":"assistant","content":"Hel"}""", finishReason: null),
            """{"error":{"message":"You exceeded your current quota.","type":"insufficient_quota","code":"insufficient_quota"}}""");

        var (_, error) = await RunAsync(Chat(sse));

        error.Should().BeOfType<BillingException>().Which.StatusCode.Should().BeNull("an error inside a stream has no status of its own");
    }

    [Fact]
    public async Task ChatCompletions_StreamEndingWithoutFinishReason_Throws()
    {
        var sse = StubHttpHandler.SseData(ChatChunk("""{"role":"assistant","content":"Hel"}""", finishReason: null));

        var (frames, error) = await RunAsync(Chat(sse));

        error.Should().BeOfType<ProviderResponseException>();
        TextOf(frames).Should().Be("Hel");
    }

    [Fact]
    public async Task ChatCompletions_CompleteStream_DoesNotThrow()
    {
        // Positive control for the guard above: a stream that ends with its finish_reason is a finished answer.
        var sse = StubHttpHandler.SseData(
            ChatChunk("""{"role":"assistant","content":"Hel"}""", finishReason: null),
            ChatChunk("""{}""", finishReason: "stop"),
            "[DONE]");

        var (frames, error) = await RunAsync(Chat(sse));

        error.Should().BeNull();
        frames.OfType<StreamingMessageDoneResponse>().Should().ContainSingle();
    }

    [Fact]
    public async Task ChatCompletions_DoneMarkerWithoutFinishReason_IsAFinishedAnswer()
    {
        // `data: [DONE]` is the protocol's end marker; a gateway or test server that omits finish_reason still says the
        // response is whole. Only a stream that closes with neither is cut off (the test above).
        var sse = StubHttpHandler.SseData(
            ChatChunk("""{"role":"assistant","content":"Hel"}""", finishReason: null),
            ChatChunk("""{"content":"lo"}""", finishReason: null),
            "[DONE]");

        var (frames, error) = await RunAsync(Chat(sse));

        error.Should().BeNull();
        TextOf(frames).Should().Be("Hello");
        frames.OfType<StreamingMessageDoneResponse>().Should().ContainSingle()
            .Which.DoneReason.Should().Be(MessageDoneReason.EndTurn);
    }

    [Fact]
    public async Task ChatCompletions_DoneMarkerAfterToolCallWithoutFinishReason_EndsWithToolCall()
    {
        var sse = StubHttpHandler.SseData(
            ChatChunk("""{"role":"assistant","tool_calls":[{"index":0,"id":"call_1","type":"function","function":{"name":"lookup","arguments":"{}"}}]}""", finishReason: null),
            "[DONE]");

        var (frames, error) = await RunAsync(Chat(sse));

        error.Should().BeNull();
        frames.OfType<StreamingMessageDoneResponse>().Should().ContainSingle()
            .Which.DoneReason.Should().Be(MessageDoneReason.ToolCall);
    }

    [Fact]
    public async Task ChatCompletions_LinesAfterDoneMarker_AreNotRead()
    {
        // [DONE] ends the response: a proxy that keeps the connection open, or writes more, does not extend the answer.
        var sse = StubHttpHandler.SseData(
            ChatChunk("""{"role":"assistant","content":"Hel"}""", finishReason: "stop"),
            "[DONE]",
            """{"error":{"message":"late","type":"server_error"}}""");

        var (frames, error) = await RunAsync(Chat(sse));

        error.Should().BeNull();
        TextOf(frames).Should().Be("Hel");
    }

    // ---- Anthropic ----

    [Fact]
    public async Task Anthropic_OverloadedErrorEvent_ThrowsProviderResponseException()
    {
        var sse = AnthropicPrefix() + StubHttpHandler.Sse(
            ("error", """{"type":"error","error":{"type":"overloaded_error","message":"Overloaded"}}"""));

        var (frames, error) = await RunAsync(Anthropic(sse));

        var thrown = error.Should().BeOfType<ProviderResponseException>().Which;
        thrown.ErrorCode.Should().Be("overloaded_error");
        thrown.EquivalentStatusCode.Should().Be((HttpStatusCode)529, "Anthropic documents overloaded_error as 529");
        thrown.Message.Should().Be("Overloaded", "the vendor's message, not the SDK's wrapper around the event data");
        TextOf(frames).Should().Be("Hel");
    }

    [Fact]
    public async Task Anthropic_InvalidRequestErrorEvent_CarriesTheRefusalStatus()
    {
        var sse = AnthropicPrefix() + StubHttpHandler.Sse(
            ("error", """{"type":"error","error":{"type":"invalid_request_error","message":"Bad request"}}"""));

        var (_, error) = await RunAsync(Anthropic(sse));

        error.Should().BeOfType<ProviderResponseException>()
            .Which.EquivalentStatusCode.Should().Be(HttpStatusCode.BadRequest, "a refusal, not a transient fault");
    }

    [Fact]
    public async Task Anthropic_BillingErrorEvent_IsBillingWithoutStatus()
    {
        var sse = AnthropicPrefix() + StubHttpHandler.Sse(
            ("error", """{"type":"error","error":{"type":"billing_error","message":"Your credit balance is too low."}}"""));

        var (_, error) = await RunAsync(Anthropic(sse));

        error.Should().BeOfType<BillingException>().Which.StatusCode.Should().BeNull();
    }

    [Fact]
    public async Task Anthropic_StreamEndingWithoutStopReason_Throws()
    {
        var (frames, error) = await RunAsync(Anthropic(AnthropicPrefix()));

        error.Should().BeOfType<ProviderResponseException>();
        TextOf(frames).Should().Be("Hel");
    }

    // ---- Gemini ----

    [Fact]
    public async Task Gemini_ErrorDataLine_Throws()
    {
        // Google.GenAI reads this event as an empty response and raises nothing; the stream then ends without a finish
        // reason, which is what the generator catches.
        var sse = StubHttpHandler.SseData(
            GeminiChunk("""[{"text":"Hel"}]""", finishReason: null),
            """{"error":{"code":503,"message":"The model is overloaded. Please try again later.","status":"UNAVAILABLE"}}""");

        var (frames, error) = await RunAsync(Gemini(sse));

        error.Should().BeOfType<ProviderResponseException>();
        TextOf(frames).Should().Be("Hel");
        frames.OfType<StreamingMessageDoneResponse>().Should().BeEmpty();
    }

    [Fact]
    public async Task Gemini_BlockedPrompt_EndsAsContentFilter()
    {
        // A blocked prompt has feedback and no candidate — a finished response the guard must not read as a cut one.
        var sse = StubHttpHandler.SseData(
            $$$"""{"promptFeedback":{"blockReason":"SAFETY"},"usageMetadata":{"promptTokenCount":5,"totalTokenCount":5},"modelVersion":"{{{GeminiModel}}}","responseId":"resp_1"}""");

        var (frames, error) = await RunAsync(Gemini(sse));

        error.Should().BeNull();
        frames.OfType<StreamingMessageDoneResponse>().Should().ContainSingle()
            .Which.DoneReason.Should().Be(MessageDoneReason.ContentFilter);
    }

    [Fact]
    public async Task Gemini_BlockedPrompt_Buffered_EndsAsContentFilter()
    {
        var json = $$$"""{"promptFeedback":{"blockReason":"SAFETY"},"usageMetadata":{"promptTokenCount":5,"totalTokenCount":5},"modelVersion":"{{{GeminiModel}}}","responseId":"resp_1"}""";
        var client = new HttpClient(new StubHttpHandler(json, "")) { Timeout = Timeout.InfiniteTimeSpan };
        using var generator = new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key", HttpClientFactory = () => client });

        var response = await generator.GenerateMessageAsync(Request(), TestContext.Current.CancellationToken);

        response.DoneReason.Should().Be(MessageDoneReason.ContentFilter, "the streaming half ends the same prompt as ContentFilter");
        response.TokenUsage!.InputTokens.Should().Be(5);
    }

    // ---- harness ----

    private static MessageGenerationRequest Request()
        => new() { Model = "model", MaxTokens = 64, Messages = [Message.User("Hi")] };

    private static async Task<(List<StreamingMessageResponse> Frames, Exception? Error)> RunAsync(IMessageGenerator generator)
    {
        var request = Request();
        var frames = new List<StreamingMessageResponse>();
        try
        {
            await foreach (var frame in generator.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken))
                frames.Add(frame);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return (frames, ex);
        }
        finally
        {
            generator.Dispose();
        }
        return (frames, null);
    }

    private static HttpClient Client(string sse)
        => new(new StubHttpHandler("{}", sse)) { Timeout = Timeout.InfiniteTimeSpan };

    private static OpenAIMessageGenerator OpenAI(string sse)
        => new OpenAIMessageGenerator(new OpenAIConfig { ApiKey = "test-key", HttpClient = Client(sse) });

    private static ChatCompletionMessageGenerator Chat(string sse)
        => new ChatCompletionMessageGenerator(new OpenAIConfig
        {
            ApiKey = "test-key",
            BaseUrl = "https://compatible.invalid/v1",
            HttpClient = Client(sse),
        });

    private static AnthropicMessageGenerator Anthropic(string sse)
        => new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key", HttpClient = Client(sse) });

    private static GoogleAIMessageGenerator Gemini(string sse)
    {
        var client = Client(sse);
        return new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key", HttpClientFactory = () => client });
    }

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

    // ---- recorded vendor bodies: a response that has started and streamed «Hel» ----

    private const string OpenAIModel = "gpt-4o-mini";

    private static string OpenAIPrefix() => StubHttpHandler.Sse(
        ("response.created", OpenAIEvent("response.created", 0, $"\"response\":{OpenAIResponse("in_progress")}")),
        ("response.output_item.added", OpenAIEvent("response.output_item.added", 1, "\"output_index\":0,\"item\":{\"type\":\"message\",\"id\":\"msg_1\",\"status\":\"in_progress\",\"role\":\"assistant\",\"content\":[]}")),
        ("response.content_part.added", OpenAIEvent("response.content_part.added", 2, "\"item_id\":\"msg_1\",\"output_index\":0,\"content_index\":0,\"part\":{\"type\":\"output_text\",\"text\":\"\",\"annotations\":[]}")),
        ("response.output_text.delta", OpenAIEvent("response.output_text.delta", 3, "\"item_id\":\"msg_1\",\"output_index\":0,\"content_index\":0,\"delta\":\"Hel\"")));

    private static string OpenAIResponse(string status)
        => $$$"""
        {"id":"resp_1","object":"response","created_at":1757635200,"status":"{{{status}}}","error":null,"incomplete_details":null,"instructions":null,"max_output_tokens":null,"model":"{{{OpenAIModel}}}","output":[],"parallel_tool_calls":true,"previous_response_id":null,"reasoning":{"effort":null,"summary":null},"store":true,"temperature":1.0,"text":{"format":{"type":"text"}},"tool_choice":"auto","tools":[],"top_p":1.0,"truncation":"disabled","usage":null,"user":null,"metadata":{}}
        """;

    private static string OpenAIEvent(string type, int sequence, string payload)
        => $$$"""{"type":"{{{type}}}","sequence_number":{{{sequence}}},{{{payload}}}}""";

    private static string ChatChunk(string delta, string? finishReason)
        => $$$"""{"id":"chatcmpl-1","object":"chat.completion.chunk","created":1757635200,"model":"gpt-4o-mini","choices":[{"index":0,"delta":{{{delta}}},"finish_reason":{{{(finishReason is null ? "null" : $"\"{finishReason}\"")}}},"logprobs":null}]}""";

    private static string AnthropicPrefix() => StubHttpHandler.Sse(
        ("message_start", """{"type":"message_start","message":{"id":"msg_1","type":"message","role":"assistant","model":"claude-sonnet-4-5","content":[],"stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":11,"output_tokens":1}}}"""),
        ("content_block_start", """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}"""),
        ("content_block_delta", """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hel"}}"""));

    private const string GeminiModel = "gemini-2.5-flash";

    private static string GeminiChunk(string parts, string? finishReason)
        => $$$"""
        {"candidates":[{"content":{"parts":{{{parts}}},"role":"model"},{{{(finishReason is null ? "" : $"\"finishReason\":\"{finishReason}\",")}}}"index":0}],"modelVersion":"{{{GeminiModel}}}","responseId":"resp_1"}
        """;
}

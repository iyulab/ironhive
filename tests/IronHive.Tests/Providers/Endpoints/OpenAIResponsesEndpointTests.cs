using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using IronHive.Providers.Declarations;
using IronHive.Providers.Endpoints;
using Microsoft.Extensions.AI;
using static IronHive.Tests.Providers.Endpoints.ChatCompletionsEndpointTests;

namespace IronHive.Tests.Providers.Endpoints;

/// <summary>
/// A chat client mounted with <c>MapOpenAIResponses</c> is read back through the library's own Responses client (the OpenAI SDK
/// underneath), declared the way a host declares a provider: the two halves of the stateless Responses subset must agree.
/// </summary>
public sealed class OpenAIResponsesEndpointTests
{
    private static ProviderDeclaration Declare(string origin) => new()
    {
        Id = "served",
        Wire = WireFormat.OpenAIResponses,
        BaseUrl = origin + "/v1",
        ApiKey = "placeholder",
    };

    [Fact]
    public async Task Text_RoundTripsThroughTheLibrariesOwnResponsesClient()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "hello there"))
        {
            FinishReason = ChatFinishReason.Stop,
            Usage = new UsageDetails { InputTokenCount = 11, OutputTokenCount = 4, TotalTokenCount = 15 },
        });
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIResponses("/v1/responses", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "model-a");
        List<ChatMessage> messages = [new(ChatRole.System, "Be terse."), new(ChatRole.User, "hi")];

        var response = await client.GetResponseAsync(messages, new ChatOptions { MaxOutputTokens = 321 }, TestContext.Current.CancellationToken);

        response.Text.Should().Be("hello there");
        response.Usage!.InputTokenCount.Should().Be(11);
        response.Usage.OutputTokenCount.Should().Be(4);
        served.LastMessages.Should().Contain(m => m.Role == ChatRole.System && m.Text.Contains("Be terse."));
        served.LastMessages.Should().Contain(m => m.Role == ChatRole.User && m.Text == "hi");
        served.LastOptions!.ModelId.Should().Be("model-a");
        served.LastOptions.MaxOutputTokens.Should().Be(321);
    }

    [Fact]
    public async Task ToolCall_GoesBackToTheCaller_AndTheResultComesInOnTheNextTurn()
    {
        var turn = 0;
        var served = new ScriptedChatClient((_, _) =>
        {
            turn++;
            return turn == 1
                ? new ChatResponse(new ChatMessage(ChatRole.Assistant,
                    [new TextContent("Let me check."), new FunctionCallContent("call_1", "get_weather", new Dictionary<string, object?> { ["city"] = "Seoul" })]))
                : new ChatResponse(new ChatMessage(ChatRole.Assistant, "It is sunny in Seoul."));
        });
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIResponses("/v1/responses", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "model-a");
        var weather = AIFunctionFactory.CreateDeclaration("get_weather", "Weather of a city",
            JsonDocument.Parse("""{"type":"object","properties":{"city":{"type":"string"}},"required":["city"]}""").RootElement);
        var options = new ChatOptions { Tools = [weather], ToolMode = ChatToolMode.RequireAny };
        var cancellationToken = TestContext.Current.CancellationToken;

        var first = await client.GetResponseAsync("weather in Seoul?", options, cancellationToken);

        first.Text.Should().Contain("Let me check.");
        var call = first.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().Should().ContainSingle().Subject;
        call.Name.Should().Be("get_weather");
        call.CallId.Should().Be("call_1");
        call.Arguments!["city"]!.ToString().Should().Be("Seoul");
        served.LastOptions!.Tools.Should().ContainSingle().Which.Name.Should().Be("get_weather");
        served.LastOptions.ToolMode.Should().Be(ChatToolMode.RequireAny);

        List<ChatMessage> history =
        [
            new(ChatRole.User, "weather in Seoul?"),
            .. first.Messages,
            new(ChatRole.Tool, [new FunctionResultContent("call_1", "sunny, 21C")]),
        ];
        var second = await client.GetResponseAsync(history, options, cancellationToken);

        second.Text.Should().Be("It is sunny in Seoul.");
        served.LastMessages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().Should().ContainSingle().Which.CallId.Should().Be("call_1");
        var result = served.LastMessages.SelectMany(m => m.Contents).OfType<FunctionResultContent>().Should().ContainSingle().Subject;
        result.CallId.Should().Be("call_1");
        result.Result!.ToString().Should().Contain("sunny, 21C");
    }

    [Fact]
    public async Task Image_ArrivesAsData()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "a cat")));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIResponses("/v1/responses", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "model-a");
        byte[] png = [137, 80, 78, 71, 13, 10, 26, 10, 1, 2, 3];

        await client.GetResponseAsync(
            [new ChatMessage(ChatRole.User, [new TextContent("what is this?"), new DataContent(png, "image/png")])],
            cancellationToken: TestContext.Current.CancellationToken);

        var image = served.LastMessages.SelectMany(m => m.Contents).OfType<DataContent>().Should().ContainSingle().Subject;
        image.MediaType.Should().Be("image/png");
        image.Data.ToArray().Should().Equal(png);
    }

    [Fact]
    public async Task Streaming_TextAndToolCall_RoundTripThroughTheLibrariesOwnResponsesClient()
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Updates(
            new ChatResponseUpdate(ChatRole.Assistant, "Hel"),
            new ChatResponseUpdate(ChatRole.Assistant, "lo"),
            new ChatResponseUpdate(ChatRole.Assistant, [new FunctionCallContent("call_9", "get_weather", new Dictionary<string, object?> { ["city"] = "Busan" })]),
            new ChatResponseUpdate { Contents = [new UsageContent(new UsageDetails { InputTokenCount = 5, OutputTokenCount = 3 })] }));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIResponses("/v1/responses", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "model-a");
        var weather = AIFunctionFactory.CreateDeclaration("get_weather", "Weather of a city",
            JsonDocument.Parse("""{"type":"object","properties":{"city":{"type":"string"}}}""").RootElement);

        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in client.GetStreamingResponseAsync("go", new ChatOptions { Tools = [weather] }, TestContext.Current.CancellationToken))
        {
            updates.Add(update);
        }

        var response = updates.ToChatResponse();
        response.Text.Should().Be("Hello");
        var call = response.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().Should().ContainSingle().Subject;
        call.CallId.Should().Be("call_9");
        call.Arguments!["city"]!.ToString().Should().Be("Busan");
        response.Usage!.OutputTokenCount.Should().Be(3);
    }

    [Fact]
    public async Task Streaming_Framing_FollowsTheResponseEventOrder_WithIncreasingSequenceNumbers()
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Updates(
            new ChatResponseUpdate(ChatRole.Assistant, "A") { ModelId = "served-model" },
            new ChatResponseUpdate(ChatRole.Assistant, "B"),
            new ChatResponseUpdate(ChatRole.Assistant, [new FunctionCallContent("call_1", "f", new Dictionary<string, object?> { ["x"] = 1 })]),
            new ChatResponseUpdate { Contents = [new UsageContent(new UsageDetails { InputTokenCount = 7, OutputTokenCount = 2 })] }));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIResponses("/v1/responses", served));

        var (status, contentType, body) = await host.PostStream("/v1/responses", """{"model":"asked","stream":true,"input":"u"}""");

        status.Should().Be(HttpStatusCode.OK);
        contentType.Should().Be("text/event-stream");
        var events = body.Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Split('\n', 2))
            .Select(parts => (Name: parts[0]["event: ".Length..], Data: JsonDocument.Parse(parts[1]["data: ".Length..]).RootElement))
            .ToList();
        events.Select(e => e.Name).Should().Equal(
            "response.created", "response.in_progress",
            "response.output_item.added", "response.content_part.added", "response.output_text.delta", "response.output_text.delta",
            "response.output_text.done", "response.content_part.done", "response.output_item.done",
            "response.output_item.added", "response.function_call_arguments.delta", "response.function_call_arguments.done", "response.output_item.done",
            "response.completed");
        events.Should().OnlyContain(e => e.Data.GetProperty("type").GetString() == e.Name);
        events.Select(e => e.Data.GetProperty("sequence_number").GetInt32()).Should().BeInAscendingOrder().And.OnlyHaveUniqueItems();
        events[0].Data.GetProperty("response").GetProperty("status").GetString().Should().Be("in_progress");
        events[0].Data.GetProperty("response").GetProperty("model").GetString().Should().Be("served-model");
        events[6].Data.GetProperty("text").GetString().Should().Be("AB");
        var completed = events[^1].Data.GetProperty("response");
        completed.GetProperty("status").GetString().Should().Be("completed");
        completed.GetProperty("output").GetArrayLength().Should().Be(2);
        completed.GetProperty("output")[0].GetProperty("content")[0].GetProperty("text").GetString().Should().Be("AB");
        completed.GetProperty("output")[1].GetProperty("call_id").GetString().Should().Be("call_1");
        completed.GetProperty("output")[1].GetProperty("arguments").GetString().Should().Be("""{"x":1}""");
        completed.GetProperty("usage").GetProperty("total_tokens").GetInt32().Should().Be(9);
        events.Where(e => e.Name.StartsWith("response.output_", StringComparison.Ordinal) || e.Name.StartsWith("response.content_part", StringComparison.Ordinal)
                || e.Name.StartsWith("response.function_call", StringComparison.Ordinal))
            .Select(e => e.Data.GetProperty("output_index").GetInt32()).Should().Equal(0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1);
    }

    [Fact]
    public async Task RequestItems_ArePlacedWhereTheChatClientExpectsThem()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIResponses("/v1/responses", served));

        var (status, _) = await host.Post("/v1/responses", """
            {"model":"m","instructions":"Be terse.","max_output_tokens":55,"temperature":0.25,"top_p":0.5,"parallel_tool_calls":false,
             "tool_choice":{"type":"function","name":"f"},
             "tools":[{"type":"function","name":"f","description":"d","parameters":{"type":"object","properties":{"a":{"type":"integer"}}}}],
             "text":{"format":{"type":"json_schema","name":"answer","schema":{"type":"object","properties":{"x":{"type":"integer"}}}}},
             "input":[
               {"role":"developer","content":"dev note"},
               {"type":"message","role":"user","content":[{"type":"input_text","text":"first"},{"type":"input_image","image_url":"data:image/png;base64,AQID"}]},
               {"type":"reasoning","id":"rs_1","summary":[]},
               {"type":"message","role":"assistant","content":[{"type":"output_text","text":"calling"}]},
               {"type":"function_call","call_id":"c1","name":"f","arguments":"{\"a\":1}"},
               {"type":"function_call","call_id":"c2","name":"f","arguments":"{\"a\":2}"},
               {"type":"function_call_output","call_id":"c1","output":"r1"},
               {"type":"function_call_output","call_id":"c2","output":"r2"}
             ]}
            """);

        status.Should().Be(HttpStatusCode.OK);
        served.LastMessages.Select(m => m.Role).Should().Equal(ChatRole.System, ChatRole.System, ChatRole.User, ChatRole.Assistant, ChatRole.Assistant, ChatRole.Tool);
        served.LastMessages[0].Text.Should().Be("Be terse.");
        served.LastMessages[2].Contents.OfType<DataContent>().Should().ContainSingle().Which.Data.ToArray().Should().Equal(1, 2, 3);
        served.LastMessages[4].Contents.OfType<FunctionCallContent>().Select(c => c.CallId).Should().Equal("c1", "c2");
        served.LastMessages[5].Contents.OfType<FunctionResultContent>().Select(c => (c.CallId, c.Result)).Should().Equal(("c1", (object?)"r1"), ("c2", "r2"));
        var options = served.LastOptions!;
        options.MaxOutputTokens.Should().Be(55);
        options.Temperature.Should().Be(0.25f);
        options.TopP.Should().Be(0.5f);
        options.AllowMultipleToolCalls.Should().BeFalse();
        options.ToolMode.Should().Be(ChatToolMode.RequireSpecific("f"));
        options.ResponseFormat.Should().BeOfType<ChatResponseFormatJson>().Which.SchemaName.Should().Be("answer");
    }

    [Theory]
    [InlineData("""{"model":"m","input":"u","previous_response_id":"resp_1"}""", "keeps no conversation state", "previous_response_id")]
    [InlineData("""{"model":"m","input":"u","conversation":"conv_1"}""", "keeps no conversation state", "conversation")]
    [InlineData("""{"model":"m","input":"u","background":true}""", "Background responses", "background")]
    [InlineData("""{"model":"m"}""", "'input' is required", "input")]
    [InlineData("""{"model":"m","input":[]}""", "non-empty array", "input")]
    [InlineData("""{"model":"m","input":[{"type":"computer_call"}]}""", "'computer_call' is not supported", "input[0].type")]
    [InlineData("""{"model":"m","input":[{"role":"wizard","content":"x"}]}""", "'wizard' is not supported", "input[0].role")]
    [InlineData("""{"model":"m","input":"u","tools":[{"type":"web_search"}]}""", "built-in tools are not served", "tools[0]")]
    public async Task MalformedRequest_IsA400InTheWiresErrorShape(string body, string messagePart, string param)
    {
        var served = new ScriptedChatClient((_, _) => throw new InvalidOperationException("must not be called"));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIResponses("/v1/responses", served));

        var (status, text) = await host.Post("/v1/responses", body);

        status.Should().Be(HttpStatusCode.BadRequest);
        var error = JsonDocument.Parse(text).RootElement.GetProperty("error");
        error.GetProperty("type").GetString().Should().Be("invalid_request_error");
        error.GetProperty("message").GetString().Should().Contain(messagePart);
        error.GetProperty("param").GetString().Should().Be(param);
        served.Calls.Should().Be(0);
    }

    [Fact]
    public async Task UnknownModel_Is404_AndAClientFailure_IsA500WithoutTheUpstreamDetail()
    {
        var failing = new ScriptedChatClient((_, _) => throw new InvalidOperationException("upstream rejected key sk-secret"));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIResponses("/v1/responses", o =>
            o.ClientResolver = (_, model) => new ValueTask<IChatClient?>(model == "known" ? failing : null)));

        var (missing, missingBody) = await host.Post("/v1/responses", """{"model":"other","input":"u"}""");
        var (failed, failedBody) = await host.Post("/v1/responses", """{"model":"known","input":"u"}""");

        missing.Should().Be(HttpStatusCode.NotFound);
        JsonDocument.Parse(missingBody).RootElement.GetProperty("error").GetProperty("code").GetString().Should().Be("model_not_found");
        failed.Should().Be(HttpStatusCode.InternalServerError);
        failedBody.Should().NotContain("sk-secret");
    }

    [Fact]
    public async Task Streaming_FailureMidStream_EndsWithAnErrorEvent()
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Failing(
            new ChatResponseUpdate(ChatRole.Assistant, "partial"), new InvalidOperationException("upstream rejected key sk-secret")));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIResponses("/v1/responses", served));

        var (status, _, body) = await host.PostStream("/v1/responses", """{"model":"m","stream":true,"input":"u"}""");

        status.Should().Be(HttpStatusCode.OK, "the stream had started");
        var events = body.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        events[^1].Should().StartWith("event: error\n");
        events.Should().NotContain(e => e.StartsWith("event: response.completed", StringComparison.Ordinal));
        body.Should().Contain("partial").And.NotContain("sk-secret");
    }

    [Fact]
    public async Task ALengthLimitedReply_IsAnIncompleteResponse()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "cut o")) { FinishReason = ChatFinishReason.Length });
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIResponses("/v1/responses", served));

        var (status, body) = await host.Post("/v1/responses", """{"model":"m","input":"u"}""");

        status.Should().Be(HttpStatusCode.OK);
        var root = JsonDocument.Parse(body).RootElement;
        root.GetProperty("status").GetString().Should().Be("incomplete");
        root.GetProperty("incomplete_details").GetProperty("reason").GetString().Should().Be("max_output_tokens");
    }
}

using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using IronHive.Providers.Declarations;
using IronHive.Providers.Endpoints;
using Microsoft.Extensions.AI;
using static IronHive.Tests.Providers.Endpoints.ChatCompletionsEndpointTests;

namespace IronHive.Tests.Providers.Endpoints;

/// <summary>
/// A chat client mounted with <c>MapAnthropicMessages</c> is read back through the library's own Anthropic client (the vendor SDK
/// underneath), declared the way a host declares a provider: the two halves of the Messages wire must agree. Observed on a loopback server.
/// </summary>
public sealed class AnthropicMessagesEndpointTests
{
    private static ProviderDeclaration Declare(string origin) => new()
    {
        Id = "served",
        Wire = WireFormat.Anthropic,
        BaseUrl = origin,
        ApiKey = "placeholder",
    };

    [Fact]
    public async Task Text_RoundTripsThroughTheLibrariesOwnAnthropicClient()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "hello there"))
        {
            FinishReason = ChatFinishReason.Length,
            Usage = new UsageDetails { InputTokenCount = 11, OutputTokenCount = 4 },
        });
        await using var host = await EndpointHost.StartAsync(e => e.MapAnthropicMessages("/v1/messages", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "claude-served");
        List<ChatMessage> messages = [new(ChatRole.System, "Be terse."), new(ChatRole.User, "hi")];

        var response = await client.GetResponseAsync(messages, new ChatOptions { MaxOutputTokens = 321 }, TestContext.Current.CancellationToken);

        response.Text.Should().Be("hello there");
        response.FinishReason.Should().Be(ChatFinishReason.Length);
        response.Usage!.InputTokenCount.Should().Be(11);
        response.Usage.OutputTokenCount.Should().Be(4);
        served.LastMessages.Should().Contain(m => m.Role == ChatRole.System && m.Text.Contains("Be terse."));
        served.LastMessages.Should().Contain(m => m.Role == ChatRole.User && m.Text == "hi");
        served.LastOptions!.ModelId.Should().Be("claude-served");
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
                    [new TextContent("Let me check."), new FunctionCallContent("toolu_1", "get_weather", new Dictionary<string, object?> { ["city"] = "Seoul" })]))
                { FinishReason = ChatFinishReason.ToolCalls }
                : new ChatResponse(new ChatMessage(ChatRole.Assistant, "It is sunny in Seoul."));
        });
        await using var host = await EndpointHost.StartAsync(e => e.MapAnthropicMessages("/v1/messages", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "claude-served");
        var weather = AIFunctionFactory.CreateDeclaration("get_weather", "Weather of a city",
            JsonDocument.Parse("""{"type":"object","properties":{"city":{"type":"string"}},"required":["city"]}""").RootElement);
        var options = new ChatOptions { Tools = [weather], ToolMode = ChatToolMode.RequireAny };
        var cancellationToken = TestContext.Current.CancellationToken;

        var first = await client.GetResponseAsync("weather in Seoul?", options, cancellationToken);

        first.Text.Should().Contain("Let me check.");
        var call = first.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().Should().ContainSingle().Subject;
        call.Name.Should().Be("get_weather");
        call.CallId.Should().Be("toolu_1");
        call.Arguments!["city"]!.ToString().Should().Be("Seoul");
        first.FinishReason.Should().Be(ChatFinishReason.ToolCalls);
        served.LastOptions!.Tools.Should().ContainSingle().Which.Name.Should().Be("get_weather");
        served.LastOptions.ToolMode.Should().Be(ChatToolMode.RequireAny);

        List<ChatMessage> history =
        [
            new(ChatRole.User, "weather in Seoul?"),
            .. first.Messages,
            new(ChatRole.Tool, [new FunctionResultContent("toolu_1", "sunny, 21C")]),
        ];
        var second = await client.GetResponseAsync(history, options, cancellationToken);

        second.Text.Should().Be("It is sunny in Seoul.");
        served.LastMessages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().Should().ContainSingle().Which.CallId.Should().Be("toolu_1");
        var result = served.LastMessages.SelectMany(m => m.Contents).OfType<FunctionResultContent>().Should().ContainSingle().Subject;
        result.CallId.Should().Be("toolu_1");
        result.Result!.ToString().Should().Contain("sunny, 21C");
    }

    [Fact]
    public async Task Image_ArrivesAsData()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "a cat")));
        await using var host = await EndpointHost.StartAsync(e => e.MapAnthropicMessages("/v1/messages", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "claude-served");
        byte[] png = [137, 80, 78, 71, 13, 10, 26, 10, 1, 2, 3];

        await client.GetResponseAsync(
            [new ChatMessage(ChatRole.User, [new TextContent("what is this?"), new DataContent(png, "image/png")])],
            cancellationToken: TestContext.Current.CancellationToken);

        var image = served.LastMessages.SelectMany(m => m.Contents).OfType<DataContent>().Should().ContainSingle().Subject;
        image.MediaType.Should().Be("image/png");
        image.Data.ToArray().Should().Equal(png);
    }

    [Fact]
    public async Task Streaming_TextAndToolCall_RoundTripThroughTheLibrariesOwnAnthropicClient()
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Updates(
            new ChatResponseUpdate(ChatRole.Assistant, "Hel"),
            new ChatResponseUpdate(ChatRole.Assistant, "lo"),
            new ChatResponseUpdate(ChatRole.Assistant, [new FunctionCallContent("toolu_9", "get_weather", new Dictionary<string, object?> { ["city"] = "Busan" })]),
            new ChatResponseUpdate { Contents = [new UsageContent(new UsageDetails { InputTokenCount = 5, OutputTokenCount = 3 })] }));
        await using var host = await EndpointHost.StartAsync(e => e.MapAnthropicMessages("/v1/messages", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "claude-served");
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
        call.CallId.Should().Be("toolu_9");
        call.Arguments!["city"]!.ToString().Should().Be("Busan");
        response.FinishReason.Should().Be(ChatFinishReason.ToolCalls);
        response.Usage!.OutputTokenCount.Should().Be(3);
    }

    [Fact]
    public async Task Streaming_Framing_FollowsTheMessageEventOrder()
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Updates(
            new ChatResponseUpdate(ChatRole.Assistant, "A") { ModelId = "served-model" },
            new ChatResponseUpdate(ChatRole.Assistant, "B"),
            new ChatResponseUpdate(ChatRole.Assistant, [new FunctionCallContent("toolu_1", "f", new Dictionary<string, object?> { ["x"] = 1 })]),
            new ChatResponseUpdate(ChatRole.Assistant, "C")));
        await using var host = await EndpointHost.StartAsync(e => e.MapAnthropicMessages("/v1/messages", served));

        var (status, contentType, body) = await host.PostStream("/v1/messages",
            """{"model":"asked","max_tokens":64,"stream":true,"messages":[{"role":"user","content":"u"}]}""");

        status.Should().Be(HttpStatusCode.OK);
        contentType.Should().Be("text/event-stream");
        var events = body.Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Split('\n', 2))
            .Select(parts => (Name: parts[0]["event: ".Length..], Data: JsonDocument.Parse(parts[1]["data: ".Length..]).RootElement))
            .ToList();
        events.Select(e => e.Name).Should().Equal(
            "message_start",
            "content_block_start", "content_block_delta", "content_block_delta", "content_block_stop",
            "content_block_start", "content_block_delta", "content_block_stop",
            "content_block_start", "content_block_delta", "content_block_stop",
            "message_delta", "message_stop");
        events.Should().OnlyContain(e => e.Data.GetProperty("type").GetString() == e.Name);
        events[0].Data.GetProperty("message").GetProperty("model").GetString().Should().Be("served-model");
        events.Where(e => e.Name.StartsWith("content_block", StringComparison.Ordinal)).Select(e => e.Data.GetProperty("index").GetInt32())
            .Should().Equal(0, 0, 0, 0, 1, 1, 1, 2, 2, 2);
        events[5].Data.GetProperty("content_block").GetProperty("type").GetString().Should().Be("tool_use");
        events[6].Data.GetProperty("delta").GetProperty("partial_json").GetString().Should().Be("""{"x":1}""");
        events[^2].Data.GetProperty("delta").GetProperty("stop_reason").GetString().Should().Be("tool_use");
    }

    [Fact]
    public async Task RequestBlocks_ArePlacedWhereTheChatClientExpectsThem()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));
        await using var host = await EndpointHost.StartAsync(e => e.MapAnthropicMessages("/v1/messages", served));

        var (status, _) = await host.Post("/v1/messages", """
            {"model":"m","max_tokens":10,"system":[{"type":"text","text":"Sys A. "},{"type":"text","text":"Sys B."}],
             "stop_sequences":["END"],"top_k":7,"top_p":0.5,
             "tool_choice":{"type":"tool","name":"f","disable_parallel_tool_use":true},
             "tools":[{"name":"f","description":"d","input_schema":{"type":"object","properties":{"a":{"type":"integer"}}}}],
             "messages":[
               {"role":"user","content":"first"},
               {"role":"assistant","content":[{"type":"thinking","thinking":"hm","signature":"s"},{"type":"text","text":"calling"},{"type":"tool_use","id":"t1","name":"f","input":{"a":1}}]},
               {"role":"user","content":[{"type":"tool_result","tool_use_id":"t1","content":[{"type":"text","text":"r1"},{"type":"text","text":"r2"}],"is_error":true},{"type":"text","text":"and then?"}]}
             ]}
            """);

        status.Should().Be(HttpStatusCode.OK);
        served.LastMessages.Select(m => m.Role).Should().Equal(ChatRole.System, ChatRole.User, ChatRole.Assistant, ChatRole.Tool, ChatRole.User);
        served.LastMessages[0].Text.Should().Be("Sys A. Sys B.");
        served.LastMessages[2].Contents.Should().HaveCount(2, "the thinking block is not carried");
        var result = served.LastMessages[3].Contents.Should().ContainSingle().Which.Should().BeOfType<FunctionResultContent>().Subject;
        result.Result.Should().Be("r1r2");
        result.Exception.Should().NotBeNull("is_error is carried");
        served.LastMessages[4].Text.Should().Be("and then?");
        var options = served.LastOptions!;
        options.StopSequences.Should().Equal("END");
        options.TopK.Should().Be(7);
        options.TopP.Should().Be(0.5f);
        options.ToolMode.Should().Be(ChatToolMode.RequireSpecific("f"));
        options.AllowMultipleToolCalls.Should().BeFalse();
    }

    [Theory]
    [InlineData("""{"model":"m","messages":[{"role":"user","content":"u"}]}""", "max_tokens", "max_tokens")]
    [InlineData("""{"model":"m","max_tokens":5}""", "'messages' is required", "messages")]
    [InlineData("""{"model":"m","max_tokens":5,"messages":[{"role":"system","content":"x"}]}""", "'user' or 'assistant'", "messages[0].role")]
    [InlineData("""{"model":"m","max_tokens":5,"messages":[{"role":"user","content":[{"type":"document"}]}]}""", "'document' is not supported", "messages[0].content[0].type")]
    [InlineData("""{"model":"m","max_tokens":5,"messages":[{"role":"user","content":"u"}],"tools":[{"type":"web_search_20250305","name":"web_search"}]}""", "server tool type", "tools[0].type")]
    [InlineData("""{"model":"m","max_tokens":5,"messages":[{"role":"user","content":"u"}],"tool_choice":{"type":"sometimes"}}""", "tool_choice type", "tool_choice.type")]
    public async Task MalformedRequest_IsA400InTheWiresErrorShape(string body, string messagePart, string param)
    {
        var served = new ScriptedChatClient((_, _) => throw new InvalidOperationException("must not be called"));
        await using var host = await EndpointHost.StartAsync(e => e.MapAnthropicMessages("/v1/messages", served));

        var (status, text) = await host.Post("/v1/messages", body);

        status.Should().Be(HttpStatusCode.BadRequest);
        var root = JsonDocument.Parse(text).RootElement;
        root.GetProperty("type").GetString().Should().Be("error");
        root.GetProperty("error").GetProperty("type").GetString().Should().Be("invalid_request_error");
        root.GetProperty("error").GetProperty("message").GetString().Should().Contain(messagePart).And.Contain(param);
        served.Calls.Should().Be(0);
    }

    [Fact]
    public async Task UnknownModel_Is404_AndAClientFailure_IsA500WithoutTheUpstreamDetail()
    {
        var failing = new ScriptedChatClient((_, _) => throw new InvalidOperationException("upstream rejected key sk-secret"));
        await using var host = await EndpointHost.StartAsync(e => e.MapAnthropicMessages("/v1/messages", o =>
            o.ClientResolver = (_, model) => new ValueTask<IChatClient?>(model == "known" ? failing : null)));

        var (missing, missingBody) = await host.Post("/v1/messages", """{"model":"other","max_tokens":5,"messages":[{"role":"user","content":"u"}]}""");
        var (failed, failedBody) = await host.Post("/v1/messages", """{"model":"known","max_tokens":5,"messages":[{"role":"user","content":"u"}]}""");

        missing.Should().Be(HttpStatusCode.NotFound);
        JsonDocument.Parse(missingBody).RootElement.GetProperty("error").GetProperty("type").GetString().Should().Be("not_found_error");
        failed.Should().Be(HttpStatusCode.InternalServerError);
        JsonDocument.Parse(failedBody).RootElement.GetProperty("error").GetProperty("type").GetString().Should().Be("api_error");
        failedBody.Should().NotContain("sk-secret");
    }

    [Fact]
    public async Task Streaming_FailureMidStream_EndsWithAnErrorEvent()
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Failing(
            new ChatResponseUpdate(ChatRole.Assistant, "partial"), new InvalidOperationException("upstream rejected key sk-secret")));
        await using var host = await EndpointHost.StartAsync(e => e.MapAnthropicMessages("/v1/messages", served));

        var (status, _, body) = await host.PostStream("/v1/messages",
            """{"model":"m","max_tokens":5,"stream":true,"messages":[{"role":"user","content":"u"}]}""");

        status.Should().Be(HttpStatusCode.OK, "the stream had started");
        var events = body.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        events[^1].Should().StartWith("event: error\n");
        events.Should().NotContain(e => e.StartsWith("event: message_stop", StringComparison.Ordinal));
        body.Should().Contain("partial").And.NotContain("sk-secret");
    }
}

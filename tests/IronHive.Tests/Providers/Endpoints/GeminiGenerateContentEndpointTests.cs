using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using IronHive.Providers.Declarations;
using IronHive.Providers.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using static IronHive.Tests.Providers.Endpoints.ChatCompletionsEndpointTests;

namespace IronHive.Tests.Providers.Endpoints;

/// <summary>
/// A chat client mounted with <c>MapGeminiGenerateContent</c> is read back through the library's own Google client (the vendor SDK
/// underneath), declared the way a host declares a provider: the two halves of the Gemini wire must agree. Observed on a loopback server.
/// </summary>
public sealed class GeminiGenerateContentEndpointTests
{
    private const string Generate = "/v1beta/models/m:generateContent";

    private static ProviderDeclaration Declare(string origin) => new()
    {
        Id = "served",
        Wire = WireFormat.GoogleAI,
        BaseUrl = origin,
        ApiKey = "placeholder",
    };

    [Fact]
    public async Task Text_RoundTripsThroughTheLibrariesOwnGoogleClient()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "hello there"))
        {
            FinishReason = ChatFinishReason.Length,
            Usage = new UsageDetails { InputTokenCount = 11, OutputTokenCount = 4 },
        });
        await using var host = await EndpointHost.StartAsync(e => e.MapGeminiGenerateContent("/v1beta", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "gemini-served");
        List<ChatMessage> messages = [new(ChatRole.System, "Be terse."), new(ChatRole.User, "hi")];

        var response = await client.GetResponseAsync(messages, new ChatOptions { MaxOutputTokens = 321, Temperature = 0.5f }, TestContext.Current.CancellationToken);

        response.Text.Should().Be("hello there");
        response.FinishReason.Should().Be(ChatFinishReason.Length);
        response.Usage!.InputTokenCount.Should().Be(11);
        response.Usage.OutputTokenCount.Should().Be(4);
        served.LastMessages.Should().Contain(m => m.Role == ChatRole.System && m.Text.Contains("Be terse."));
        served.LastMessages.Should().Contain(m => m.Role == ChatRole.User && m.Text == "hi");
        served.LastOptions!.ModelId.Should().Be("gemini-served", "the model comes from the route");
        served.LastOptions.MaxOutputTokens.Should().Be(321);
        served.LastOptions.Temperature.Should().Be(0.5f);
    }

    [Fact]
    public async Task ToolCall_GoesBackToTheCaller_AndTheResultIsPairedByNameOnTheNextTurn()
    {
        var turn = 0;
        var served = new ScriptedChatClient((_, _) =>
        {
            turn++;
            return turn == 1
                ? new ChatResponse(new ChatMessage(ChatRole.Assistant,
                    [new FunctionCallContent("get_weather", "get_weather", new Dictionary<string, object?> { ["city"] = "Seoul" })]))
                : new ChatResponse(new ChatMessage(ChatRole.Assistant, "It is sunny in Seoul."));
        });
        await using var host = await EndpointHost.StartAsync(e => e.MapGeminiGenerateContent("/v1beta", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "gemini-served");
        var weather = AIFunctionFactory.CreateDeclaration("get_weather", "Weather of a city",
            JsonDocument.Parse("""{"type":"object","properties":{"city":{"type":"string"}},"required":["city"]}""").RootElement);
        var options = new ChatOptions { Tools = [weather] };
        var cancellationToken = TestContext.Current.CancellationToken;

        var first = await client.GetResponseAsync("weather in Seoul?", options, cancellationToken);

        var call = first.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().Should().ContainSingle().Subject;
        call.Name.Should().Be("get_weather");
        call.Arguments!["city"]!.ToString().Should().Be("Seoul");
        served.LastOptions!.Tools.Should().ContainSingle().Which.Name.Should().Be("get_weather");

        List<ChatMessage> history =
        [
            new(ChatRole.User, "weather in Seoul?"),
            .. first.Messages,
            new(ChatRole.Tool, [new FunctionResultContent(call.CallId, "sunny, 21C")]),
        ];
        var second = await client.GetResponseAsync(history, options, cancellationToken);

        second.Text.Should().Be("It is sunny in Seoul.");
        var seenCall = served.LastMessages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().Should().ContainSingle().Subject;
        var seenResult = served.LastMessages.SelectMany(m => m.Contents).OfType<FunctionResultContent>().Should().ContainSingle().Subject;
        seenResult.CallId.Should().Be(seenCall.CallId, "the result is paired with its call");
        seenResult.Result!.ToString().Should().Contain("sunny, 21C");
    }

    [Fact]
    public async Task Image_ArrivesAsData()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "a cat")));
        await using var host = await EndpointHost.StartAsync(e => e.MapGeminiGenerateContent("/v1beta", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "gemini-served");
        byte[] png = [137, 80, 78, 71, 13, 10, 26, 10, 1, 2, 3];

        await client.GetResponseAsync(
            [new ChatMessage(ChatRole.User, [new TextContent("what is this?"), new DataContent(png, "image/png")])],
            cancellationToken: TestContext.Current.CancellationToken);

        var image = served.LastMessages.SelectMany(m => m.Contents).OfType<DataContent>().Should().ContainSingle().Subject;
        image.MediaType.Should().Be("image/png");
        image.Data.ToArray().Should().Equal(png);
    }

    [Fact]
    public async Task Streaming_TextAndToolCall_RoundTripThroughTheLibrariesOwnGoogleClient()
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Updates(
            new ChatResponseUpdate(ChatRole.Assistant, "Hel"),
            new ChatResponseUpdate(ChatRole.Assistant, "lo"),
            new ChatResponseUpdate(ChatRole.Assistant, [new FunctionCallContent("get_weather", "get_weather", new Dictionary<string, object?> { ["city"] = "Busan" })]),
            new ChatResponseUpdate { Contents = [new UsageContent(new UsageDetails { InputTokenCount = 5, OutputTokenCount = 3 })] }));
        await using var host = await EndpointHost.StartAsync(e => e.MapGeminiGenerateContent("/v1beta", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "gemini-served");
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
        call.Name.Should().Be("get_weather");
        call.Arguments!["city"]!.ToString().Should().Be("Busan");
        response.Usage!.OutputTokenCount.Should().Be(3);
    }

    [Fact]
    public async Task Streaming_Framing_IsDataEventsWithTheClosingChunkCarryingFinishAndUsage()
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Updates(
            new ChatResponseUpdate(ChatRole.Assistant, "A") { ModelId = "served-model" },
            new ChatResponseUpdate(ChatRole.Assistant, "B"),
            new ChatResponseUpdate { Contents = [new UsageContent(new UsageDetails { InputTokenCount = 7, OutputTokenCount = 2 })] }));
        await using var host = await EndpointHost.StartAsync(e => e.MapGeminiGenerateContent("/v1beta", served));

        var (status, contentType, body) = await host.PostStream("/v1beta/models/asked:streamGenerateContent?alt=sse",
            """{"contents":[{"role":"user","parts":[{"text":"u"}]}]}""");

        status.Should().Be(HttpStatusCode.OK);
        contentType.Should().Be("text/event-stream");
        var events = body.Split("\r\n\r\n", StringSplitOptions.RemoveEmptyEntries);
        events.Should().OnlyContain(e => e.StartsWith("data: ", StringComparison.Ordinal));
        var chunks = events.Select(e => JsonDocument.Parse(e["data: ".Length..]).RootElement).ToList();
        chunks.Should().HaveCount(3, "one chunk per text delta and a closing chunk");
        string Text(JsonElement chunk) => chunk.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString()!;
        Text(chunks[0]).Should().Be("A");
        Text(chunks[1]).Should().Be("B");
        chunks[0].GetProperty("candidates")[0].TryGetProperty("finishReason", out _).Should().BeFalse();
        chunks[0].GetProperty("modelVersion").GetString().Should().Be("served-model");
        var closing = chunks[^1];
        closing.GetProperty("candidates")[0].GetProperty("finishReason").GetString().Should().Be("STOP");
        closing.GetProperty("usageMetadata").GetProperty("promptTokenCount").GetInt32().Should().Be(7);
        closing.GetProperty("usageMetadata").GetProperty("candidatesTokenCount").GetInt32().Should().Be(2);
        closing.GetProperty("usageMetadata").GetProperty("totalTokenCount").GetInt32().Should().Be(9);
        chunks.Select(c => c.GetProperty("responseId").GetString()).Distinct().Should().ContainSingle();
    }

    [Fact]
    public async Task RequestParts_ArePlacedWhereTheChatClientExpectsThem()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));
        await using var host = await EndpointHost.StartAsync(e => e.MapGeminiGenerateContent("/v1beta", served));

        var (status, _) = await host.Post(Generate, """
            {"systemInstruction":{"parts":[{"text":"Sys A. "},{"text":"Sys B."}]},
             "generationConfig":{"temperature":0.25,"topP":0.5,"topK":7,"maxOutputTokens":55,"stopSequences":["END"],"seed":9,
               "responseMimeType":"application/json","responseSchema":{"type":"OBJECT","properties":{"x":{"type":"INTEGER"}}}},
             "toolConfig":{"functionCallingConfig":{"mode":"ANY","allowedFunctionNames":["f"]}},
             "tools":[{"functionDeclarations":[{"name":"f","description":"d","parameters":{"type":"OBJECT","properties":{"a":{"type":"INTEGER"},"b":{"type":"ARRAY","items":{"type":"STRING"}}}}}]}],
             "contents":[
               {"role":"user","parts":[{"text":"first"}]},
               {"role":"model","parts":[{"text":"thinking...","thought":true},{"text":"calling"},{"functionCall":{"name":"f","args":{"a":1}}}]},
               {"role":"user","parts":[{"functionResponse":{"name":"f","response":{"result":"r1"}}}]},
               {"role":"user","parts":[{"fileData":{"mimeType":"image/png","fileUri":"https://example.com/a.png"}},{"text":"and then?"}]}
             ]}
            """);

        status.Should().Be(HttpStatusCode.OK);
        served.LastMessages.Select(m => m.Role).Should().Equal(ChatRole.System, ChatRole.User, ChatRole.Assistant, ChatRole.Tool, ChatRole.User);
        served.LastMessages[0].Text.Should().Be("Sys A. Sys B.");
        served.LastMessages[2].Contents.Should().HaveCount(2, "the thought part is not carried");
        var result = served.LastMessages[3].Contents.Should().ContainSingle().Which.Should().BeOfType<FunctionResultContent>().Subject;
        result.CallId.Should().Be("f", "without an id the name stands in on both sides");
        served.LastMessages[2].Contents.OfType<FunctionCallContent>().Single().CallId.Should().Be("f");
        served.LastMessages[4].Contents.OfType<UriContent>().Should().ContainSingle().Which.MediaType.Should().Be("image/png");
        var options = served.LastOptions!;
        options.Temperature.Should().Be(0.25f);
        options.TopP.Should().Be(0.5f);
        options.TopK.Should().Be(7);
        options.MaxOutputTokens.Should().Be(55);
        options.StopSequences.Should().Equal("END");
        options.Seed.Should().Be(9);
        options.ToolMode.Should().Be(ChatToolMode.RequireSpecific("f"));
        var schema = ((AIFunctionDeclaration)options.Tools!.Single()).JsonSchema.ToString();
        schema.Should().Contain("\"object\"").And.Contain("\"integer\"").And.Contain("\"array\"").And.NotContain("OBJECT").And.NotContain("INTEGER");
        options.ResponseFormat.Should().BeOfType<ChatResponseFormatJson>().Which.Schema!.Value.ToString().Should().Contain("\"integer\"");
    }

    [Theory]
    [InlineData("""{"model":"m"}""", "'contents' is required", "contents")]
    [InlineData("""{"contents":[{"role":"system","parts":[{"text":"x"}]}]}""", "'user' or 'model'", "contents[0].role")]
    [InlineData("""{"contents":[{"role":"user","parts":[{"executableCode":{}}]}]}""", "not supported here", "contents[0].parts[0]")]
    [InlineData("""{"contents":[{"role":"user","parts":[{"text":"u"}]}],"generationConfig":{"candidateCount":2}}""", "candidateCount must be 1", "generationConfig.candidateCount")]
    [InlineData("""{"contents":[{"role":"user","parts":[{"text":"u"}]}],"tools":[{"googleSearch":{}}]}""", "built-in tools are not served", "tools[0]")]
    [InlineData("""{"contents":[{"role":"user","parts":[{"text":"u"}]}],"toolConfig":{"functionCallingConfig":{"mode":"SOMETIMES"}}}""", "'SOMETIMES' is not supported", "functionCallingConfig.mode")]
    [InlineData("""not json""", "not valid JSON", "")]
    public async Task MalformedRequest_IsA400InTheWiresErrorShape(string body, string messagePart, string param)
    {
        var served = new ScriptedChatClient((_, _) => throw new InvalidOperationException("must not be called"));
        await using var host = await EndpointHost.StartAsync(e => e.MapGeminiGenerateContent("/v1beta", served));

        var (status, text) = await host.Post(Generate, body);

        status.Should().Be(HttpStatusCode.BadRequest);
        var error = JsonDocument.Parse(text).RootElement.GetProperty("error");
        error.GetProperty("code").GetInt32().Should().Be(400);
        error.GetProperty("status").GetString().Should().Be("INVALID_ARGUMENT");
        error.GetProperty("message").GetString().Should().Contain(messagePart);
        if (param.Length > 0)
        {
            error.GetProperty("message").GetString().Should().Contain(param);
        }

        served.Calls.Should().Be(0);
    }

    [Fact]
    public async Task UnknownModel_Is404_AndAClientFailure_IsA500WithoutTheUpstreamDetail()
    {
        var failing = new ScriptedChatClient((_, _) => throw new InvalidOperationException("upstream rejected key sk-secret"));
        await using var host = await EndpointHost.StartAsync(e => e.MapGeminiGenerateContent("/v1beta", o =>
            o.ClientResolver = (_, model) => new ValueTask<IChatClient?>(model == "known" ? failing : null)));
        const string body = """{"contents":[{"role":"user","parts":[{"text":"u"}]}]}""";

        var (missing, missingBody) = await host.Post("/v1beta/models/other:generateContent", body);
        var (failed, failedBody) = await host.Post("/v1beta/models/known:generateContent", body);

        missing.Should().Be(HttpStatusCode.NotFound);
        JsonDocument.Parse(missingBody).RootElement.GetProperty("error").GetProperty("status").GetString().Should().Be("NOT_FOUND");
        failed.Should().Be(HttpStatusCode.InternalServerError);
        JsonDocument.Parse(failedBody).RootElement.GetProperty("error").GetProperty("status").GetString().Should().Be("INTERNAL");
        failedBody.Should().NotContain("sk-secret");
    }

    [Fact]
    public async Task Streaming_FailureMidStream_EndsWithAnErrorChunk()
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Failing(
            new ChatResponseUpdate(ChatRole.Assistant, "partial"), new InvalidOperationException("upstream rejected key sk-secret")));
        await using var host = await EndpointHost.StartAsync(e => e.MapGeminiGenerateContent("/v1beta", served));

        var (status, _, body) = await host.PostStream("/v1beta/models/m:streamGenerateContent?alt=sse",
            """{"contents":[{"role":"user","parts":[{"text":"u"}]}]}""");

        status.Should().Be(HttpStatusCode.OK, "the stream had started");
        var events = body.Split("\r\n\r\n", StringSplitOptions.RemoveEmptyEntries);
        JsonDocument.Parse(events[^1]["data: ".Length..]).RootElement.GetProperty("error").GetProperty("status").GetString().Should().Be("INTERNAL");
        body.Should().Contain("partial").And.NotContain("sk-secret");
    }

    [Fact]
    public void Mapping_WithoutAClientResolver_Throws()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        var app = builder.Build();

        var act = () => app.MapGeminiGenerateContent("/v1beta", _ => { });

        act.Should().Throw<InvalidOperationException>().WithMessage("*ClientResolver*");
    }
}

using System.Net;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using IronHive.Providers.Declarations;
using IronHive.Providers.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace IronHive.Tests.Providers.Endpoints;

/// <summary>
/// A chat client mounted with <see cref="ProviderEndpointRouteBuilderExtensions.MapOpenAIChatCompletions(IEndpointRouteBuilder, string, Action{ProviderEndpointOptions})"/>
/// is read back through the library's own Chat Completions client, declared the way a host declares a provider: the two
/// halves of the wire must agree on text, tool calls, finish reason and usage. Observed on a loopback server.
/// </summary>
public sealed class ChatCompletionsEndpointTests
{
    private static ProviderDeclaration Declare(string origin) => new()
    {
        Id = "served",
        Wire = WireFormat.OpenAIChatCompletions,
        BaseUrl = origin + "/v1",
        ApiKey = "placeholder",
    };

    [Fact]
    public async Task Text_RoundTripsThroughTheLibrariesOwnClient()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "hello there"))
        {
            FinishReason = ChatFinishReason.Stop,
            Usage = new UsageDetails { InputTokenCount = 11, OutputTokenCount = 4, TotalTokenCount = 15 },
        });
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));

        var client = ProviderClientFactory.Create(Declare(host.Origin), "model-a");
        var response = await client.GetResponseAsync("hi", cancellationToken: TestContext.Current.CancellationToken);

        response.Text.Should().Be("hello there");
        response.FinishReason.Should().Be(ChatFinishReason.Stop);
        response.Usage!.InputTokenCount.Should().Be(11);
        response.Usage.OutputTokenCount.Should().Be(4);
        served.LastMessages.Should().ContainSingle(m => m.Role == ChatRole.User && m.Text == "hi");
        served.LastOptions!.ModelId.Should().Be("model-a");
    }

    [Fact]
    public async Task ToolCall_GoesBackToTheCaller_AndTheResultComesInOnTheNextTurn()
    {
        var turn = 0;
        var served = new ScriptedChatClient((messages, _) =>
        {
            turn++;
            return turn == 1
                ? new ChatResponse(new ChatMessage(ChatRole.Assistant,
                    [new FunctionCallContent("call_1", "get_weather", new Dictionary<string, object?> { ["city"] = "Seoul" })]))
                { FinishReason = ChatFinishReason.ToolCalls }
                : new ChatResponse(new ChatMessage(ChatRole.Assistant, "It is sunny in Seoul."));
        });
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "model-a");
        var weather = AIFunctionFactory.CreateDeclaration("get_weather", "Weather of a city",
            JsonDocument.Parse("""{"type":"object","properties":{"city":{"type":"string"}},"required":["city"]}""").RootElement);
        var options = new ChatOptions { Tools = [weather] };
        var cancellationToken = TestContext.Current.CancellationToken;

        var first = await client.GetResponseAsync("weather in Seoul?", options, cancellationToken);

        var call = first.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().Should().ContainSingle().Subject;
        call.Name.Should().Be("get_weather");
        call.CallId.Should().Be("call_1");
        call.Arguments!["city"]!.ToString().Should().Be("Seoul");
        first.FinishReason.Should().Be(ChatFinishReason.ToolCalls);
        served.LastOptions!.Tools.Should().ContainSingle().Which.Name.Should().Be("get_weather");
        served.LastOptions.Tools![0].Should().BeAssignableTo<AIFunctionDeclaration>().Which.JsonSchema.ToString().Should().Contain("city");

        List<ChatMessage> history =
        [
            new(ChatRole.User, "weather in Seoul?"),
            .. first.Messages,
            new(ChatRole.Tool, [new FunctionResultContent("call_1", "sunny, 21C")]),
        ];
        var second = await client.GetResponseAsync(history, options, cancellationToken);

        second.Text.Should().Be("It is sunny in Seoul.");
        var seenCall = served.LastMessages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().Should().ContainSingle().Subject;
        seenCall.CallId.Should().Be("call_1");
        var seenResult = served.LastMessages.SelectMany(m => m.Contents).OfType<FunctionResultContent>().Should().ContainSingle().Subject;
        seenResult.CallId.Should().Be("call_1");
        seenResult.Result!.ToString().Should().Be("sunny, 21C");
    }

    [Fact]
    public async Task Image_ArrivesAsData()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "a cat")));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));
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
    public async Task ResponseFormatAndSampling_ReachTheClientOptions()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "{}")));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));

        var (status, _) = await host.Post("/v1/chat/completions", """
            {"model":"m","messages":[{"role":"system","content":"s"},{"role":"user","content":"u"}],
             "temperature":0.25,"top_p":0.5,"max_completion_tokens":77,"stop":["END","STOP"],"seed":9,
             "tool_choice":"required","parallel_tool_calls":false,
             "tools":[{"type":"function","function":{"name":"f","parameters":{"type":"object","properties":{}}}}],
             "response_format":{"type":"json_schema","json_schema":{"name":"answer","schema":{"type":"object","properties":{"x":{"type":"integer"}}}}}}
            """);

        status.Should().Be(HttpStatusCode.OK);
        var options = served.LastOptions!;
        options.Temperature.Should().Be(0.25f);
        options.TopP.Should().Be(0.5f);
        options.MaxOutputTokens.Should().Be(77);
        options.StopSequences.Should().Equal("END", "STOP");
        options.Seed.Should().Be(9);
        options.ToolMode.Should().Be(ChatToolMode.RequireAny);
        options.AllowMultipleToolCalls.Should().BeFalse();
        options.ResponseFormat.Should().BeOfType<ChatResponseFormatJson>().Which.SchemaName.Should().Be("answer");
        served.LastMessages.Select(m => m.Role).Should().Equal(ChatRole.System, ChatRole.User);
    }

    [Fact]
    public async Task ConfigureChatOptions_CanClampWhatTheCallerAsked()
    {
        var served = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", o =>
        {
            o.ClientResolver = (_, _) => new ValueTask<IChatClient?>(served);
            o.ConfigureChatOptions = (_, chat) =>
            {
                chat.MaxOutputTokens = Math.Min(chat.MaxOutputTokens ?? 100, 100);
                chat.Tools = null;
                return ValueTask.CompletedTask;
            };
        }));

        await host.Post("/v1/chat/completions", """
            {"model":"m","messages":[{"role":"user","content":"u"}],"max_tokens":5000,
             "tools":[{"type":"function","function":{"name":"f"}}]}
            """);

        served.LastOptions!.MaxOutputTokens.Should().Be(100);
        served.LastOptions.Tools.Should().BeNull();
    }

    [Fact]
    public async Task Resolver_ChoosesTheClientByTheRequestedModel_AndNullIsModelNotFound()
    {
        var a = new ScriptedChatClient((_, _) => new ChatResponse(new ChatMessage(ChatRole.Assistant, "from a")));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", o =>
            o.ClientResolver = (_, model) => new ValueTask<IChatClient?>(model == "a" ? a : null)));

        var (ok, okBody) = await host.Post("/v1/chat/completions", """{"model":"a","messages":[{"role":"user","content":"u"}]}""");
        var (missing, missingBody) = await host.Post("/v1/chat/completions", """{"model":"b","messages":[{"role":"user","content":"u"}]}""");

        ok.Should().Be(HttpStatusCode.OK);
        JsonDocument.Parse(okBody).RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString().Should().Be("from a");
        missing.Should().Be(HttpStatusCode.NotFound);
        var error = JsonDocument.Parse(missingBody).RootElement.GetProperty("error");
        error.GetProperty("code").GetString().Should().Be("model_not_found");
        error.GetProperty("param").GetString().Should().Be("model");
    }

    [Theory]
    [InlineData("""not json""", "not valid JSON", null)]
    [InlineData("""[1,2]""", "must be a JSON object", null)]
    [InlineData("""{"model":"m"}""", "'messages' is required", "messages")]
    [InlineData("""{"model":"m","messages":[{"role":"user","content":"u"}],"n":2}""", "n must be 1", "n")]
    [InlineData("""{"model":"m","messages":[{"role":"wizard","content":"u"}]}""", "'wizard' is not supported", "messages[0].role")]
    [InlineData("""{"model":"m","messages":[{"role":"tool","content":"x"}]}""", "needs a 'tool_call_id'", "messages[0].tool_call_id")]
    [InlineData("""{"model":"m","messages":[{"role":"user","content":[{"type":"image_url","image_url":{"url":"data:image/png;base64,@@"}}]}]}""", "not a valid data URI", "messages[0].content[0].image_url.url")]
    [InlineData("""{"model":"m","messages":[{"role":"user","content":"u"}],"tools":[{"type":"retrieval"}]}""", "Only tools of type 'function'", "tools[0]")]
    public async Task MalformedRequest_IsA400InTheWiresErrorShape(string body, string messagePart, string? param)
    {
        var served = new ScriptedChatClient((_, _) => throw new InvalidOperationException("must not be called"));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));

        var (status, text) = await host.Post("/v1/chat/completions", body);

        status.Should().Be(HttpStatusCode.BadRequest);
        var error = JsonDocument.Parse(text).RootElement.GetProperty("error");
        error.GetProperty("type").GetString().Should().Be("invalid_request_error");
        error.GetProperty("message").GetString().Should().Contain(messagePart);
        (error.GetProperty("param").ValueKind == JsonValueKind.Null ? null : error.GetProperty("param").GetString()).Should().Be(param);
        served.Calls.Should().Be(0);
    }

    [Fact]
    public async Task ClientFailure_IsAProviderShaped500_WithoutTheUpstreamDetailByDefault()
    {
        var served = new ScriptedChatClient((_, _) => throw new InvalidOperationException("upstream said: key sk-secret rejected by https://internal.example"));
        await using var closed = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));
        await using var open = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", o =>
        {
            o.ClientResolver = (_, _) => new ValueTask<IChatClient?>(served);
            o.ExposeErrorDetails = true;
        }));
        const string body = """{"model":"m","messages":[{"role":"user","content":"u"}]}""";

        var (status, text) = await closed.Post("/v1/chat/completions", body);
        var (openStatus, openText) = await open.Post("/v1/chat/completions", body);

        status.Should().Be(HttpStatusCode.InternalServerError);
        JsonDocument.Parse(text).RootElement.GetProperty("error").GetProperty("type").GetString().Should().Be("server_error");
        text.Should().NotContain("sk-secret").And.NotContain("internal.example");
        openStatus.Should().Be(HttpStatusCode.InternalServerError);
        openText.Should().Contain("sk-secret");
    }

    [Fact]
    public async Task UpstreamRateLimit_IsA429()
    {
        var served = new ScriptedChatClient((_, _) => throw new HttpRequestException("slow down", null, HttpStatusCode.TooManyRequests));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));

        var (status, text) = await host.Post("/v1/chat/completions", """{"model":"m","messages":[{"role":"user","content":"u"}]}""");

        status.Should().Be((HttpStatusCode)429);
        JsonDocument.Parse(text).RootElement.GetProperty("error").GetProperty("code").GetString().Should().Be("rate_limit_exceeded");
    }

    [Fact]
    public async Task OversizedBody_Is413()
    {
        var served = new ScriptedChatClient((_, _) => throw new InvalidOperationException("must not be called"));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", o =>
        {
            o.ClientResolver = (_, _) => new ValueTask<IChatClient?>(served);
            o.MaxRequestBodyBytes = 64;
        }));

        var (status, _) = await host.Post("/v1/chat/completions",
            """{"model":"m","messages":[{"role":"user","content":"a message that is longer than sixty-four bytes in total"}]}""");

        status.Should().Be(HttpStatusCode.RequestEntityTooLarge);
        served.Calls.Should().Be(0);
    }

    [Fact]
    public async Task CallerAbort_CancelsTheChatClientCall()
    {
        var reached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var served = new GatedChatClient(reached, cancelled);
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));
        using var abort = new CancellationTokenSource();

        var request = host.PostCancellableAsync("/v1/chat/completions", """{"model":"m","messages":[{"role":"user","content":"u"}]}""", abort.Token);
        await reached.Task.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
        abort.Cancel();

        await cancelled.Task.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
        await request.Invoking(r => r).Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Streaming_TextAndFinishReason_RoundTripThroughTheLibrariesOwnClient()
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Updates(
            new ChatResponseUpdate(ChatRole.Assistant, "Hel"),
            new ChatResponseUpdate(ChatRole.Assistant, "lo "),
            new ChatResponseUpdate(ChatRole.Assistant, "world"),
            new ChatResponseUpdate { FinishReason = ChatFinishReason.Length }));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "model-a");

        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in client.GetStreamingResponseAsync("hi", cancellationToken: TestContext.Current.CancellationToken))
        {
            updates.Add(update);
        }

        var response = updates.ToChatResponse();
        response.Text.Should().Be("Hello world");
        updates.Count(u => u.Contents.OfType<TextContent>().Any(t => t.Text.Length > 0)).Should().Be(3, "each text delta is its own event");
        response.FinishReason.Should().Be(ChatFinishReason.Length);
        served.LastMessages.Should().ContainSingle(m => m.Role == ChatRole.User && m.Text == "hi");
    }

    [Fact]
    public async Task Streaming_ToolCall_ArrivesAsAToolCallDelta()
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Updates(
            new ChatResponseUpdate(ChatRole.Assistant, [new FunctionCallContent("call_7", "get_weather", new Dictionary<string, object?> { ["city"] = "Busan" })])));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));
        var client = ProviderClientFactory.Create(Declare(host.Origin), "model-a");
        var weather = AIFunctionFactory.CreateDeclaration("get_weather", "Weather of a city",
            JsonDocument.Parse("""{"type":"object","properties":{"city":{"type":"string"}}}""").RootElement);

        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in client.GetStreamingResponseAsync("weather in Busan?", new ChatOptions { Tools = [weather] }, TestContext.Current.CancellationToken))
        {
            updates.Add(update);
        }

        var response = updates.ToChatResponse();
        var call = response.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().Should().ContainSingle().Subject;
        call.CallId.Should().Be("call_7");
        call.Name.Should().Be("get_weather");
        call.Arguments!["city"]!.ToString().Should().Be("Busan");
        response.FinishReason.Should().Be(ChatFinishReason.ToolCalls, "a stream with a tool call and no stated reason finishes as tool_calls");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Streaming_Framing_IsServerSentEventsEndingInDone_WithUsageOnlyWhenAsked(bool includeUsage)
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Updates(
            new ChatResponseUpdate(ChatRole.Assistant, "A") { ModelId = "served-model" },
            new ChatResponseUpdate(ChatRole.Assistant, "B"),
            new ChatResponseUpdate { Contents = [new UsageContent(new UsageDetails { InputTokenCount = 7, OutputTokenCount = 2 })] }));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));

        var (status, contentType, body) = await host.PostStream("/v1/chat/completions",
            $$"""{"model":"asked","stream":true,"stream_options":{"include_usage":{{(includeUsage ? "true" : "false")}}},"messages":[{"role":"user","content":"u"}]}""");

        status.Should().Be(HttpStatusCode.OK);
        contentType.Should().Be("text/event-stream");
        var events = body.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        events.Should().OnlyContain(e => e.StartsWith("data: ", StringComparison.Ordinal));
        events[^1].Should().Be("data: [DONE]");
        var chunks = events[..^1].Select(e => JsonDocument.Parse(e["data: ".Length..]).RootElement).ToList();
        chunks.Should().OnlyContain(c => c.GetProperty("object").GetString() == "chat.completion.chunk");
        chunks.Select(c => c.GetProperty("id").GetString()).Distinct().Should().ContainSingle("one id for the whole stream");
        chunks[0].GetProperty("model").GetString().Should().Be("served-model");
        chunks[0].GetProperty("choices")[0].GetProperty("delta").GetProperty("role").GetString().Should().Be("assistant");
        var text = string.Concat(chunks.Where(c => c.GetProperty("choices").GetArrayLength() > 0)
            .Select(c => c.GetProperty("choices")[0].GetProperty("delta"))
            .Where(d => d.TryGetProperty("content", out _)).Select(d => d.GetProperty("content").GetString()));
        text.Should().Be("AB");
        var finish = chunks.Where(c => c.GetProperty("choices").GetArrayLength() > 0).Select(c => c.GetProperty("choices")[0]).Last();
        finish.GetProperty("finish_reason").GetString().Should().Be("stop");
        var usageChunks = chunks.Where(c => c.TryGetProperty("usage", out _)).ToList();
        if (includeUsage)
        {
            var usageChunk = usageChunks.Should().ContainSingle().Subject;
            usageChunk.GetProperty("choices").GetArrayLength().Should().Be(0);
            usageChunk.GetProperty("usage").GetProperty("prompt_tokens").GetInt32().Should().Be(7);
            usageChunk.GetProperty("usage").GetProperty("completion_tokens").GetInt32().Should().Be(2);
            usageChunk.GetProperty("usage").GetProperty("total_tokens").GetInt32().Should().Be(9);
        }
        else
        {
            usageChunks.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Streaming_FailureBeforeTheFirstUpdate_IsAnOrdinaryErrorResponse()
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Failing(null, new InvalidOperationException("upstream rejected key sk-secret")));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));

        var (status, contentType, body) = await host.PostStream("/v1/chat/completions",
            """{"model":"m","stream":true,"messages":[{"role":"user","content":"u"}]}""");

        status.Should().Be(HttpStatusCode.InternalServerError);
        contentType.Should().Be("application/json");
        JsonDocument.Parse(body).RootElement.GetProperty("error").GetProperty("type").GetString().Should().Be("server_error");
        body.Should().NotContain("sk-secret");
    }

    [Fact]
    public async Task Streaming_FailureMidStream_EndsWithAnErrorEventAndNoDone()
    {
        var served = new ScriptedStreamingChatClient((_, _, _) => Failing(
            new ChatResponseUpdate(ChatRole.Assistant, "partial"), new InvalidOperationException("upstream rejected key sk-secret")));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));

        var (status, contentType, body) = await host.PostStream("/v1/chat/completions",
            """{"model":"m","stream":true,"messages":[{"role":"user","content":"u"}]}""");

        status.Should().Be(HttpStatusCode.OK, "the stream had started");
        contentType.Should().Be("text/event-stream");
        var events = body.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        events.Should().NotContain("data: [DONE]");
        var last = JsonDocument.Parse(events[^1]["data: ".Length..]).RootElement;
        last.GetProperty("error").GetProperty("type").GetString().Should().Be("server_error");
        body.Should().Contain("partial").And.NotContain("sk-secret");
    }

    [Fact]
    public async Task Streaming_CallerAbort_CancelsTheChatClientStream()
    {
        var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var served = new ScriptedStreamingChatClient((_, _, token) => Hanging(cancelled, token));
        await using var host = await EndpointHost.StartAsync(e => e.MapOpenAIChatCompletions("/v1/chat/completions", served));
        using var abort = new CancellationTokenSource();

        var reading = host.ReadFirstEventThenAbort("/v1/chat/completions",
            """{"model":"m","stream":true,"messages":[{"role":"user","content":"u"}]}""", abort);

        await cancelled.Task.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
        (await reading).Should().StartWith("data: ");
    }

    internal static async IAsyncEnumerable<ChatResponseUpdate> Updates(params ChatResponseUpdate[] updates)
    {
        foreach (var update in updates)
        {
            yield return update;
            await Task.Yield();
        }
    }

    internal static async IAsyncEnumerable<ChatResponseUpdate> Failing(ChatResponseUpdate? first, Exception failure)
    {
        await Task.Yield();
        if (first is not null)
        {
            yield return first;
        }

        throw failure;
    }

    internal static async IAsyncEnumerable<ChatResponseUpdate> Hanging(
        TaskCompletionSource cancelled, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken token)
    {
        yield return new ChatResponseUpdate(ChatRole.Assistant, "first");
        try
        {
            await Task.Delay(Timeout.Infinite, token);
        }
        catch (OperationCanceledException)
        {
            cancelled.TrySetResult();
            throw;
        }
    }

    [Fact]
    public void Mapping_WithoutAClientResolver_Throws()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        var app = builder.Build();

        var act = () => app.MapOpenAIChatCompletions("/v1/chat/completions", _ => { });

        act.Should().Throw<InvalidOperationException>().WithMessage("*ClientResolver*");
    }

    internal sealed class ScriptedChatClient(Func<IEnumerable<ChatMessage>, ChatOptions?, ChatResponse> script) : IChatClient
    {
        public List<ChatMessage> LastMessages { get; private set; } = [];
        public ChatOptions? LastOptions { get; private set; }
        public int Calls { get; private set; }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            Calls++;
            LastMessages = [.. messages];
            LastOptions = options;
            return Task.FromResult(script(LastMessages, options));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }

    internal sealed class ScriptedStreamingChatClient(
        Func<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken, IAsyncEnumerable<ChatResponseUpdate>> script) : IChatClient
    {
        public List<ChatMessage> LastMessages { get; private set; } = [];

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            LastMessages = [.. messages];
            return script(LastMessages, options, cancellationToken);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }

    private sealed class GatedChatClient(TaskCompletionSource reached, TaskCompletionSource cancelled) : IChatClient
    {
        public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            reached.TrySetResult();
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                cancelled.TrySetResult();
                throw;
            }

            return new ChatResponse();
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }
}

/// <summary>An ASP.NET host on a loopback port, started for one test.</summary>
internal sealed class EndpointHost : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _http;

    private EndpointHost(WebApplication app, string origin)
    {
        _app = app;
        Origin = origin;
        _http = new HttpClient { BaseAddress = new Uri(origin) };
    }

    public string Origin { get; }

    public static async Task<EndpointHost> StartAsync(Action<IEndpointRouteBuilder> map)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.None);
        if (Environment.GetEnvironmentVariable("ENDPOINT_TEST_LOG") == "1")
        {
            // Opt-in: shows what the server threw when a client reports "the response ended prematurely".
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Error);
        }

        var app = builder.Build();
        map(app);
        await app.StartAsync();
        return new EndpointHost(app, app.Urls.First());
    }

    public async Task<(HttpStatusCode Status, string? ContentType, string Body)> PostStream(string path, string json)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        return (response.StatusCode, response.Content.Headers.ContentType?.MediaType, await response.Content.ReadAsStringAsync(cancellationToken));
    }

    /// <summary>Reads the first server-sent event, then drops the connection; returns that event.</summary>
    public async Task<string> ReadFirstEventThenAbort(string path, string json, CancellationTokenSource abort)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, abort.Token);
        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(abort.Token));
        string? first = null;
        while (first is null || first.Length == 0)
        {
            first = await reader.ReadLineAsync(abort.Token);
        }

        await abort.CancelAsync();
        return first;
    }

    public Task<(HttpStatusCode Status, string Body)> Post(string path, string json) =>
        PostCancellableAsync(path, json, TestContext.Current.CancellationToken);

    public async Task<(HttpStatusCode Status, string Body)> PostCancellableAsync(string path, string json, CancellationToken cancellationToken)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _http.PostAsync(path, content, cancellationToken);
        return (response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
    }

    public async ValueTask DisposeAsync()
    {
        _http.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}

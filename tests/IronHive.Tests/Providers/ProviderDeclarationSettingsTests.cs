using System.Diagnostics;
using System.Text.Json;
using AwesomeAssertions;
using IronHive.Providers.Declarations;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Tests.Providers.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;

namespace IronHive.Tests.Providers;

/// <summary>
/// <see cref="ServerSettings"/> on a declaration reach the adapter that owns them - observed on a loopback server, not read off the
/// config - the model's value replaces the provider's per property, and a setting the wire cannot carry is refused by name.
/// </summary>
public sealed class ProviderDeclarationSettingsTests
{
    private const string Reply =
        """{"id":"x","object":"chat.completion","created":1,"model":"m","choices":[{"index":0,"message":{"role":"assistant","content":"ok"},"finish_reason":"stop"}],"usage":{"prompt_tokens":1,"completion_tokens":1,"total_tokens":2}}""";

    private static ProviderDeclaration Chat(string origin, ServerSettings? settings = null, IDictionary<string, ModelDeclaration>? models = null) => new()
    {
        Id = "srv",
        Wire = WireFormat.OpenAIChatCompletions,
        BaseUrl = origin + "/v1",
        ApiKey = "k",
        Settings = settings,
        Models = models,
    };

    private static async Task<EndpointHost> CaptureAsync(List<JsonElement> bodies, Func<HttpContext, Task>? respond = null, string path = "/v1/chat/completions") =>
        await EndpointHost.StartAsync(e => e.MapPost(path, async (HttpContext ctx) =>
        {
            using var document = await JsonDocument.ParseAsync(ctx.Request.Body);
            bodies.Add(document.RootElement.Clone());
            if (respond is not null)
            {
                await respond(ctx);
                return;
            }

            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(Reply);
        }));

    [Fact]
    public async Task TokenLimitParameter_ChangesTheSpellingOnTheWire()
    {
        var bodies = new List<JsonElement>();
        await using var host = await CaptureAsync(bodies);
        var options = new ChatOptions { MaxOutputTokens = 50 };
        var cancellationToken = TestContext.Current.CancellationToken;

        await ProviderClientFactory.Create(Chat(host.Origin), "m").GetResponseAsync("hi", options, cancellationToken);
        await ProviderClientFactory.Create(Chat(host.Origin, new ServerSettings { TokenLimitParameter = TokenLimitParameter.MaxTokens }), "m")
            .GetResponseAsync("hi", options, cancellationToken);

        bodies[0].TryGetProperty("max_completion_tokens", out _).Should().BeTrue("the default spelling is unchanged");
        bodies[0].TryGetProperty("max_tokens", out _).Should().BeFalse();
        bodies[1].GetProperty("max_tokens").GetInt32().Should().Be(50);
        bodies[1].TryGetProperty("max_completion_tokens", out _).Should().BeFalse();
    }

    [Fact]
    public async Task InstructionsPlacement_ChangesTheMessageLayout()
    {
        var bodies = new List<JsonElement>();
        await using var host = await CaptureAsync(bodies);
        var options = new ChatOptions { Instructions = "PER-REQUEST-INSTRUCTIONS" };
        List<ChatMessage> messages = [new(ChatRole.System, "You are terse."), new(ChatRole.User, "hi")];
        var cancellationToken = TestContext.Current.CancellationToken;

        await ProviderClientFactory.Create(Chat(host.Origin), "m").GetResponseAsync(messages, options, cancellationToken);
        await ProviderClientFactory.Create(Chat(host.Origin, new ServerSettings { InstructionsPlacement = InstructionsPlacement.LeadingUserTurn }), "m")
            .GetResponseAsync(messages, options, cancellationToken);

        static IEnumerable<(string Role, string Text)> Layout(JsonElement body) =>
            body.GetProperty("messages").EnumerateArray().Select(m => (m.GetProperty("role").GetString()!, m.GetProperty("content").ToString()));

        Layout(bodies[0]).Should().Contain(m => m.Role == "system" && m.Text.Contains("PER-REQUEST-INSTRUCTIONS"), "SystemAppend is the default");
        Layout(bodies[1]).Should().NotContain(m => m.Role == "system" && m.Text.Contains("PER-REQUEST-INSTRUCTIONS"));
        Layout(bodies[1]).Should().Contain(m => m.Role == "user" && m.Text.Contains("PER-REQUEST-INSTRUCTIONS"));
    }

    [Fact]
    public async Task ModelSettings_ReplaceTheProvidersPerProperty_AndTheRestFallThrough()
    {
        var bodies = new List<JsonElement>();
        await using var host = await CaptureAsync(bodies);
        var declaration = Chat(host.Origin,
            new ServerSettings { TokenLimitParameter = TokenLimitParameter.MaxTokens, InstructionsPlacement = InstructionsPlacement.LeadingUserTurn },
            new Dictionary<string, ModelDeclaration>
            {
                ["modern"] = new() { Settings = new ServerSettings { TokenLimitParameter = TokenLimitParameter.MaxCompletionTokens } },
            });
        var options = new ChatOptions { MaxOutputTokens = 9, Instructions = "PER-REQUEST-INSTRUCTIONS" };
        var cancellationToken = TestContext.Current.CancellationToken;

        await ProviderClientFactory.Create(declaration, "legacy").GetResponseAsync("hi", options, cancellationToken);
        await ProviderClientFactory.Create(declaration, "modern").GetResponseAsync("hi", options, cancellationToken);

        bodies[0].TryGetProperty("max_tokens", out _).Should().BeTrue("the provider's value applies to a model without its own");
        bodies[1].TryGetProperty("max_completion_tokens", out _).Should().BeTrue("the model's value replaces the provider's");
        bodies[1].TryGetProperty("max_tokens", out _).Should().BeFalse();
        bodies[1].GetProperty("messages").EnumerateArray().Where(m => m.GetProperty("role").GetString() == "system")
            .Should().OnlyContain(m => !m.GetProperty("content").ToString().Contains("PER-REQUEST-INSTRUCTIONS"), "the placement the model did not state falls through to the provider's");
    }

    [Fact]
    public async Task StreamIdleTimeout_EndsAStalledStream()
    {
        var bodies = new List<JsonElement>();
        await using var host = await CaptureAsync(bodies, async ctx =>
        {
            ctx.Response.ContentType = "text/event-stream";
            await ctx.Response.WriteAsync("""data: {"id":"x","object":"chat.completion.chunk","created":1,"model":"m","choices":[{"index":0,"delta":{"role":"assistant","content":"par"},"finish_reason":null}]}""" + "\n\n");
            await ctx.Response.Body.FlushAsync();
            await Task.Delay(Timeout.Infinite, ctx.RequestAborted);
        });
        var client = ProviderClientFactory.Create(Chat(host.Origin, new ServerSettings { StreamIdleTimeout = TimeSpan.FromMilliseconds(300) }), "m");
        var clock = Stopwatch.StartNew();

        var read = async () =>
        {
            // Without the setting the stream never ends; the cancellation (an OperationCanceledException, not a TimeoutException) turns that into a red test.
            using var cap = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            cap.CancelAfter(TimeSpan.FromSeconds(20));
            await foreach (var _ in client.GetStreamingResponseAsync("hi", cancellationToken: cap.Token))
            {
            }
        };

        await read.Should().ThrowAsync<TimeoutException>();
        clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(20));
    }

    [Fact]
    public async Task MaxRetries_IsHonouredWhereTheSdkRetries()
    {
        var bodies = new List<JsonElement>();
        await using var host = await CaptureAsync(bodies, async ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await ctx.Response.WriteAsync("busy");
        }, "/v1/responses");
        var cancellationToken = TestContext.Current.CancellationToken;
        ProviderDeclaration Responses(ServerSettings settings) => Chat(host.Origin, settings) with { Wire = WireFormat.OpenAIResponses };

        var once = async () => await ProviderClientFactory.Create(Responses(new ServerSettings { MaxRetries = 0 }), "m").GetResponseAsync("hi", cancellationToken: cancellationToken);
        await once.Should().ThrowAsync<Exception>();
        var afterNoRetries = bodies.Count;
        var retried = async () => await ProviderClientFactory.Create(Responses(new ServerSettings { MaxRetries = 1 }), "m").GetResponseAsync("hi", cancellationToken: cancellationToken);
        await retried.Should().ThrowAsync<Exception>();

        afterNoRetries.Should().Be(1, "MaxRetries 0 sends one request");
        (bodies.Count - afterNoRetries).Should().Be(2, "MaxRetries 1 sends the request and one retry");
    }

    [Fact]
    public void Settings_CreateAClientOnEveryWire()
    {
        var timeouts = new ServerSettings
        {
            ConnectTimeout = TimeSpan.FromSeconds(2),
            Timeout = TimeSpan.FromMinutes(5),
            StreamIdleTimeout = TimeSpan.FromMinutes(1),
        };
        foreach (var wire in Enum.GetValues<WireFormat>())
        {
            var declaration = new ProviderDeclaration { Id = "srv", Wire = wire, BaseUrl = "http://127.0.0.1:1/v1", ApiKey = "k", Settings = timeouts };

            var create = () => ProviderClientFactory.Create(declaration, "m").Dispose();

            create.Should().NotThrow($"timeouts apply on the {wire} wire");
        }
    }

    [Theory]
    [InlineData(WireFormat.Anthropic, "InstructionsPlacement", "Settings.InstructionsPlacement")]
    [InlineData(WireFormat.GoogleAI, "TokenLimitParameter", "Settings.TokenLimitParameter")]
    [InlineData(WireFormat.OpenAIResponses, "CarryImageToolResultsAsUserMessage", "Settings.CarryImageToolResultsAsUserMessage")]
    [InlineData(WireFormat.GoogleAI, "MaxRetries", "Settings.MaxRetries")]
    [InlineData(WireFormat.OpenAIChatCompletions, "MaxRetries", "Settings.MaxRetries")]
    public void ASettingTheWireHasNoHomeFor_IsRefusedByName(WireFormat wire, string setting, string expected)
    {
        var settings = setting switch
        {
            "InstructionsPlacement" => new ServerSettings { InstructionsPlacement = InstructionsPlacement.LeadingUserTurn },
            "TokenLimitParameter" => new ServerSettings { TokenLimitParameter = TokenLimitParameter.MaxTokens },
            "CarryImageToolResultsAsUserMessage" => new ServerSettings { CarryImageToolResultsAsUserMessage = true },
            _ => new ServerSettings { MaxRetries = 2 },
        };
        var declaration = new ProviderDeclaration { Id = "srv", Wire = wire, BaseUrl = "http://127.0.0.1:1", ApiKey = "k", Settings = settings };

        var create = () => ProviderClientFactory.Create(declaration, "m");

        create.Should().Throw<ArgumentException>().WithMessage($"*Provider 'srv'*{expected}*does not apply on the {wire} wire*");
    }

    [Fact]
    public void ASettingOnAModelWhoseWireCannotCarryIt_NamesTheModel()
    {
        var declaration = new ProviderDeclaration
        {
            Id = "srv",
            Wire = WireFormat.OpenAIChatCompletions,
            BaseUrl = "http://127.0.0.1:1/v1",
            Settings = new ServerSettings { InstructionsPlacement = InstructionsPlacement.LeadingUserTurn },
            Models = new Dictionary<string, ModelDeclaration> { ["claude-x"] = new() { Wire = WireFormat.Anthropic, BaseUrl = "http://127.0.0.1:1" } },
        };

        ProviderClientFactory.Create(declaration, "plain").Dispose();
        var create = () => ProviderClientFactory.Create(declaration, "claude-x");

        create.Should().Throw<ArgumentException>().WithMessage("*Settings.InstructionsPlacement*does not apply on the Anthropic wire*");
    }

    [Theory]
    [InlineData("00:00:00", "Settings.StreamIdleTimeout must be greater than zero")]
    [InlineData("-00:00:05", "Settings.StreamIdleTimeout must be greater than zero")]
    public void ANonPositiveTimeout_IsRefusedByName(string timeout, string message)
    {
        var declaration = new ProviderDeclaration
        {
            Id = "srv",
            Wire = WireFormat.OpenAIChatCompletions,
            BaseUrl = "http://127.0.0.1:1/v1",
            Settings = new ServerSettings { StreamIdleTimeout = TimeSpan.Parse(timeout, System.Globalization.CultureInfo.InvariantCulture) },
        };

        var create = () => ProviderClientFactory.Create(declaration, "m");

        create.Should().Throw<ArgumentException>().WithMessage($"*Provider 'srv'*{message}*");
    }

    [Fact]
    public void ANegativeRetryCountOrUndefinedEnum_IsRefusedByName()
    {
        ProviderDeclaration With(ServerSettings settings, string? modelOverride = null) => new()
        {
            Id = "srv",
            Wire = WireFormat.OpenAIChatCompletions,
            BaseUrl = "http://127.0.0.1:1/v1",
            Settings = modelOverride is null ? settings : null,
            Models = modelOverride is null ? null : new Dictionary<string, ModelDeclaration> { [modelOverride] = new() { Settings = settings } },
        };

        ((Action)(() => ProviderClientFactory.Create(With(new ServerSettings { MaxRetries = -1 }), "m")))
            .Should().Throw<ArgumentException>().WithMessage("*Settings.MaxRetries must not be negative*");
        ((Action)(() => ProviderClientFactory.Create(With(new ServerSettings { InstructionsPlacement = (InstructionsPlacement)99 }), "m")))
            .Should().Throw<ArgumentException>().WithMessage("*Settings.InstructionsPlacement 99 is not one of*");
        ((Action)(() => ProviderClientFactory.Create(With(new ServerSettings { Timeout = TimeSpan.Zero }, "m"), "m")))
            .Should().Throw<ArgumentException>().WithMessage("*Models['m'].Settings.Timeout must be greater than zero*");
    }

    [Fact]
    public void Settings_RoundTripThroughJsonByName()
    {
        var declaration = new ProviderDeclaration
        {
            Id = "srv",
            Wire = WireFormat.OpenAIChatCompletions,
            BaseUrl = "http://127.0.0.1:1/v1",
            Settings = new ServerSettings
            {
                StreamIdleTimeout = TimeSpan.FromMinutes(2),
                MaxRetries = 0,
                InstructionsPlacement = InstructionsPlacement.LeadingUserTurn,
                TokenLimitParameter = TokenLimitParameter.MaxTokens,
                CarryImageToolResultsAsUserMessage = true,
            },
        };

        var json = JsonSerializer.Serialize(declaration);
        var back = JsonSerializer.Deserialize<ProviderDeclaration>(json)!;

        json.Should().Contain("\"StreamIdleTimeout\":\"00:02:00\"").And.Contain("\"InstructionsPlacement\":\"LeadingUserTurn\"")
            .And.Contain("\"TokenLimitParameter\":\"MaxTokens\"");
        back.Settings.Should().Be(declaration.Settings);
        JsonSerializer.Deserialize<ProviderDeclaration>("""{"Id":"a","Wire":"Anthropic","BaseUrl":"http://x"}""")!.Settings
            .Should().BeNull("a stored declaration without Settings means every adapter default");
    }
}

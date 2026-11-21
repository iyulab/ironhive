using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Anthropic.Models.Messages;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Providers.Anthropic;

namespace IronHive.Tests.Providers;

/// <summary>
/// The Models API states per model which thinking types and effort levels it accepts; the finder turns that into the policy the
/// generator consults (<see cref="AnthropicModelCapabilities"/>), so a model the built-in table does not know yet is declared by
/// the vendor's own answer instead of by a hand-copied row. Observed through the real SDK client on a stubbed wire in the
/// documented <c>ModelInfo</c> shape, and through the real generator for what reaches the wire.
/// </summary>
public sealed class AnthropicModelFinderPolicyTests
{
    /// <summary>What the live Models API reported on 2026-11-21 (BYOK key, 15 ids): (id, enabled, adaptive, disabled, between_tools, effort supported, xhigh).</summary>
    private static readonly (string Id, bool Enabled, bool Adaptive, bool Disabled, bool BetweenTools, bool Effort, bool XHigh)[] LiveReport =
    [
        ("claude-fable-5-2", false, true, false, false, true, true),
        ("claude-haiku-5-5", false, true, true, false, true, true),
        ("claude-sonnet-5-5", false, true, false, true, true, true),
        ("claude-opus-5-5", false, true, false, false, true, true),
        ("claude-fable-5-1", false, true, false, false, true, true),
        ("claude-opus-5", false, true, true, false, true, true),
        ("claude-sonnet-5", false, true, true, false, true, true),
        ("claude-fable-5", false, true, false, false, true, true),
        ("claude-opus-4-8", false, true, true, false, true, true),
        ("claude-opus-4-7", false, true, true, false, true, true),
        ("claude-sonnet-4-6", true, true, true, false, true, false),
        ("claude-opus-4-6", true, true, true, false, true, false),
        ("claude-opus-4-5-20251101", true, false, true, false, true, false),
        ("claude-haiku-4-5-20251001", true, false, true, false, false, false),
        ("claude-sonnet-4-5-20250929", true, false, true, false, false, false),
    ];

    private static JsonObject Support(bool supported) => new() { ["supported"] = supported };

    private static JsonObject Capabilities(bool enabled, bool adaptive, bool disabled, bool betweenTools, bool effort, bool xhigh) => new()
    {
        ["batch"] = Support(true),
        ["citations"] = Support(true),
        ["code_execution"] = Support(true),
        ["context_management"] = new JsonObject
        {
            ["supported"] = false,
            ["clear_thinking_20251015"] = Support(false),
            ["clear_tool_uses_20250919"] = Support(false),
            ["compact_20260112"] = Support(false),
        },
        ["effort"] = new JsonObject
        {
            ["supported"] = effort,
            ["low"] = Support(effort),
            ["medium"] = Support(effort),
            ["high"] = Support(effort),
            ["max"] = Support(effort),
            ["xhigh"] = Support(xhigh),
        },
        ["image_input"] = Support(true),
        ["pdf_input"] = Support(true),
        ["server_tools"] = new JsonObject
        {
            ["supported"] = true,
            ["code_execution"] = Support(true),
            ["web_search"] = Support(true),
        },
        ["structured_outputs"] = Support(true),
        ["thinking"] = new JsonObject
        {
            ["supported"] = enabled || adaptive,
            ["types"] = new JsonObject
            {
                ["adaptive"] = Support(adaptive),
                ["between_tools"] = Support(betweenTools),
                ["disabled"] = Support(disabled),
                ["enabled"] = Support(enabled),
            },
        },
    };

    private static JsonObject Model(string id, JsonObject? capabilities)
    {
        var model = new JsonObject
        {
            ["type"] = "model",
            ["id"] = id,
            ["display_name"] = id,
            ["created_at"] = "2026-11-01T00:00:00Z",
            ["max_input_tokens"] = 1_000_000,
            ["max_tokens"] = 128_000,
        };
        if (capabilities is not null)
            model["capabilities"] = capabilities;
        return model;
    }

    private static JsonObject Reported(string id, bool enabled, bool adaptive, bool disabled, bool betweenTools, bool effort = true, bool xhigh = true) =>
        Model(id, Capabilities(enabled, adaptive, disabled, betweenTools, effort, xhigh));

    private static AnthropicModelFinder Finder(string body, HttpStatusCode status = HttpStatusCode.OK, AnthropicConfig? config = null)
    {
        var http = new HttpClient(new StubHandler(body, status));
        var settings = config ?? new AnthropicConfig();
        settings.BaseUrl = "http://server.test";
        settings.ApiKey = "k";
        settings.HttpClient = http;
        settings.MaxRetries = 0;
        return new AnthropicModelFinder(settings);
    }

    private static string Page(params JsonObject[] models) => new JsonObject
    {
        ["data"] = new JsonArray(models.Select(m => (JsonNode)m).ToArray()),
        ["has_more"] = false,
        ["first_id"] = models.FirstOrDefault()?["id"]?.GetValue<string>(),
        ["last_id"] = models.LastOrDefault()?["id"]?.GetValue<string>(),
    }.ToJsonString();

    private static string WireOf(MessageCreateParams req) => req.ToString().Replace(" ", string.Empty);

    private static MessageGenerationRequest NoThinking(string model) => new()
    {
        Model = model,
        ThinkingEffort = MessageThinkingEffort.None,
        Messages = [IronHive.Abstractions.Messages.Message.User("Hi")],
    };

    // ── the derivation ───────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AnUnknownModel_ThatReportsBetweenTools_IsTurnedOffWithBetweenTools()
    {
        // `claude-next-1` is in no table row: without the API's answer it would be the LowEffort default.
        using var finder = Finder(Model("claude-next-1", Capabilities(enabled: false, adaptive: true, disabled: false, betweenTools: true, effort: true, xhigh: true)).ToJsonString());

        var policy = await finder.FindCapabilitiesAsync("claude-next-1", TestContext.Current.CancellationToken);

        AnthropicModelCapabilities.Resolve("claude-next-1").ThinkingOffStyle.Should().Be(AnthropicThinkingOffStyle.LowEffort, "the table does not know this id");
        policy.Should().NotBeNull();
        policy!.ThinkingStyle.Should().Be(AnthropicThinkingStyle.Adaptive);
        policy.ThinkingOffStyle.Should().Be(AnthropicThinkingOffStyle.BetweenTools);
        policy.SupportsXHighEffort.Should().BeTrue();
    }

    [Theory]
    [InlineData(false, true, true, false, AnthropicThinkingOffStyle.Disabled)]
    [InlineData(false, true, false, false, AnthropicThinkingOffStyle.LowEffort)]
    [InlineData(false, true, true, true, AnthropicThinkingOffStyle.BetweenTools)]
    public async Task TheOffStyleFollowsTheReportedThinkingTypes(bool enabled, bool adaptive, bool disabled, bool betweenTools, AnthropicThinkingOffStyle expected)
    {
        using var finder = Finder(Model("claude-x", Capabilities(enabled, adaptive, disabled, betweenTools, effort: true, xhigh: true)).ToJsonString());

        (await finder.FindCapabilitiesAsync("claude-x", TestContext.Current.CancellationToken))!.ThinkingOffStyle.Should().Be(expected);
    }

    [Fact]
    public async Task AModelThatReportsOnlyEnabled_IsBudgetStyle_AndANoXHighEffortModelCapsAtHigh()
    {
        using var legacy = Finder(Model("claude-old", Capabilities(enabled: true, adaptive: false, disabled: true, betweenTools: false, effort: true, xhigh: false)).ToJsonString());

        var policy = (await legacy.FindCapabilitiesAsync("claude-old", TestContext.Current.CancellationToken))!;

        policy.ThinkingStyle.Should().Be(AnthropicThinkingStyle.Budget);
        policy.SupportsXHighEffort.Should().BeFalse();
    }

    [Fact]
    public async Task WhatTheApiDoesNotState_ComesFromTheBuiltInRow_AndAConsumerRowIsTheFallback()
    {
        // `claude-fable-5-2-next` matches the Fable 5.2 prefix row (no forced tool choice); the API has no word on tool_choice.
        using var builtIn = Finder(Model("claude-fable-5-2-next", Capabilities(false, true, true, false, true, true)).ToJsonString());
        var fromTable = (await builtIn.FindCapabilitiesAsync("claude-fable-5-2-next", TestContext.Current.CancellationToken))!;
        fromTable.SupportsForcedToolChoice.Should().BeFalse("a forced tool_choice is not in capabilities - the table's value stays");
        fromTable.ThinkingOffStyle.Should().Be(AnthropicThinkingOffStyle.Disabled, "the report wins where the API speaks");

        // A consumer entry is the fallback for an id of their own - it outranks the built-in table.
        var config = new AnthropicConfig
        {
            ModelCapabilities = new Dictionary<string, AnthropicModelCapabilities>
            {
                ["claude-mine"] = new() { SupportsForcedToolChoice = false },
            },
        };
        using var consumer = Finder(Model("claude-mine", Capabilities(false, true, true, false, true, true)).ToJsonString(), config: config);
        var mine = (await consumer.FindCapabilitiesAsync("claude-mine", TestContext.Current.CancellationToken))!;
        mine.SupportsForcedToolChoice.Should().BeFalse();
        mine.ThinkingOffStyle.Should().Be(AnthropicThinkingOffStyle.Disabled);
    }

    [Fact]
    public async Task AnAnswerWithoutCapabilities_IsTheBuiltInRowUnchanged_AndAnUnknownModelIsNull()
    {
        using var gateway = Finder(Model("claude-sonnet-5-5", capabilities: null).ToJsonString());
        (await gateway.FindCapabilitiesAsync("claude-sonnet-5-5", TestContext.Current.CancellationToken))
            .Should().Be(AnthropicModelCapabilities.Resolve("claude-sonnet-5-5"));

        using var missing = Finder("""{"type":"error","error":{"type":"not_found_error","message":"model: nope"}}""", HttpStatusCode.NotFound);
        (await missing.FindCapabilitiesAsync("nope", TestContext.Current.CancellationToken)).Should().BeNull();
    }

    // ── what the generator then sends ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListCapabilities_AssignedToTheConfig_ChangesWhatReachesTheWire()
    {
        using var finder = Finder(Page(
            Reported("claude-next-1", enabled: false, adaptive: true, disabled: false, betweenTools: true),
            Reported("claude-next-2", enabled: false, adaptive: true, disabled: true, betweenTools: false)));
        var policies = await finder.ListCapabilitiesAsync(TestContext.Current.CancellationToken);

        policies.Keys.Should().Equal("claude-next-1", "claude-next-2");

        var config = new AnthropicConfig { ApiKey = "test-key", ModelCapabilities = policies };
        using var generator = new AnthropicMessageGenerator(config);

        var between = WireOf(generator.ToMessageCreateParams(NoThinking("claude-next-1")));
        between.Should().Contain("\"type\":\"between_tools\"").And.Contain("\"effort\":\"low\"");

        var disabled = WireOf(generator.ToMessageCreateParams(NoThinking("claude-next-2")));
        disabled.Should().Contain("\"type\":\"disabled\"").And.NotContain("\"effort\"");

        // The same two ids without the finder's answer both get the LowEffort default (adaptive thinking at the lowest effort).
        using var plain = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });
        WireOf(plain.ToMessageCreateParams(NoThinking("claude-next-2"))).Should().NotContain("\"type\":\"disabled\"");
    }

    [Fact]
    public void Haiku55_TurnsThinkingOffWithDisabled()
    {
        // The Models API reports `disabled` supported for it and a live request with it answered 200 (2026-11-21); before its row
        // the model fell to the LowEffort default and kept thinking on.
        AnthropicModelCapabilities.Resolve("claude-haiku-5-5").ThinkingOffStyle.Should().Be(AnthropicThinkingOffStyle.Disabled);

        using var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });
        var wire = WireOf(generator.ToMessageCreateParams(NoThinking("claude-haiku-5-5")));
        wire.Should().Contain("\"type\":\"disabled\"").And.NotContain("\"effort\"");
    }

    // ── the table against the vendor's own report ────────────────────────────────────────────────

    // Teeth for the hand-written table: for every id the live API was asked about, what the finder derives from the report equals
    // what the built-in table says. A new Claude row that contradicts the API, or a table gap the API fills, shows up here. The one
    // exception is a decision, not a fact: Claude Opus 5 reports `disabled` as supported while the table follows the vendor's advice
    // to send a low effort instead - the finder follows the report, the table follows the advice.
    [Fact]
    public async Task TheBuiltInTable_AgreesWithTheVendorsReport_ExceptOpus5()
    {
        using var finder = Finder(Page(LiveReport.Select(r => Reported(r.Id, r.Enabled, r.Adaptive, r.Disabled, r.BetweenTools, r.Effort, r.XHigh)).ToArray()));
        var derived = await finder.ListCapabilitiesAsync(TestContext.Current.CancellationToken);

        derived.Should().HaveCount(LiveReport.Length);
        foreach (var (id, _, adaptive, _, _, effort, _) in LiveReport)
        {
            var table = AnthropicModelCapabilities.Resolve(id);
            var fromReport = derived[id];

            fromReport.ThinkingStyle.Should().Be(table.ThinkingStyle, $"{id}: thinking style");
            if (adaptive && id != "claude-opus-5")
                fromReport.ThinkingOffStyle.Should().Be(table.ThinkingOffStyle, $"{id}: off style");
            // xhigh only matters on the adaptive branch of the generator (Budget generations send a token budget, not an effort).
            if (effort && adaptive)
                fromReport.SupportsXHighEffort.Should().Be(table.SupportsXHighEffort, $"{id}: xhigh effort");
        }

        derived["claude-opus-5"].ThinkingOffStyle.Should().Be(AnthropicThinkingOffStyle.Disabled, "the report says disabled is supported");
        AnthropicModelCapabilities.Resolve("claude-opus-5").ThinkingOffStyle.Should().Be(AnthropicThinkingOffStyle.LowEffort, "the table follows the vendor's advice");
    }

    private sealed class StubHandler(string body, HttpStatusCode status) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
    }
}

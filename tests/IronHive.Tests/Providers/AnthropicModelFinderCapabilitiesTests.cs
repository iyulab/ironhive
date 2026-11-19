using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using IronHive.Abstractions.Models;
using IronHive.Providers.Anthropic;

namespace IronHive.Tests.Providers;

/// <summary>
/// The Models API states what a model accepts (<c>capabilities</c>): the finder carries it onto the card so a consumer
/// learns «does it think / does it take images» from the vendor instead of from a name table or the first answer.
/// Observed through the real SDK client against a stubbed wire, in the documented <c>ModelInfo</c> shape.
/// </summary>
public sealed class AnthropicModelFinderCapabilitiesTests
{
    private static JsonObject Support(bool supported) => new() { ["supported"] = supported };

    /// <summary>The documented <c>capabilities</c> object — every key is always present; <paramref name="on"/> are the ones the vendor reports as supported.</summary>
    private static JsonObject Capabilities(params string[] on)
    {
        JsonObject Flag(string key) => Support(on.Contains(key));

        return new JsonObject
        {
            ["batch"] = Flag("batch"),
            ["citations"] = Flag("citations"),
            ["code_execution"] = Flag("programmatic-code-execution"),
            ["context_management"] = new JsonObject
            {
                ["supported"] = false,
                ["clear_thinking_20251015"] = Support(false),
                ["clear_tool_uses_20250919"] = Support(false),
                ["compact_20260112"] = Support(false),
            },
            ["effort"] = new JsonObject
            {
                ["supported"] = on.Contains("effort"),
                ["low"] = Support(on.Contains("effort")),
                ["medium"] = Support(on.Contains("effort")),
                ["high"] = Support(on.Contains("effort")),
                ["max"] = Support(on.Contains("effort")),
                ["xhigh"] = Support(on.Contains("effort")),
            },
            ["image_input"] = Flag("image"),
            ["pdf_input"] = Flag("pdf"),
            ["server_tools"] = new JsonObject
            {
                ["supported"] = on.Contains("web-search") || on.Contains("code-execution"),
                ["code_execution"] = Support(on.Contains("code-execution")),
                ["web_search"] = Support(on.Contains("web-search")),
            },
            ["structured_outputs"] = Flag("structured-output"),
            ["thinking"] = new JsonObject
            {
                ["supported"] = on.Contains("thinking"),
                ["types"] = new JsonObject
                {
                    ["adaptive"] = on.Contains("thinking"),
                    ["between_tools"] = on.Contains("thinking"),
                    ["disabled"] = false,
                    ["enabled"] = false,
                },
            },
        };
    }

    private static JsonObject Model(string id, JsonObject? capabilities, bool limits = true)
    {
        var model = new JsonObject
        {
            ["type"] = "model",
            ["id"] = id,
            ["display_name"] = id,
            ["created_at"] = "2026-11-01T00:00:00Z",
        };
        if (limits)
        {
            model["max_input_tokens"] = 1_000_000;
            model["max_tokens"] = 128_000;
        }
        if (capabilities is not null)
            model["capabilities"] = capabilities;
        return model;
    }

    private static AnthropicModelFinder Finder(string body)
    {
        var http = new HttpClient(new StubHandler(body));
        return new AnthropicModelFinder(new AnthropicConfig { BaseUrl = "http://server.test", ApiKey = "k", HttpClient = http, MaxRetries = 0 });
    }

    private static string Single(JsonObject model) => model.ToJsonString();

    private static string Page(params JsonObject[] models) => new JsonObject
    {
        ["data"] = new JsonArray(models.Select(m => (JsonNode)m).ToArray()),
        ["has_more"] = false,
        ["first_id"] = models.FirstOrDefault()?["id"]?.GetValue<string>(),
        ["last_id"] = models.LastOrDefault()?["id"]?.GetValue<string>(),
    }.ToJsonString();

    [Fact]
    public async Task FindModel_carries_what_the_vendor_reports_as_supported()
    {
        using var finder = Finder(Single(Model("claude-x", Capabilities("thinking", "effort", "image", "pdf", "structured-output", "web-search"))));

        var card = await finder.FindModelAsync("claude-x", TestContext.Current.CancellationToken);

        var language = card.Should().BeOfType<LanguageModelCard>().Subject;
        language.ContextWindow.Should().Be(1_000_000);
        language.MaxOutputTokens.Should().Be(128_000);
        language.InputModalities.Should().Equal("text", "image", "pdf");
        language.Features.Should().Equal("thinking", "effort", "structured-output", "web-search");
    }

    [Fact]
    public async Task A_capability_reported_unsupported_is_not_listed()
    {
        // Positive control for the test above: the same wire, only the image flag off — it disappears from the list, the rest stays.
        using var finder = Finder(Single(Model("claude-y", Capabilities("thinking", "pdf"))));

        var language = (await finder.FindModelAsync("claude-y", TestContext.Current.CancellationToken))
            .Should().BeOfType<LanguageModelCard>().Subject;

        language.InputModalities.Should().Equal("text", "pdf");
        language.Features.Should().Equal("thinking");
    }

    [Fact]
    public async Task ListModels_carries_the_capabilities_per_model()
    {
        using var finder = Finder(Page(
            Model("claude-a", Capabilities("image")),
            Model("claude-b", Capabilities())));

        var cards = (await finder.ListModelsAsync(TestContext.Current.CancellationToken)).Cast<LanguageModelCard>().ToList();

        cards.Select(c => c.ModelId).Should().Equal("claude-a", "claude-b");
        cards[0].InputModalities.Should().Equal("text", "image");
        cards[1].InputModalities.Should().Equal("text");
        cards[1].Features.Should().BeEmpty("the vendor reported every capability, none of them supported");
    }

    [Fact]
    public async Task An_answer_without_capabilities_leaves_both_lists_null_and_keeps_the_limits()
    {
        // A gateway that answers the limits but not the capabilities object.
        using var finder = Finder(Single(Model("claude-gw", capabilities: null)));

        var language = (await finder.FindModelAsync("claude-gw", TestContext.Current.CancellationToken))
            .Should().BeOfType<LanguageModelCard>().Subject;

        language.ContextWindow.Should().Be(1_000_000);
        language.Features.Should().BeNull();
        language.InputModalities.Should().BeNull();
    }

    [Fact]
    public async Task An_answer_with_neither_limits_nor_capabilities_is_still_a_plain_card()
    {
        using var finder = Finder(Single(Model("claude-bare", capabilities: null, limits: false)));

        var card = await finder.FindModelAsync("claude-bare", TestContext.Current.CancellationToken);

        card.Should().BeOfType<ModelCard>();
    }

    private sealed class StubHandler(string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
    }
}

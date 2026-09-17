using Anthropic.Models.Messages;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Providers.Anthropic;

namespace IronHive.Tests.Providers;

/// <summary>
/// The Anthropic generator translates the requested intent into what the target model accepts on the
/// wire, and AnthropicModelCapabilities is the model-generation policy behind that translation. These
/// facts pin the translation without a network: a forced tool choice on a model that returns 400 for
/// it (Claude 5.1, per the vendor migration guide) must leave as tool_choice auto plus an explicit
/// instruction, while the same request on a Claude 4.x model must keep the forced wire value; the
/// thinking parameter shape must follow the generation; and a consumer override must win over the
/// built-in table so a model this package has never heard of can be declared without a code change.
/// </summary>
public class AnthropicModelCapabilitiesTests
{
    private static AnthropicMessageGenerator Generator(AnthropicConfig? config = null)
        => new(config ?? new AnthropicConfig { ApiKey = "test-key" });

    private static MessageGenerationRequest Request(
        string model,
        IronHive.Abstractions.Messages.ToolChoice? toolChoice = null,
        string? system = null,
        MessageThinkingEffort? effort = null)
        => new()
        {
            Model = model,
            System = system,
            ToolChoice = toolChoice,
            ThinkingEffort = effort,
            Messages = [IronHive.Abstractions.Messages.Message.User("Hi")],
        };

    [Fact]
    public void Resolve_LongestPrefixWins_AndUnknownModelIsDefault()
    {
        AnthropicModelCapabilities.Resolve("claude-fable-5-1-20260901").SupportsForcedToolChoice.Should().BeFalse();
        AnthropicModelCapabilities.Resolve("claude-mythos-5-1").SupportsForcedToolChoice.Should().BeFalse();
        AnthropicModelCapabilities.Resolve("claude-sonnet-4-5-20250929").ThinkingStyle.Should().Be(AnthropicThinkingStyle.Budget);
        AnthropicModelCapabilities.Resolve("claude-opus-5").Should().BeSameAs(AnthropicModelCapabilities.Default);
    }

    [Fact]
    public void Resolve_ConsumerOverrideWinsOverBuiltIn()
    {
        var overrides = new Dictionary<string, AnthropicModelCapabilities>
        {
            ["claude-fable-5-1"] = new() { SupportsForcedToolChoice = true },
            ["claude-opus-5"] = new() { SupportsForcedToolChoice = false },
        };

        AnthropicModelCapabilities.Resolve("claude-fable-5-1", overrides).SupportsForcedToolChoice.Should().BeTrue();
        AnthropicModelCapabilities.Resolve("claude-opus-5-20261001", overrides).SupportsForcedToolChoice.Should().BeFalse();
    }

    [Fact]
    public void RequiredToolChoice_OnClaude51_LeavesAsAutoWithInstruction()
    {
        var req = Generator().ToMessageCreateParams(Request("claude-fable-5-1", new RequiredToolChoice(), system: "Be brief."));

        req.ToolChoice.Should().BeNull("tool_choice any returns 400 on this model, so the wire must carry auto");
        var system = SystemTextOf(req);
        system.Should().StartWith("Be brief.");
        system.Should().Contain("calling one of the provided tools");
    }

    [Fact]
    public void NamedToolChoice_OnClaude51_NamesTheToolInTheInstruction()
    {
        var req = Generator().ToMessageCreateParams(Request("claude-fable-5-1", new FunctionToolChoice(["get_weather"])));

        req.ToolChoice.Should().BeNull();
        SystemTextOf(req).Should().Contain("`get_weather`");
    }

    [Fact]
    public void RequiredToolChoice_OnClaude4x_KeepsTheForcedWireValue()
    {
        var req = Generator().ToMessageCreateParams(Request("claude-sonnet-4-5", new RequiredToolChoice(), system: "Be brief."));

        req.ToolChoice.Should().NotBeNull();
        req.ToolChoice!.TryPickAny(out _).Should().BeTrue();
        SystemTextOf(req).Should().Be("Be brief.", "no instruction is injected where the model accepts the forced choice");
    }

    [Fact]
    public void RequiredToolChoice_OverrideRestoresForcedWireValue()
    {
        var config = new AnthropicConfig
        {
            ApiKey = "test-key",
            ModelCapabilities = new Dictionary<string, AnthropicModelCapabilities>
            {
                ["claude-fable-5-1"] = new() { SupportsForcedToolChoice = true },
            },
        };

        var req = Generator(config).ToMessageCreateParams(Request("claude-fable-5-1", new RequiredToolChoice()));

        req.ToolChoice!.TryPickAny(out _).Should().BeTrue();
    }

    [Fact]
    public void ThinkingShape_FollowsTheGeneration()
    {
        var legacy = Generator().ToMessageCreateParams(Request("claude-sonnet-4-5", effort: MessageThinkingEffort.Low));
        var current = Generator().ToMessageCreateParams(Request("claude-fable-5-1", effort: MessageThinkingEffort.Low));

        legacy.Thinking!.TryPickEnabled(out _).Should().BeTrue();
        current.Thinking!.TryPickAdaptive(out _).Should().BeTrue();
    }

    // Live 400 (claude-haiku-4-5, max_tokens 3000, effort Low → budget 4000): "`max_tokens` must be greater
    // than `thinking.budget_tokens`". A caller's small output cap must not turn a thinking request into a failure.
    [Theory]
    [InlineData(null, 4_000L)]    // no cap: provider default max_tokens, full effort budget
    [InlineData(8_000, 4_000L)]   // cap above the budget: unchanged
    [InlineData(4_000, 2_000L)]   // cap equal to the budget: halved, leaving room for the answer
    [InlineData(3_000, 1_500L)]   // the measured case
    [InlineData(2_048, 1_024L)]   // halved lands exactly on the vendor minimum
    public void BudgetThinking_StaysBelowMaxTokens(int? maxTokens, long expectedBudget)
    {
        var request = Request("claude-sonnet-4-5", effort: MessageThinkingEffort.Low);
        request.MaxTokens = maxTokens;

        var req = Generator().ToMessageCreateParams(request);

        req.Thinking!.TryPickEnabled(out var enabled).Should().BeTrue();
        enabled!.BudgetTokens.Should().Be(expectedBudget);
        enabled.BudgetTokens.Should().BeLessThan(req.MaxTokens);
    }

    [Fact]
    public void BudgetThinking_UnderACapTooSmallToThink_IsNotEnabled()
    {
        var request = Request("claude-sonnet-4-5", effort: MessageThinkingEffort.Minimal);
        request.MaxTokens = 1_024;

        var req = Generator().ToMessageCreateParams(request);

        req.Thinking.Should().BeNull("half of 1,024 is below the vendor minimum budget, so no valid thinking request fits");
    }

    private static string SystemTextOf(MessageCreateParams req)
    {
        if (req.System!.TryPickString(out var text)) return text;
        if (req.System.TryPickTextBlockParams(out var blocks)) return string.Concat(blocks.Select(b => b.Text));
        return string.Empty;
    }
}

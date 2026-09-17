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

    // Adaptive depth is output_config.effort; without it Minimal and XHigh were the same request.
    [Theory]
    [InlineData("claude-sonnet-5", MessageThinkingEffort.Minimal, "low")]
    [InlineData("claude-sonnet-5", MessageThinkingEffort.Medium, "medium")]
    [InlineData("claude-sonnet-5", MessageThinkingEffort.XHigh, "xhigh")]
    [InlineData("claude-opus-4-6", MessageThinkingEffort.XHigh, "high")]
    [InlineData("claude-opus-5", MessageThinkingEffort.High, "high")]
    public void AdaptiveThinking_CarriesTheEffortLevel(string model, MessageThinkingEffort effort, string wireEffort)
    {
        var req = Generator().ToMessageCreateParams(Request(model, effort: effort));

        req.Thinking!.TryPickAdaptive(out _).Should().BeTrue();
        WireOf(req).Should().Contain($"\"effort\":\"{wireEffort}\"");
    }

    // Sonnet 5 / Opus 5 / Fable think when `thinking` is omitted, so None must say "off" on the wire.
    [Fact]
    public void NoThinking_OnAModelThatAcceptsDisabled_IsDisabled()
    {
        var req = Generator().ToMessageCreateParams(Request("claude-sonnet-5", effort: MessageThinkingEffort.None));

        req.Thinking!.TryPickDisabled(out _).Should().BeTrue();
        WireOf(req).Should().NotContain("\"effort\"");
    }

    [Theory]
    [InlineData("claude-fable-5-1")]
    [InlineData("claude-opus-5")]
    public void NoThinking_OnAModelThatCannotDisable_IsTheLowestEffort(string model)
    {
        var req = Generator().ToMessageCreateParams(Request(model, effort: MessageThinkingEffort.None));

        req.Thinking.Should().BeNull("disabled is a 400 on Fable and vendor-discouraged on Opus 5");
        WireOf(req).Should().Contain("\"effort\":\"low\"");
    }

    [Fact]
    public void NoThinking_OnABudgetModel_SendsNothing()
    {
        var req = Generator().ToMessageCreateParams(Request("claude-haiku-4-5", effort: MessageThinkingEffort.None));

        req.Thinking.Should().BeNull();
        req.OutputConfig.Should().BeNull("omitting thinking is already off here, and effort is a 400 on Haiku 4.5");
    }

    [Fact]
    public void UnsetThinking_SendsNothing()
    {
        var req = Generator().ToMessageCreateParams(Request("claude-sonnet-5"));

        req.Thinking.Should().BeNull();
        req.OutputConfig.Should().BeNull();
    }

    [Theory]
    [InlineData(MessageThinkingOutput.None, "omitted")]
    [InlineData(MessageThinkingOutput.Summary, "summarized")]
    [InlineData(MessageThinkingOutput.Full, "summarized")]
    public void AdaptiveThinking_CarriesTheDisplay(MessageThinkingOutput output, string display)
    {
        var request = Request("claude-sonnet-5", effort: MessageThinkingEffort.Medium);
        request.ThinkingOutput = output;

        WireOf(Generator().ToMessageCreateParams(request)).Should().Contain($"\"display\":\"{display}\"");
    }

    [Fact]
    public void AdaptiveThinking_WithoutOutput_LeavesTheModelDefaultDisplay()
    {
        WireOf(Generator().ToMessageCreateParams(Request("claude-sonnet-5", effort: MessageThinkingEffort.Medium)))
            .Should().NotContain("\"display\"");
    }

    private static string WireOf(MessageCreateParams req) => req.ToString().Replace(" ", string.Empty);

    private static string SystemTextOf(MessageCreateParams req)
    {
        if (req.System!.TryPickString(out var text)) return text;
        if (req.System.TryPickTextBlockParams(out var blocks)) return string.Concat(blocks.Select(b => b.Text));
        return string.Empty;
    }
}

using System.Reflection;
using AwesomeAssertions;
using Anthropic.Models.Messages;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using IronHive.Extensions.AI;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using Microsoft.Extensions.AI;
using ChatFinishReason = Microsoft.Extensions.AI.ChatFinishReason;
using IronHiveMessage = IronHive.Abstractions.Messages.Message;
using NSubstitute;

namespace IronHive.Tests.Conventions.Providers;

/// <summary>
/// A provider's response must say what the provider did where it differs from what was asked: Anthropic's stop reasons keep
/// the cases a caller acts on differently apart, thinking that could not be applied is reported (not dropped silently), and
/// the one provider that cannot honour <c>AllowParallelToolCalls = false</c> refuses it instead of ignoring it.
/// </summary>
public class ProviderSignalsTests
{
    private static MessageGenerationRequest Request(string model, MessageThinkingEffort? effort = null, int? maxTokens = null) => new()
    {
        Model = model,
        Messages = [IronHiveMessage.User("Hi")],
        ThinkingEffort = effort,
        MaxTokens = maxTokens,
    };

    private static ITool Tool()
    {
        var tool = Substitute.For<ITool>();
        tool.UniqueName.Returns("read_file");
        tool.Description.Returns("d");
        tool.Parameters.Returns(null);
        tool.RequiresApproval.Returns(false);
        return tool;
    }

    // ---- stop reasons ----

    [Theory]
    [InlineData(StopReason.ToolUse, MessageDoneReason.ToolCall)]
    [InlineData(StopReason.EndTurn, MessageDoneReason.EndTurn)]
    [InlineData(StopReason.PauseTurn, MessageDoneReason.Paused)]
    [InlineData(StopReason.MaxTokens, MessageDoneReason.MaxTokens)]
    [InlineData(StopReason.ModelContextWindowExceeded, MessageDoneReason.ContextExceeded)]
    [InlineData(StopReason.StopSequence, MessageDoneReason.StopSequence)]
    [InlineData(StopReason.Refusal, MessageDoneReason.ContentFilter)]
    public void Anthropic_StopReason_KeepsEachCaseApart(StopReason stopReason, MessageDoneReason expected)
    {
        AnthropicMessageGenerator.ToDoneReason(stopReason).Should().Be(expected);
    }

    [Fact]
    public void Anthropic_NoStopReason_IsNull()
    {
        AnthropicMessageGenerator.ToDoneReason(null).Should().BeNull();
    }

    [Fact]
    public void Anthropic_TheSeparatedReasons_AreNotAnyOtherValue()
    {
        // The three cases the audit listed collapsed into EndTurn / StopSequence / null; none may land there again.
        var separated = new[] { StopReason.PauseTurn, StopReason.Refusal, StopReason.ModelContextWindowExceeded }
            .Select(r => AnthropicMessageGenerator.ToDoneReason(r))
            .ToList();

        separated.Should().OnlyHaveUniqueItems();
        separated.Should().NotContain(new MessageDoneReason?[] { MessageDoneReason.EndTurn, MessageDoneReason.StopSequence, null });
    }

    private static ChatFinishReason? Finish(MessageDoneReason? reason) =>
        (ChatFinishReason?)typeof(ChatClientAdapter)
            .GetMethod("ConvertDoneReason", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [reason]);

    [Fact]
    public void ChatClientAdapter_Paused_IsAFinishReasonOfItsOwn_AndContextExceededIsLength()
    {
        Finish(MessageDoneReason.Paused).Should().Be(new ChatFinishReason("pause_turn"));
        Finish(MessageDoneReason.Paused).Should().NotBe(ChatFinishReason.Stop);
        Finish(MessageDoneReason.ContextExceeded).Should().Be(ChatFinishReason.Length);
        Finish(MessageDoneReason.ContentFilter).Should().Be(ChatFinishReason.ContentFilter);
        Finish(MessageDoneReason.EndTurn).Should().Be(ChatFinishReason.Stop);
    }

    [Fact]
    public void MessageContext_PausedAndContextExceeded_EndTheTurn()
    {
        // Like MaxTokens: reported, not re-sent. The doc on ShouldContinue says so; this pins the behaviour.
        var context = new MessageContext(new MessageRequest { Provider = "anthropic", Model = "claude-sonnet-4-5", Messages = [IronHiveMessage.User("Hi")] });
        foreach (var reason in new[] { MessageDoneReason.Paused, MessageDoneReason.ContextExceeded, MessageDoneReason.MaxTokens })
        {
            typeof(MessageContext).GetProperty(nameof(MessageContext.TurnReason))!
                .GetSetMethod(nonPublic: true)!.Invoke(context, [reason]);
            context.ShouldContinue().Should().BeFalse($"{reason} has no pending tool call to run");
        }
    }

    // ---- thinking that cannot be applied ----

    [Fact]
    public void Anthropic_ThinkingBelowTheMinimumBudget_IsReported()
    {
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });
        var warnings = new List<MessageWarning>();

        generator.ToMessageCreateParams(Request("claude-sonnet-4-5", MessageThinkingEffort.Medium, maxTokens: 1000), warnings);

        warnings.Should().ContainSingle().Which.Code.Should().Be(MessageWarningCodes.ThinkingNotApplied);
        warnings[0].Message.Should().Contain("1000").And.Contain("1024");
    }

    [Fact]
    public void Anthropic_ThinkingThatFits_OrWasNotAsked_ReportsNothing()
    {
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });
        var fits = new List<MessageWarning>();
        var notAsked = new List<MessageWarning>();

        generator.ToMessageCreateParams(Request("claude-sonnet-4-5", MessageThinkingEffort.Medium, maxTokens: 20_000), fits);
        generator.ToMessageCreateParams(Request("claude-sonnet-4-5", effort: null, maxTokens: 300), notAsked);

        fits.Should().BeEmpty();
        notAsked.Should().BeEmpty();
    }

    [Fact]
    public void Gemini_ThinkingBelowTheMinimumBudget_IsReported()
    {
        var generator = new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key" });
        var tooSmall = new List<MessageWarning>();
        var fits = new List<MessageWarning>();

        generator.ToGoogleAIParams(Request("gemini-2.5-flash", MessageThinkingEffort.Medium, maxTokens: 600), tooSmall);
        generator.ToGoogleAIParams(Request("gemini-2.5-flash", MessageThinkingEffort.Medium, maxTokens: 4000), fits);

        tooSmall.Should().ContainSingle().Which.Code.Should().Be(MessageWarningCodes.ThinkingNotApplied);
        fits.Should().BeEmpty();
    }

    [Fact]
    public void ChatCompletions_ReasoningTheCapLeavesNoRoomFor_IsReported()
    {
        var tooSmall = new List<MessageWarning>();
        var fits = new List<MessageWarning>();
        var none = new List<MessageWarning>();

        ChatCompletionMessageGenerator.BuildRequest(Request("gpt-4o", MessageThinkingEffort.Medium, maxTokens: 256), warnings: tooSmall);
        ChatCompletionMessageGenerator.BuildRequest(Request("gpt-4o", MessageThinkingEffort.Medium, maxTokens: 4000), warnings: fits);
        ChatCompletionMessageGenerator.BuildRequest(Request("gpt-4o", MessageThinkingEffort.None, maxTokens: 256), warnings: none);

        tooSmall.Should().ContainSingle().Which.Code.Should().Be(MessageWarningCodes.ThinkingNotApplied);
        fits.Should().BeEmpty();
        none.Should().BeEmpty("None asks for no reasoning, so nothing was dropped");
    }

    [Fact]
    public void ChatClientAdapter_CarriesWarningsToAdditionalProperties_AndOmitsTheKeyWhenThereAreNone()
    {
        var convert = typeof(ChatClientAdapter).GetMethod("ToAdditionalProperties", BindingFlags.NonPublic | BindingFlags.Static)!;
        IReadOnlyList<MessageWarning> warnings = [MessageWarning.ThinkingNotApplied(1024, 800)];

        var withWarnings = (AdditionalPropertiesDictionary?)convert.Invoke(null, [null, warnings]);
        var without = (AdditionalPropertiesDictionary?)convert.Invoke(null, [null, null]);

        withWarnings.Should().NotBeNull();
        withWarnings![ChatClientAdapter.WarningsKey].Should().BeAssignableTo<IReadOnlyList<MessageWarning>>()
            .Which.Should().ContainSingle().Which.Code.Should().Be("thinking_not_applied");
        without.Should().BeNull();
    }

    // ---- parallel tool calls on a provider that cannot honour the control ----

    [Fact]
    public async Task Gemini_RefusesNoParallelToolCalls_WhenToolsAreSent()
    {
        using var generator = new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key" });
        var request = Request("gemini-2.5-flash");
        request.Tools = new ToolCollection([Tool()]);
        request.AllowParallelToolCalls = false;

        await FluentActions.Awaiting(() => generator.GenerateMessageAsync(request, TestContext.Current.CancellationToken))
            .Should().ThrowAsync<NotSupportedException>().WithMessage("*AllowParallelToolCalls*");
        await FluentActions.Awaiting(async () =>
            {
                await foreach (var _ in generator.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken))
                {
                }
            })
            .Should().ThrowAsync<NotSupportedException>().WithMessage("*AllowParallelToolCalls*");
    }

    [Fact]
    public void Gemini_DoesNotRefuseTheControlWhenItAsksForNothingGeminiLacks()
    {
        var generator = new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key" });
        var reject = typeof(GoogleAIMessageGenerator).GetMethod("RejectUnsupported", BindingFlags.NonPublic | BindingFlags.Static)!;

        var unset = Request("gemini-2.5-flash");
        unset.Tools = new ToolCollection([Tool()]);
        var allowed = Request("gemini-2.5-flash");
        allowed.Tools = new ToolCollection([Tool()]);
        allowed.AllowParallelToolCalls = true;
        var noTools = Request("gemini-2.5-flash");
        noTools.AllowParallelToolCalls = false;

        foreach (var request in new[] { unset, allowed, noTools })
        {
            FluentActions.Invoking(() => reject.Invoke(null, [request])).Should().NotThrow();
        }

        generator.Should().NotBeNull();
    }
}

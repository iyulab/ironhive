using System.ClientModel.Primitives;
using System.Reflection;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using OpenAI.Responses;

namespace IronHive.Tests.Conventions.Providers;

/// <summary>
/// Per-request instructions (<see cref="MessageGenerationRequest.Instructions"/>) reach every provider after the system
/// prompt, never ahead of it: a prefix cache reuses only the unchanged leading text, so two requests that differ only in
/// their instructions must open with the same prompt. With no prompt, the instructions take its place.
/// </summary>
public class ProviderInstructionsPlacementTests
{
    private const string Prompt = "You are a file assistant. Follow the workspace rules.";
    private const string Instructions = "This run: only touch files under /reports.";

    private static MessageGenerationRequest Request(string model, string? system, string? instructions) => new()
    {
        Model = model,
        System = system,
        Instructions = instructions,
        Messages = [Message.User("Tidy up.")],
    };

    // ── Anthropic ────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Anthropic_InstructionsAreASecondSystemBlockAfterThePrompt()
    {
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });

        var system = generator.ToMessageCreateParams(Request("claude-sonnet-5", Prompt, Instructions)).System!;

        system.TryPickTextBlockParams(out var blocks).Should().BeTrue();
        blocks!.Select(b => b.Text).Should().Equal(Prompt, Instructions);
    }

    [Fact]
    public void Anthropic_InstructionsAloneAreThePrompt()
    {
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });

        var system = generator.ToMessageCreateParams(Request("claude-sonnet-5", null, Instructions)).System!;

        system.TryPickString(out var text).Should().BeTrue();
        text.Should().Be(Instructions);
    }

    [Fact]
    public void Anthropic_CountTokens_CountsTheInstructionsToo()
    {
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });

        var wire = generator.ToMessageCountTokensParams(Request("claude-sonnet-5", Prompt, Instructions)).ToString();

        wire.IndexOf(Prompt, StringComparison.Ordinal).Should().BeLessThan(wire.IndexOf(Instructions, StringComparison.Ordinal));
    }

    // ── GoogleAI ─────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GoogleAI_InstructionsAreASecondSystemPartAfterThePrompt()
    {
        var generator = new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key" });

        var (_, config) = generator.ToGoogleAIParams(Request("gemini-2.5-flash", Prompt, Instructions));

        config.SystemInstruction!.Parts!.Select(p => p.Text).Should().Equal(Prompt, Instructions);
    }

    // ── OpenAI Responses ─────────────────────────────────────────────────────────────────────────

    private static CreateResponseOptions OpenAIOptions(MessageGenerationRequest request)
    {
        var build = typeof(OpenAIMessageGenerator).GetMethod("BuildOptions", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (CreateResponseOptions)build.Invoke(null, [request, null])!;
    }

    [Fact]
    public void OpenAIResponses_PromptIsInstructions_AndTheRequestsInstructionsOpenTheInputAsADeveloperMessage()
    {
        var options = OpenAIOptions(Request("gpt-5", Prompt, Instructions));

        options.Instructions.Should().Be(Prompt);
        var first = ModelReaderWriter.Write(options.InputItems[0]).ToString();
        first.Should().Contain("\"developer\"").And.Contain(Instructions);
    }

    [Fact]
    public void OpenAIResponses_InstructionsAloneAreThePrompt()
    {
        var options = OpenAIOptions(Request("gpt-5", null, Instructions));

        options.Instructions.Should().Be(Instructions);
        options.InputItems.Should().HaveCount(1, "only the user message — no developer message duplicates the prompt");
    }

    // ── OpenAI-compatible Chat Completions ───────────────────────────────────────────────────────

    [Fact]
    public void ChatCompletions_ByDefault_InstructionsFollowThePromptInTheSystemMessage()
    {
        var messages = ChatCompletionMessageGenerator.BuildMessages(Request("m", Prompt, Instructions));
        var other = ChatCompletionMessageGenerator.BuildMessages(Request("m", Prompt, "Another run: only /drafts."));

        var system = messages[0].Should().BeOfType<SystemChatMessage>().Subject.Content;
        system.Should().StartWith(Prompt).And.EndWith(Instructions);
        ((SystemChatMessage)other[0]).Content.Should().StartWith(Prompt, "runs that differ only in their instructions share the leading prompt");
        messages.OfType<SystemChatMessage>().Should().ContainSingle("every chat template accepts one system message");
    }

    [Fact]
    public void ChatCompletions_OptIn_InstructionsAreASecondSystemMessage()
    {
        var messages = ChatCompletionMessageGenerator.BuildMessages(
            Request("m", Prompt, Instructions), instructionsAsSeparateSystemMessage: true);

        messages.OfType<SystemChatMessage>().Select(m => m.Content).Should().Equal(Prompt, Instructions);
        messages[2].Should().BeOfType<UserChatMessage>();
    }

    [Fact]
    public void ChatCompletions_InstructionsAloneAreTheSystemMessage()
    {
        var messages = ChatCompletionMessageGenerator.BuildMessages(
            Request("m", null, Instructions), instructionsAsSeparateSystemMessage: true);

        messages.OfType<SystemChatMessage>().Should().ContainSingle().Which.Content.Should().Be(Instructions);
    }
}

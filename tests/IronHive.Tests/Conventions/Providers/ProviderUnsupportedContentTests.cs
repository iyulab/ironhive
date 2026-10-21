using System.Reflection;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;

namespace IronHive.Tests.Conventions.Providers;

/// <summary>
/// Content a provider does not accept fails with <see cref="NotSupportedException"/> naming the provider and the content
/// type — not <see cref="NotImplementedException"/>("not supported yet"), which read as a missing feature and named nothing.
/// <see cref="MessageContent"/> is public and unsealed, so a consumer's own subtype is the case every provider meets.
/// </summary>
public class ProviderUnsupportedContentTests
{
    private sealed class CustomContent : MessageContent;

    private static MessageGenerationRequest Request(string model, Message message) => new()
    {
        Model = model,
        Messages = [message],
    };

    private static Message With(MessageRole role)
    {
        var message = new Message { Role = role };
        message.Content.Add(new CustomContent());
        return message;
    }

    public static TheoryData<MessageRole, string> Roles => new()
    {
        { MessageRole.User, "user" },
        { MessageRole.Assistant, "assistant" },
    };

    [Theory]
    [MemberData(nameof(Roles))]
    public void Anthropic_NamesTheUnsupportedContent(MessageRole role, string side)
    {
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });

        var act = () => generator.ToMessageCreateParams(Request("claude-sonnet-5-5", With(role)));

        act.Should().Throw<NotSupportedException>().WithMessage($"Anthropic does not support {side} message content of type CustomContent*");
    }

    [Theory]
    [MemberData(nameof(Roles))]
    public void GoogleAI_NamesTheUnsupportedContent(MessageRole role, string side)
    {
        var generator = new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key" });

        var act = () => generator.ToGoogleAIParams(Request("gemini-2.5-flash", With(role)));

        act.Should().Throw<NotSupportedException>().WithMessage($"Google AI does not support {side} message content of type CustomContent*");
    }

    [Theory]
    [MemberData(nameof(Roles))]
    public void OpenAIResponses_NamesTheUnsupportedContent(MessageRole role, string side)
    {
        var build = typeof(OpenAIMessageGenerator).GetMethod("BuildOptions", BindingFlags.NonPublic | BindingFlags.Static)!;

        var act = () => build.Invoke(null, [Request("gpt-5", With(role)), null]);

        act.Should().Throw<TargetInvocationException>().WithInnerException<NotSupportedException>()
            .WithMessage($"OpenAI does not support {side} message content of type CustomContent*");
    }

    [Theory]
    [MemberData(nameof(Roles))]
    public void ChatCompletions_NamesTheUnsupportedContent(MessageRole role, string side)
    {
        var act = () => ChatCompletionMessageGenerator.BuildMessages(Request("gpt-4o", With(role)));

        act.Should().Throw<NotSupportedException>()
            .WithMessage($"The OpenAI-compatible provider does not support {side} message content of type CustomContent*");
    }
}

using System.Text.Json;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using IronHive.Tests.Conventions.Providers;

namespace IronHive.Tests.Providers;

/// <summary>
/// A turn that failed before any reply leaves the person's message without an answer, so the next request would carry
/// two user messages in a row. A server whose chat template checks that roles alternate refuses that while rendering and
/// every later turn of the conversation fails the same way — so neighbouring text-only messages of one role go out as one.
/// </summary>
public class ChatCompletionAdjacentTurnTests
{
    private static MessageGenerationRequest Request(params Message[] messages) => new()
    {
        Model = "test-model",
        System = "You are brief.",
        Messages = messages,
    };

    private static IEnumerable<string> Roles(IEnumerable<ChatMessage> messages) => messages.Select(m => m switch
    {
        SystemChatMessage => "system",
        UserChatMessage => "user",
        AssistantChatMessage => "assistant",
        ToolChatMessage => "tool",
        _ => "?",
    });

    private static string Text(ChatMessage message) =>
        string.Concat(((UserChatMessage)message).Content.Cast<TextChatMessageContent>().Select(p => p.Text));

    [Fact]
    public void TwoUserMessagesInARow_GoOutAsOneWithABlankLineBetween()
    {
        var messages = ChatCompletionMessageGenerator.BuildMessages(
            Request(Message.User("first try"), Message.User("second try")));

        Roles(messages).Should().Equal("system", "user");
        Text(messages[1]).Should().Be("first try\n\nsecond try");
    }

    [Fact]
    public async Task TheRequestAfterAFailedTurn_ReachesTheServerWithAlternatingRoles()
    {
        // The history a host keeps when the first turn failed before any reply: the person's message, then the next one.
        const string response =
            """{"id":"chatcmpl-1","object":"chat.completion","created":1757635200,"model":"test-model","choices":[{"index":0,"message":{"role":"assistant","content":"ok"},"finish_reason":"stop","logprobs":null}],"usage":{"prompt_tokens":1,"completion_tokens":1,"total_tokens":2}}""";
        var handler = new StubHttpHandler(response, string.Empty);
        using var generator = new ChatCompletionMessageGenerator(new OpenAIConfig
        {
            ApiKey = "test-key",
            BaseUrl = "https://compatible.invalid/v1",
            HttpClient = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan },
        });

        await generator.GenerateMessageAsync(
            Request(Message.User("first try"), Message.User("second try")),
            TestContext.Current.CancellationToken);

        using var body = JsonDocument.Parse(handler.Requests.Should().ContainSingle().Which.Body);
        var messages = body.RootElement.GetProperty("messages").EnumerateArray().ToList();
        messages.Select(m => m.GetProperty("role").GetString()).Should().Equal("system", "user");
        messages[1].GetProperty("content").EnumerateArray().Select(p => p.GetProperty("text").GetString())
            .Should().Equal("first try", "\n\nsecond try");
    }

    [Fact]
    public void FailedTurnInTheMiddle_OnlyTheDoubledUserMessageIsJoined()
    {
        var messages = ChatCompletionMessageGenerator.BuildMessages(Request(
            Message.User("q1"), Message.Assistant("a1"), Message.User("q2 (failed)"), Message.User("q2 again")));

        Roles(messages).Should().Equal("system", "user", "assistant", "user");
        Text(messages[3]).Should().Be("q2 (failed)\n\nq2 again");
    }

    [Fact]
    public void TwoTextAssistantMessagesInARow_GoOutAsOne()
    {
        var messages = ChatCompletionMessageGenerator.BuildMessages(
            Request(Message.User("q"), Message.Assistant("part one"), Message.Assistant("part two")));

        Roles(messages).Should().Equal("system", "user", "assistant");
        messages[2].Should().BeOfType<AssistantChatMessage>().Which.Content.Should().Be("part one\n\npart two");
    }

    [Fact]
    public void AlternatingTurns_AreUntouched()
    {
        var messages = ChatCompletionMessageGenerator.BuildMessages(
            Request(Message.User("q1"), Message.Assistant("a1"), Message.User("q2")));

        Roles(messages).Should().Equal("system", "user", "assistant", "user");
        Text(messages[1]).Should().Be("q1");
        Text(messages[3]).Should().Be("q2");
    }

    [Fact]
    public void AToolCallRound_IsNeverMerged()
    {
        var call = new ToolMessageContent
        {
            Id = "call_1",
            Name = "get_weather",
            Input = "{}",
            Output = ToolOutput.Success("sunny"),
            IsApproved = true,
        };

        // A text-only assistant message directly before one that holds text AND the call: the two are neighbours of one
        // role, and only the second carries tool_calls - they must stay two messages (merging would drop the call) with
        // the result right after it.
        var messages = ChatCompletionMessageGenerator.BuildMessages(Request(
            Message.User("weather?"),
            Message.Assistant("Hold on."),
            Message.Assistant(new TextMessageContent { Value = "Let me check." }, call),
            Message.Assistant("It is sunny."),
            Message.User("thanks")));

        Roles(messages).Should().Equal("system", "user", "assistant", "assistant", "tool", "assistant", "user");
        messages[3].Should().BeOfType<AssistantChatMessage>().Which.ToolCalls.Should().ContainSingle();
    }

    [Fact]
    public void AUserMessageWithAnImage_IsNeverMerged()
    {
        var image = new ImageMessageContent { Format = ImageFormat.Png, Base64 = "AAAA" };

        var messages = ChatCompletionMessageGenerator.BuildMessages(Request(
            Message.User(new TextMessageContent { Value = "look at this" }, image),
            Message.User("and then this")));

        Roles(messages).Should().Equal("system", "user", "user");
    }
}

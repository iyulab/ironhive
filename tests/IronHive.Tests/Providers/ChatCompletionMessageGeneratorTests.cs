using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;

namespace IronHive.Tests.Providers;

/// <summary>
/// Covers the IronHive → Chat Completions request translation and finish-reason mapping. These are the
/// error-prone, provider-boundary paths; the live HTTP round-trip is exercised by the Filer e2e gate (AC#5).
/// </summary>
public class ChatCompletionMessageGeneratorTests
{
    private static MessageGenerationRequest Request(string? system, params Message[] messages) => new()
    {
        Model = "test-model",
        System = system,
        Messages = messages,
    };

    [Fact]
    public void BuildMessages_System_IsPrependedAsSystemMessage()
    {
        var messages = ChatCompletionMessageGenerator.BuildMessages(
            Request("you are helpful", Message.User("hi")));

        messages.Should().HaveCount(2);
        var system = messages[0].Should().BeOfType<SystemChatMessage>().Subject;
        system.Content.Should().Be("you are helpful");
        messages[1].Should().BeOfType<UserChatMessage>();
    }

    [Fact]
    public void BuildMessages_NoSystem_OmitsSystemMessage()
    {
        var messages = ChatCompletionMessageGenerator.BuildMessages(
            Request(null, Message.User("hi")));

        messages.Should().ContainSingle().Which.Should().BeOfType<UserChatMessage>();
    }

    [Fact]
    public void BuildMessages_UserTextAndImage_MapToContentParts()
    {
        var msg = Message.User(
            new TextMessageContent { Value = "look" },
            new ImageMessageContent { Format = ImageFormat.Png, Base64 = "aGVsbG8=" });

        var messages = ChatCompletionMessageGenerator.BuildMessages(Request(null, msg));

        var user = messages.Single().Should().BeOfType<UserChatMessage>().Subject;
        user.Content.Should().HaveCount(2);
        user.Content.First().Should().BeOfType<TextChatMessageContent>()
            .Which.Text.Should().Be("look");
        user.Content.Last().Should().BeOfType<ImageChatMessageContent>()
            .Which.ImageUrl.Url.Should().StartWith("data:image/png;base64,");
    }

    [Fact]
    public void BuildMessages_AssistantText_MapsToAssistantMessage()
    {
        var messages = ChatCompletionMessageGenerator.BuildMessages(
            Request(null, Message.User("q"), Message.Assistant("a")));

        messages.Should().HaveCount(2);
        var assistant = messages[1].Should().BeOfType<AssistantChatMessage>().Subject;
        assistant.Content.Should().Be("a");
    }

    [Fact]
    public void BuildMessages_AssistantToolCall_EmitsToolCallThenToolResult()
    {
        var toolContent = new ToolMessageContent
        {
            Id = "call_1",
            Name = "get_weather",
            Input = "{\"city\":\"seoul\"}",
            Output = new ToolOutput(true, "sunny"),
            IsApproved = true,
        };
        var messages = ChatCompletionMessageGenerator.BuildMessages(
            Request(null, Message.User("weather?"), Message.Assistant(toolContent)));

        // user, assistant(with tool call), tool(result)
        messages.Should().HaveCount(3);
        var assistant = messages[1].Should().BeOfType<AssistantChatMessage>().Subject;
        assistant.ToolCalls.Should().ContainSingle();
        assistant.ToolCalls!.First().Id.Should().Be("call_1");
        assistant.ToolCalls!.First().Function!.Name.Should().Be("get_weather");

        var tool = messages[2].Should().BeOfType<ToolChatMessage>().Subject;
        tool.ToolCallId.Should().Be("call_1");
        tool.Content.Should().Be("sunny");
    }

    [Fact]
    public void BuildMessages_AssistantThinking_IsNotReplayed()
    {
        // Chat Completions has no reasoning-input block; thinking-only assistant turns produce no message.
        var messages = ChatCompletionMessageGenerator.BuildMessages(
            Request(null, Message.User("q"),
                Message.Assistant(new ThinkingMessageContent { Value = "hmm" })));

        messages.Should().ContainSingle().Which.Should().BeOfType<UserChatMessage>();
    }

    [Fact]
    public void BuildRequest_MaxTokens_IsMapped()
    {
        var request = Request(null, Message.User("hi"));
        request.MaxTokens = 128;

        var chatRequest = ChatCompletionMessageGenerator.BuildRequest(request);

        chatRequest.MaxCompletionTokens.Should().Be(128);
    }

    [Theory]
    [InlineData(ChatFinishReason.ToolCalls, MessageDoneReason.ToolCall)]
    [InlineData(ChatFinishReason.Stop, MessageDoneReason.EndTurn)]
    [InlineData(ChatFinishReason.Length, MessageDoneReason.MaxTokens)]
    [InlineData(ChatFinishReason.ContentFilter, MessageDoneReason.ContentFilter)]
    public void MapFinishReason_MapsToIronHiveReason(ChatFinishReason input, MessageDoneReason expected)
    {
        ChatCompletionMessageGenerator.MapFinishReason(input).Should().Be(expected);
    }
}

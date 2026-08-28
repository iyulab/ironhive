using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using IronHive.Providers.OpenAI.Compatible;
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

    // === Output-length parameter name ===
    //
    // OpenAI renamed max_tokens to max_completion_tokens and the ecosystem split: current OpenAI
    // models reject the old name, while many self-hosted servers only recognise it - and an
    // unrecognised field is ignored rather than rejected, so the limit vanishes without an error and
    // the caller just gets a longer response than they asked for. Since no single name works
    // everywhere, these assert the payload that actually goes on the wire for each choice.

    // Mirrors ChatCompletionHttpClient's own options: what the assertions read has to be what the
    // client would actually put on the wire.
    private static readonly System.Text.Json.JsonSerializerOptions PayloadOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private static string SerializePayload(ChatCompletionRequest request) =>
        System.Text.Json.JsonSerializer.Serialize(request, PayloadOptions);

    [Fact]
    public void BuildRequest_DefaultsToTheCurrentSpelling_SoExistingCallersSeeNoChange()
    {
        var request = Request(null, Message.User("hi"));
        request.MaxTokens = 300;

        var payload = SerializePayload(ChatCompletionMessageGenerator.BuildRequest(request));

        payload.Should().Contain("\"max_completion_tokens\":300");
        payload.Should().NotContain("\"max_tokens\"");
    }

    [Fact]
    public void BuildRequest_MaxTokensChoice_SendsOnlyTheDeprecatedSpelling()
    {
        var request = Request(null, Message.User("hi"));
        request.MaxTokens = 300;

        var payload = SerializePayload(
            ChatCompletionMessageGenerator.BuildRequest(request, TokenLimitParameter.MaxTokens));

        payload.Should().Contain("\"max_tokens\":300");
        payload.Should().NotContain("max_completion_tokens");
    }

    [Fact]
    public void BuildRequest_BothChoice_SendsBothSpellingsWithTheSameValue()
    {
        var request = Request(null, Message.User("hi"));
        request.MaxTokens = 300;

        var payload = SerializePayload(
            ChatCompletionMessageGenerator.BuildRequest(request, TokenLimitParameter.Both));

        payload.Should().Contain("\"max_completion_tokens\":300");
        payload.Should().Contain("\"max_tokens\":300");
    }

    [Theory]
    [InlineData(TokenLimitParameter.MaxCompletionTokens)]
    [InlineData(TokenLimitParameter.MaxTokens)]
    [InlineData(TokenLimitParameter.Both)]
    public void BuildRequest_WithoutAMaxTokens_SendsNeitherSpelling(TokenLimitParameter choice)
    {
        // Selecting a name must not conjure a limit that the caller never set.
        var payload = SerializePayload(
            ChatCompletionMessageGenerator.BuildRequest(Request(null, Message.User("hi")), choice));

        payload.Should().NotContain("max_tokens");
        payload.Should().NotContain("max_completion_tokens");
    }

    /// <summary>
    /// A setting the config exposes but never hands to the generator is the same silent no-op the
    /// setting exists to fix, so the hand-off is pinned rather than assumed.
    /// </summary>
    [Fact]
    public void Config_CarriesTheChoiceToTheGenerator()
    {
        var config = new OpenAICompatibleConfig
        {
            BaseUrl = "http://localhost:11434",
            TokenLimitParameter = TokenLimitParameter.MaxTokens
        };

        using var generator = new OpenAICompatibleMessageGenerator(config);

        generator.EffectiveTokenLimitParameter.Should().Be(TokenLimitParameter.MaxTokens);
    }

    [Fact]
    public void Config_DefaultsToTheCurrentSpelling()
    {
        new OpenAICompatibleConfig().TokenLimitParameter
            .Should().Be(TokenLimitParameter.MaxCompletionTokens);
    }

    // === tool_choice ===
    //
    // ChatOptions.ToolMode (via ChatClientAdapter -> MessageGenerationRequest.ToolChoice) must
    // actually reach the wire — an unset/Auto value omits tool_choice entirely so the server's own
    // default applies, unchanged from before this field existed.

    private static Message[] UserWithTools => [Message.User("hi")];

    private static MessageGenerationRequest RequestWithTools(MessageToolChoice? toolChoice) => new()
    {
        Model = "test-model",
        Messages = UserWithTools,
        Tools = new ToolCollection([new StubTool("get_weather")]),
        ToolChoice = toolChoice,
    };

    private sealed class StubTool(string name) : ITool
    {
        public string UniqueName => name;
        public string? Description => null;
        public object? Parameters => null;
        public bool RequiresApproval { get; set; }
        public Task<ToolOutput> InvokeAsync(ToolInput input, CancellationToken cancellationToken = default) =>
            Task.FromResult(ToolOutput.Success(string.Empty));
    }

    [Fact]
    public void BuildRequest_ToolChoiceUnset_OmitsToolChoice_KeepsTools()
    {
        var payload = SerializePayload(ChatCompletionMessageGenerator.BuildRequest(RequestWithTools(null)));

        payload.Should().NotContain("tool_choice");
        payload.Should().Contain("get_weather");
    }

    [Fact]
    public void BuildRequest_ToolChoiceAuto_OmitsToolChoice_KeepsTools()
    {
        var payload = SerializePayload(
            ChatCompletionMessageGenerator.BuildRequest(RequestWithTools(MessageToolChoice.Auto)));

        payload.Should().NotContain("tool_choice");
        payload.Should().Contain("get_weather");
    }

    [Fact]
    public void BuildRequest_ToolChoiceNone_OmitsToolsEntirely_NotJustToolChoice()
    {
        // Not just tool_choice:"none" — the tool catalog itself must be gone, since some self-hosted
        // backends only partially honor tool_choice as a hint (see docket a95e2953 ask #2).
        var payload = SerializePayload(
            ChatCompletionMessageGenerator.BuildRequest(RequestWithTools(MessageToolChoice.None)));

        payload.Should().NotContain("get_weather");
        payload.Should().NotContain("\"tools\"");
    }

    [Fact]
    public void BuildRequest_ToolChoiceRequired_SendsRequiredString_KeepsTools()
    {
        var payload = SerializePayload(
            ChatCompletionMessageGenerator.BuildRequest(RequestWithTools(MessageToolChoice.Required)));

        payload.Should().Contain("\"tool_choice\":\"required\"");
        payload.Should().Contain("get_weather");
    }

    [Fact]
    public void BuildRequest_ToolChoiceFunction_SendsTypeAndFunctionNameObject_KeepsTools()
    {
        var payload = SerializePayload(
            ChatCompletionMessageGenerator.BuildRequest(RequestWithTools(MessageToolChoice.Function("get_weather"))));

        payload.Should().Contain("\"tool_choice\":{\"type\":\"function\",\"function\":{\"name\":\"get_weather\"}}");
        payload.Should().Contain("\"tools\"");
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

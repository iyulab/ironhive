using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using IronHive.Providers.OpenAI.Compatible.GpuStack;

namespace IronHive.Tests.Providers;

/// <summary>
/// Covers the IronHive → Chat Completions request translation and finish-reason mapping. These are the
/// error-prone, provider-boundary paths; the live HTTP round-trip is not exercised here.
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
    public void BuildMessages_UserAudio_MapsToInputAudioPart()
    {
        var msg = Message.User(
            new AudioMessageContent { Format = AudioFormat.Wav, Base64 = "aGVsbG8=" });

        var messages = ChatCompletionMessageGenerator.BuildMessages(Request(null, msg));

        var user = messages.Single().Should().BeOfType<UserChatMessage>().Subject;
        var audio = user.Content.Single().Should().BeOfType<AudioChatMessageContent>().Subject;
        audio.InputAudio.Data.Should().Be("aGVsbG8=");
        audio.InputAudio.Format.Should().Be("wav");
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
            Output = ToolOutput.Success("sunny"),
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
    public void BuildMessages_ToolResultWithMixedContent_JoinsTextAndDescribesNonText()
    {
        // Chat Completions' tool-role content is a plain string on the wire — text is joined as-is,
        // and content with no textual wire representation (images, ...) degrades to a placeholder.
        var toolContent = new ToolMessageContent
        {
            Id = "call_1",
            Name = "get_chart",
            Input = "{}",
            Output = ToolOutput.Success(
            [
                new TextMessageContent { Value = "here you go" },
                new ImageMessageContent { Format = ImageFormat.Png, Base64 = "AAAA" }
            ]),
            IsApproved = true,
        };
        var messages = ChatCompletionMessageGenerator.BuildMessages(
            Request(null, Message.User("chart?"), Message.Assistant(toolContent)));

        var tool = messages[2].Should().BeOfType<ToolChatMessage>().Subject;
        tool.Content.Should().Contain("here you go");
        tool.Content.Should().Contain("unsupported");
        tool.Content.Should().NotContain("AAAA");
    }

    [Fact]
    public void BuildMessages_CarryImageToolResults_PutsTheImagesInOneUserMessageAfterTheRoundsToolMessages()
    {
        // OpenAI-compatible servers refuse an image part in a tool message; opted in, the images follow the round's
        // tool messages in a user message, each introduced by the call it came from.
        var chart = new ToolMessageContent
        {
            Id = "call_1",
            Name = "get_chart",
            Input = "{}",
            Output = ToolOutput.Success(
            [
                new TextMessageContent { Value = "here you go" },
                new ImageMessageContent { Format = ImageFormat.Png, Base64 = "AAAA" }
            ]),
            IsApproved = true,
        };
        var weather = new ToolMessageContent { Id = "call_2", Name = "get_weather", Input = "{}", Output = ToolOutput.Success("sunny"), IsApproved = true };

        var messages = ChatCompletionMessageGenerator.BuildMessages(
            Request(null, Message.User("chart?"), Message.Assistant(chart, weather)), carryImageToolResults: true);

        messages.Select(m => m.GetType().Name).Should().Equal(
            nameof(UserChatMessage), nameof(AssistantChatMessage), nameof(ToolChatMessage), nameof(ToolChatMessage), nameof(UserChatMessage));
        var first = messages[2].Should().BeOfType<ToolChatMessage>().Subject;
        first.Content.Should().Be("here you go\n[image image/png — attached in the next message]");
        messages[3].Should().BeOfType<ToolChatMessage>().Which.Content.Should().Be("sunny");
        var carried = messages[4].Should().BeOfType<UserChatMessage>().Subject.Content!.ToList();
        carried[0].Should().BeOfType<TextChatMessageContent>().Which.Text.Should().Be("Image returned by tool call call_1 (get_chart):");
        carried[1].Should().BeOfType<ImageChatMessageContent>().Which.ImageUrl!.Url.Should().Be("data:image/png;base64,AAAA");
        carried.Should().HaveCount(2);
    }

    [Fact]
    public void BuildMessages_CarryImageToolResults_WithoutImages_AddsNoMessage()
    {
        var weather = new ToolMessageContent { Id = "call_2", Name = "get_weather", Input = "{}", Output = ToolOutput.Success("sunny"), IsApproved = true };

        var messages = ChatCompletionMessageGenerator.BuildMessages(
            Request(null, Message.User("weather?"), Message.Assistant(weather)), carryImageToolResults: true);

        messages.Should().HaveCount(3);
    }

    [Fact]
    public void CarryImageToolResults_FlowsFromBothConfigsToTheGenerator()
    {
        new GpuStackConfig { CarryImageToolResultsAsUserMessage = true }.ToOpenAICompatible()
            .CarryImageToolResultsAsUserMessage.Should().BeTrue();

        using var generator = new OpenAICompatibleMessageGenerator(new OpenAICompatibleConfig { CarryImageToolResultsAsUserMessage = true });
        var inner = typeof(OpenAICompatibleMessageGenerator)
            .GetField("_inner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(generator).Should().BeOfType<ChatCompletionMessageGenerator>().Subject;
        inner.CarryImageToolResultsAsUserMessage.Should().BeTrue();
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

    private static MessageGenerationRequest RequestWithTools(ToolChoice? toolChoice) => new()
    {
        Model = "test-model",
        Messages = UserWithTools,
        Tools = new ToolCollection([new StubTool("get_weather"), new StubTool("get_forecast"), new StubTool("get_alerts")]),
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
            ChatCompletionMessageGenerator.BuildRequest(RequestWithTools(ToolChoice.Auto)));

        payload.Should().NotContain("tool_choice");
        payload.Should().Contain("get_weather");
    }

    [Fact]
    public void BuildRequest_ToolChoiceNone_OmitsToolsEntirely_NotJustToolChoice()
    {
        // Not just tool_choice:"none" — the tool catalog itself must be gone, since some self-hosted
        // backends only partially honor tool_choice as a hint.
        var payload = SerializePayload(
            ChatCompletionMessageGenerator.BuildRequest(RequestWithTools(ToolChoice.None)));

        payload.Should().NotContain("get_weather");
        payload.Should().NotContain("\"tools\"");
    }

    [Fact]
    public void BuildRequest_ToolChoiceRequired_SendsRequiredString_KeepsTools()
    {
        var payload = SerializePayload(
            ChatCompletionMessageGenerator.BuildRequest(RequestWithTools(ToolChoice.Required)));

        payload.Should().Contain("\"tool_choice\":\"required\"");
        payload.Should().Contain("get_weather");
    }

    [Fact]
    public void BuildRequest_ToolChoiceFunction_SendsTypeAndFunctionNameObject_KeepsTools()
    {
        var payload = SerializePayload(
            ChatCompletionMessageGenerator.BuildRequest(RequestWithTools(ToolChoice.Function("get_weather"))));

        payload.Should().Contain("\"tool_choice\":{\"type\":\"function\",\"function\":{\"name\":\"get_weather\"}}");
        payload.Should().Contain("\"tools\"");
    }

    [Fact]
    public void BuildRequest_ToolChoiceMultiFunction_SendsRequired_FiltersToolsToNamedSet()
    {
        // No native "one of these N" wire value exists — approximate it with tool_choice:"required"
        // plus the outgoing tools array filtered down to just the named subset.
        var payload = SerializePayload(
            ChatCompletionMessageGenerator.BuildRequest(RequestWithTools(ToolChoice.Function("get_weather", "get_forecast"))));

        payload.Should().Contain("\"tool_choice\":\"required\"");
        payload.Should().Contain("get_weather");
        payload.Should().Contain("get_forecast");
        payload.Should().NotContain("get_alerts");
    }

    [Theory]
    [InlineData(nameof(ChatFinishReason.ToolCalls), MessageDoneReason.ToolCall)]
    [InlineData(nameof(ChatFinishReason.Stop), MessageDoneReason.EndTurn)]
    [InlineData(nameof(ChatFinishReason.Length), MessageDoneReason.MaxTokens)]
    [InlineData(nameof(ChatFinishReason.ContentFilter), MessageDoneReason.ContentFilter)]
    public void MapFinishReason_MapsToIronHiveReason(string input, MessageDoneReason expected)
    {
        // The wire enum is internal (a public test method cannot take it), so the row names it.
        ChatCompletionMessageGenerator.MapFinishReason(Enum.Parse<ChatFinishReason>(input)).Should().Be(expected);
    }

    // === Reasoning on a compatible server ===
    //
    // A compatible endpoint serves arbitrary model families, so the only reasoning instruction this
    // generator may put on the wire is one the caller actually gave. The vendor extensions below
    // (vLLM's thinking_token_budget, the chat_template_kwargs flags Qwen/DeepSeek/Granite read) are
    // how "no reasoning" is expressed to a model that reasons by default -- which is exactly why an
    // unset ThinkingEffort must not reach them.

    [Fact]
    public void BuildRequest_NoThinkingEffort_SendsNoReasoningInstructionAtAll()
    {
        var chatRequest = ChatCompletionMessageGenerator.BuildRequest(Request(null, Message.User("hi")));

        chatRequest.ReasoningEffort.Should().BeNull();
        chatRequest.ExtraBody.Should().BeNull(
            "a caller who never mentioned reasoning has not asked for it to be turned off");
    }

    [Fact]
    public void BuildRequest_ThinkingEffortNone_TurnsReasoningOffExplicitly()
    {
        var request = Request(null, Message.User("hi"));
        request.ThinkingEffort = MessageThinkingEffort.None;

        var chatRequest = ChatCompletionMessageGenerator.BuildRequest(request);

        chatRequest.ExtraBody.Should().NotBeNull();
        chatRequest.ExtraBody!["thinking_token_budget"]!.GetValue<int>().Should().Be(0);
        chatRequest.ExtraBody["chat_template_kwargs"]!["enable_thinking"]!.GetValue<bool>().Should().BeFalse();
        chatRequest.ExtraBody["chat_template_kwargs"]!["thinking"]!.GetValue<bool>().Should().BeFalse();
    }

    [Theory]
    [InlineData(MessageThinkingEffort.Minimal, 256)]
    [InlineData(MessageThinkingEffort.Low, 512)]
    [InlineData(MessageThinkingEffort.Medium, 1024)]
    [InlineData(MessageThinkingEffort.High, 2048)]
    [InlineData(MessageThinkingEffort.XHigh, 4096)]
    public void BuildRequest_ThinkingEffort_CarriesTheBudgetAndEnablesReasoning(
        MessageThinkingEffort effort, int expectedBudget)
    {
        var request = Request(null, Message.User("hi"));
        request.ThinkingEffort = effort;

        var chatRequest = ChatCompletionMessageGenerator.BuildRequest(request);

        chatRequest.ExtraBody!["thinking_token_budget"]!.GetValue<int>().Should().Be(expectedBudget);
        chatRequest.ExtraBody["chat_template_kwargs"]!["enable_thinking"]!.GetValue<bool>().Should().BeTrue();
    }

    /// <summary>
    /// The budget reaches llama.cpp too: it reads <c>reasoning_budget_tokens</c> (alias <c>thinking_budget_tokens</c>), not
    /// vLLM's <c>thinking_token_budget</c> — under one name the budget never applied there and reasoning ran to the output cap.
    /// </summary>
    [Theory]
    [InlineData(MessageThinkingEffort.None, 0)]
    [InlineData(MessageThinkingEffort.Medium, 1024)]
    public void BuildRequest_ThinkingEffort_SendsTheBudgetUnderEveryServersName(MessageThinkingEffort effort, int expectedBudget)
    {
        var request = Request(null, Message.User("hi"));
        request.ThinkingEffort = effort;

        var body = ChatCompletionMessageGenerator.BuildRequest(request).ExtraBody!;

        foreach (var name in new[] { "thinking_token_budget", "reasoning_budget_tokens", "thinking_budget_tokens" })
            body[name]!.GetValue<int>().Should().Be(expectedBudget, name);
    }

    [Fact]
    public void BuildRequest_CallerBudget_ReplacesOnlyTheNameItSets()
    {
        var request = Request(null, Message.User("hi"));
        request.ThinkingEffort = MessageThinkingEffort.High;
        request.ExtraBody = new System.Text.Json.Nodes.JsonObject { ["reasoning_budget_tokens"] = 99 };

        var body = ChatCompletionMessageGenerator.BuildRequest(request).ExtraBody!;

        body["reasoning_budget_tokens"]!.GetValue<int>().Should().Be(99);
        body["thinking_token_budget"]!.GetValue<int>().Should().Be(2048, "a caller field replaces only the name it names");
    }
}

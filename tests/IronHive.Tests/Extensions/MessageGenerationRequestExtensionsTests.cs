using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Providers.OpenAI.Payloads.Responses;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Tests.Extensions;

public class MessageGenerationRequestExtensionsTests
{
    #region ToOpenAI — Basic

    [Fact]
    public void ToOpenAI_Model_MapsToModel()
    {
        var request = CreateRequest("gpt-4o");

        var result = request.ToOpenAI();

        result.Model.Should().Be("gpt-4o");
    }

    [Fact]
    public void ToOpenAI_System_MapsToInstructions()
    {
        var request = CreateRequest();
        request.System = "You are a helpful assistant";

        var result = request.ToOpenAI();

        result.Instructions.Should().Be("You are a helpful assistant");
    }

    [Fact]
    public void ToOpenAI_MaxTokens_MapsToMaxOutputTokens()
    {
        var request = CreateRequest();
        request.MaxTokens = 1000;

        var result = request.ToOpenAI();

        result.MaxOutputTokens.Should().Be(1000);
    }

    #endregion

    #region ToOpenAI — UserMessage

    [Fact]
    public void ToOpenAI_UserMessage_TextContent_MapsToResponsesInputText()
    {
        var request = CreateRequest();
        request.Messages.Add(new UserMessage
        {
            Content = [new TextMessageContent { Value = "Hello" }]
        });

        var result = request.ToOpenAI();

        result.Input.Should().ContainSingle();
        var msg = result.Input!.First().Should().BeOfType<ResponsesMessageItem>().Subject;
        msg.Role.Should().Be(ResponsesMessageRole.User);
        msg.Content.Should().ContainSingle();
        var text = msg.Content.First().Should().BeOfType<ResponsesInputTextContent>().Subject;
        text.Text.Should().Be("Hello");
    }

    [Fact]
    public void ToOpenAI_UserMessage_DocumentContent_MapsToResponsesInputFile()
    {
        var request = CreateRequest();
        request.Messages.Add(new UserMessage
        {
            Content = [new DocumentMessageContent { Data = "doc-content" }]
        });

        var result = request.ToOpenAI();

        var msg = result.Input!.First().Should().BeOfType<ResponsesMessageItem>().Subject;
        msg.Content.Should().ContainSingle();
        var file = msg.Content.First().Should().BeOfType<ResponsesInputFileContent>().Subject;
        file.FileData.Should().Be("doc-content");
    }

    [Fact]
    public void ToOpenAI_UserMessage_ImageContent_MapsToResponsesInputImage()
    {
        var request = CreateRequest();
        request.Messages.Add(new UserMessage
        {
            Content = [new ImageMessageContent { Base64 = "abc123", Format = ImageFormat.Png }]
        });

        var result = request.ToOpenAI();

        var msg = result.Input!.First().Should().BeOfType<ResponsesMessageItem>().Subject;
        var image = msg.Content.First().Should().BeOfType<ResponsesInputImageContent>().Subject;
        image.ImageUrl.Should().Be("data:image/png;base64,abc123");
        image.Detail.Should().Be("auto");
    }

    [Fact]
    public void ToOpenAI_UserMessage_ImageWithDataPrefix_KeepsAsIs()
    {
        var request = CreateRequest();
        request.Messages.Add(new UserMessage
        {
            Content = [new ImageMessageContent { Base64 = "data:image/jpeg;base64,xyz", Format = ImageFormat.Jpeg }]
        });

        var result = request.ToOpenAI();

        var msg = result.Input!.First().Should().BeOfType<ResponsesMessageItem>().Subject;
        var image = msg.Content.First().Should().BeOfType<ResponsesInputImageContent>().Subject;
        image.ImageUrl.Should().Be("data:image/jpeg;base64,xyz");
    }

    #endregion

    #region ToOpenAI — AssistantMessage

    [Fact]
    public void ToOpenAI_AssistantMessage_TextContent_MapsToResponsesOutputText()
    {
        var request = CreateRequest();
        request.Messages.Add(new AssistantMessage
        {
            Content = [new TextMessageContent { Value = "Answer" }]
        });

        var result = request.ToOpenAI();

        var msg = result.Input!.First().Should().BeOfType<ResponsesMessageItem>().Subject;
        msg.Role.Should().Be(ResponsesMessageRole.Assistant);
        var text = msg.Content.First().Should().BeOfType<ResponsesOutputTextContent>().Subject;
        text.Text.Should().Be("Answer");
    }

    [Fact]
    public void ToOpenAI_AssistantMessage_ToolContent_MapsToCallAndOutput()
    {
        var request = CreateRequest();
        request.Messages.Add(new AssistantMessage
        {
            Content = [new ToolMessageContent
            {
                IsApproved = true,
                Id = "call-1",
                Name = "search",
                Input = "{\"q\":\"test\"}",
                Output = new ToolOutput { Result = "found it" }
            }]
        });

        var result = request.ToOpenAI();

        result.Input.Should().HaveCount(2);
        var call = result.Input!.ElementAt(0).Should().BeOfType<ResponsesFunctionToolCallItem>().Subject;
        call.CallId.Should().Be("call-1");
        call.Name.Should().Be("search");
        call.Arguments.Should().Be("{\"q\":\"test\"}");

        var output = result.Input!.ElementAt(1).Should().BeOfType<ResponsesFunctionToolOutputItem>().Subject;
        output.CallId.Should().Be("call-1");
        output.Output.Should().Be("found it");
    }

    [Fact]
    public void ToOpenAI_AssistantMessage_ThinkingContent_MapsToReasoningItem()
    {
        var request = CreateRequest();
        request.Messages.Add(new AssistantMessage
        {
            Content = [new ThinkingMessageContent
            {
                Value = "Let me think...",
                Signature = "enc-data"
            }]
        });

        var result = request.ToOpenAI();

        var reasoning = result.Input!.First().Should().BeOfType<ResponsesReasoningItem>().Subject;
        reasoning.EncryptedContent.Should().Be("enc-data");
        reasoning.Summary.Should().ContainSingle();
        reasoning.Summary!.First().Text.Should().Be("Let me think...");
    }

    #endregion

    #region ToOpenAI — ThinkingEffort / Reasoning

    [Fact]
    public void ToOpenAI_NoThinkingEffort_ReasoningIsNull()
    {
        var request = CreateRequest();

        var result = request.ToOpenAI();

        result.Reasoning.Should().BeNull();
        result.Include.Should().BeNull();
    }

    [Fact]
    public void ToOpenAI_ThinkingEffortHigh_SetsReasoning()
    {
        var request = CreateRequest();
        request.ThinkingEffort = MessageThinkingEffort.High;

        var result = request.ToOpenAI();

        result.Reasoning.Should().NotBeNull();
        result.Reasoning!.Effort.Should().Be(ResponsesReasoningEffort.High);
        result.Include.Should().Contain("reasoning.encrypted_content");
    }

    [Fact]
    public void ToOpenAI_ThinkingEffortEnabled_TemperatureIsNull()
    {
        var request = CreateRequest();
        request.ThinkingEffort = MessageThinkingEffort.Medium;
        request.Temperature = 0.7f;
        request.TopP = 0.9f;

        var result = request.ToOpenAI();

        result.Temperature.Should().BeNull();
        result.TopP.Should().BeNull();
    }

    [Fact]
    public void ToOpenAI_NoThinkingEffort_TemperaturePreserved()
    {
        var request = CreateRequest();
        request.Temperature = 0.7f;
        request.TopP = 0.9f;

        var result = request.ToOpenAI();

        result.Temperature.Should().Be(0.7f);
        result.TopP.Should().Be(0.9f);
    }

    #endregion

    #region ToOpenAILegacy — Basic

    [Fact]
    public void ToOpenAILegacy_Model_MapsToModel()
    {
        var request = CreateRequest("gpt-3.5-turbo");

        var result = request.ToOpenAILegacy();

        result.Model.Should().Be("gpt-3.5-turbo");
    }

    [Fact]
    public void ToOpenAILegacy_System_NoThinking_MapsToSystemMessage()
    {
        var request = CreateRequest();
        request.System = "Be helpful";
        request.ThinkingEffort = MessageThinkingEffort.None;

        var result = request.ToOpenAILegacy();

        result.Messages.Should().ContainSingle();
        result.Messages!.First().Should().BeOfType<SystemChatMessage>();
    }

    [Fact]
    public void ToOpenAILegacy_SystemWithReasoning_MapsToDeveloperMessage()
    {
        var request = CreateRequest();
        request.System = "Be helpful";
        request.ThinkingEffort = MessageThinkingEffort.High;

        var result = request.ToOpenAILegacy();

        result.Messages!.First().Should().BeOfType<DeveloperChatMessage>();
    }

    [Fact]
    public void ToOpenAILegacy_UserMessage_MapsToUserChatMessage()
    {
        var request = CreateRequest();
        request.Messages.Add(new UserMessage
        {
            Content = [new TextMessageContent { Value = "Hi" }]
        });

        var result = request.ToOpenAILegacy();

        result.Messages.Should().ContainSingle();
        var user = result.Messages!.First().Should().BeOfType<UserChatMessage>().Subject;
        user.Content.Should().ContainSingle();
        user.Content.First().Should().BeOfType<TextChatMessageContent>()
            .Which.Text.Should().Be("Hi");
    }

    [Fact]
    public void ToOpenAILegacy_MaxTokens_MapsToMaxCompletionTokens()
    {
        var request = CreateRequest();
        request.MaxTokens = 500;

        var result = request.ToOpenAILegacy();

        result.MaxCompletionTokens.Should().Be(500);
    }

    #endregion

    #region Test Helpers

    private static MessageGenerationRequest CreateRequest(string model = "gpt-4o")
    {
        return new MessageGenerationRequest
        {
            Model = model,
        };
    }

    #endregion
}

using System.ClientModel.Primitives;
using System.Text.Json;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Providers.OpenAI;

namespace IronHive.Tests.Providers;

/// <summary>
/// The Responses API's <c>function_call_output.output</c> is a plain string in this SDK, so a tool result goes out as its
/// text blocks joined by newlines, with a fixed placeholder standing in for each block the string cannot carry.
/// </summary>
public class OpenAIToolResultWireTests
{
    [Fact]
    public void A_tool_result_goes_out_as_its_text_with_non_text_blocks_named_as_omitted()
    {
        var tool = new ToolMessageContent
        {
            Id = "call_1",
            Name = "get_chart",
            Input = "{}",
            Output = ToolOutput.Success(
            [
                new TextMessageContent { Value = "here you go" },
                new ImageMessageContent { Format = ImageFormat.Png, Base64 = "AAAA" },
            ]),
            IsApproved = true,
        };
        var options = OpenAIMessageGenerator.BuildOptions(
            new MessageGenerationRequest { Model = "gpt-4.1", Messages = [Message.User("chart?"), Message.Assistant(tool)] }, null);
        var body = JsonDocument.Parse(ModelReaderWriter.Write(options).ToString()).RootElement;

        var output = body.GetProperty("input").EnumerateArray()
            .Single(i => i.TryGetProperty("type", out var t) && t.GetString() == "function_call_output");
        output.GetProperty("call_id").GetString().Should().Be("call_1");
        output.GetProperty("output").GetString().Should()
            .Be("here you go\n[unsupported content omitted — not supported in this provider's tool-result format]");
    }
}

using System.ClientModel.Primitives;
using System.Text.Json;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Providers.OpenAI;

namespace IronHive.Tests.Providers;

/// <summary>
/// The Responses generator keeps the conversation on the client and sends the whole history every turn, so it
/// asks the vendor not to store the response (the Responses API stores by default) and carries reasoning across
/// turns as the encrypted content the vendor returned, which is what stateless use requires.
/// </summary>
public class OpenAIStatelessRequestTests
{
    private static JsonElement Body(MessageGenerationRequest request)
    {
        var options = OpenAIMessageGenerator.BuildOptions(request, null);
        return JsonDocument.Parse(ModelReaderWriter.Write(options).ToString()).RootElement.Clone();
    }

    [Fact]
    public void Every_request_asks_the_vendor_not_to_store_it()
    {
        var body = Body(new MessageGenerationRequest { Model = "gpt-4.1", Messages = [Message.User("Hi")] });

        body.GetProperty("store").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public void Reasoning_from_an_earlier_turn_goes_back_as_its_encrypted_content()
    {
        var assistant = Message.Assistant(
            new ThinkingMessageContent { Format = ThinkingFormat.Summary, Value = "look up the weather", Signature = "gAAAA-opaque" },
            new TextMessageContent { Value = "Sunny." });
        var body = Body(new MessageGenerationRequest
        {
            Model = "gpt-5-mini",
            Messages = [Message.User("Weather?"), assistant, Message.User("And tomorrow?")],
        });

        var reasoning = body.GetProperty("input").EnumerateArray()
            .Single(i => i.TryGetProperty("type", out var t) && t.GetString() == "reasoning");
        reasoning.GetProperty("encrypted_content").GetString().Should().Be("gAAAA-opaque");
        reasoning.TryGetProperty("id", out _).Should().BeFalse("an item id refers to a stored response, and nothing is stored");
    }

    [Fact]
    public void Reasoning_without_encrypted_content_carries_none()
    {
        var assistant = Message.Assistant(
            new ThinkingMessageContent { Format = ThinkingFormat.Summary, Value = "summary only" });
        var body = Body(new MessageGenerationRequest
        {
            Model = "gpt-5-mini",
            Messages = [Message.User("Q"), assistant, Message.User("Q2")],
        });

        var reasoning = body.GetProperty("input").EnumerateArray()
            .Single(i => i.TryGetProperty("type", out var t) && t.GetString() == "reasoning");
        reasoning.TryGetProperty("encrypted_content", out var enc).Should().BeFalse($"got {enc}");
    }
}

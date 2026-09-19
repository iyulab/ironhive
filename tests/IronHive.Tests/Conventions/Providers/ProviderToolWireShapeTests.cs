using System.ClientModel.Primitives;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using NSubstitute;
using OpenAI.Responses;

namespace IronHive.Tests.Conventions.Providers;

/// <summary>
/// Provider wire-shape roster — the things every provider's request translation must carry, pinned on
/// the request object each provider hands its SDK (no network, no key):
/// (a) a tool's schema reaches the wire whatever shape <see cref="ITool.Parameters"/> arrives in — the
///     Anthropic rows live in <c>AnthropicToolInputSchemaTests</c> (#325: it sent an empty schema for
///     every non-<see cref="JsonObject"/> shape for seven months, unseen without a live key);
/// (b) a provider-private continuity value on a replayed message survives the round trip (Gemini 3
///     <c>thoughtSignature</c> on a function call, Anthropic <c>signature</c> on a thinking block — #326);
/// (c) a non-text tool result is carried natively where the provider can (image block / inlineData) and
///     named, not dropped, where it cannot.
/// </summary>
public class ProviderToolWireShapeTests
{
    private const string Schema = """{"type":"object","properties":{"path":{"type":"string"}},"required":["path"]}""";

    public static TheoryData<string, object?> SchemaShapes => new()
    {
        { "JsonObject", JsonNode.Parse(Schema)!.AsObject() },
        { "JsonElement", JsonDocument.Parse(Schema).RootElement.Clone() },
    };

    private static ITool Tool(object? parameters, string name = "read_file")
    {
        var tool = Substitute.For<ITool>();
        tool.UniqueName.Returns(name);
        tool.Description.Returns("d");
        tool.Parameters.Returns(parameters);
        tool.RequiresApproval.Returns(false);
        return tool;
    }

    private static MessageGenerationRequest Request(string model, ITool? tool = null, params Message[] history) => new()
    {
        Model = model,
        Messages = history.Length > 0 ? [.. history] : [Message.User("Hi")],
        Tools = tool is null ? null : new ToolCollection([tool]),
    };

    private static Message Assistant(params MessageContent[] content)
    {
        var message = new Message { Role = IronHive.Abstractions.Messages.MessageRole.Assistant };
        foreach (var c in content) message.Content.Add(c);
        return message;
    }

    private static ToolMessageContent Call(string id, ToolOutput? output = null, string? signature = null) => new()
    {
        Id = id,
        Name = "read_file",
        Input = """{"path":"a.txt"}""",
        IsApproved = true,
        Output = output,
        Signature = signature,
    };

    private static void AssertObjectSchema(JsonElement schema, string shape)
    {
        schema.GetProperty("type").GetString().Should().Be("object", $"{shape}: the schema type must reach the wire");
        schema.GetProperty("properties").GetProperty("path").GetProperty("type").GetString().Should().Be("string", $"{shape}: the schema body must survive");
    }

    // ── (a) tool schema survives every Parameters shape ──────────────────────────────────────────

    [Theory]
    [MemberData(nameof(SchemaShapes))]
    public void ChatCompletions_ToolSchema_ReachesTheWire(string shape, object? parameters)
    {
        var chatRequest = ChatCompletionMessageGenerator.BuildRequest(Request("gpt-4o", Tool(parameters)));

        var wire = JsonSerializer.Serialize(chatRequest.Tools!.Single().Function.Parameters);
        AssertObjectSchema(JsonDocument.Parse(wire).RootElement, shape);
    }

    [Fact]
    public void ChatCompletions_ToolWithoutParameters_SendsAnEmptyObjectSchema()
    {
        var chatRequest = ChatCompletionMessageGenerator.BuildRequest(Request("gpt-4o", Tool(null)));

        var schema = JsonDocument.Parse(JsonSerializer.Serialize(chatRequest.Tools!.Single().Function.Parameters)).RootElement;
        schema.GetProperty("type").GetString().Should().Be("object");
    }

    [Theory]
    [MemberData(nameof(SchemaShapes))]
    public void GoogleAI_ToolSchema_ReachesTheWire(string shape, object? parameters)
    {
        var generator = new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key" });

        var (_, config) = generator.ToGoogleAIParams(Request("gemini-2.5-flash", Tool(parameters)));

        var declaration = config.Tools!.Single().FunctionDeclarations!.Single();
        var wire = JsonSerializer.Serialize(declaration.ParametersJsonSchema);
        AssertObjectSchema(JsonDocument.Parse(wire).RootElement, shape);
    }

    [Theory]
    [MemberData(nameof(SchemaShapes))]
    public void OpenAIResponses_ToolSchema_ReachesTheWire(string shape, object? parameters)
    {
        var build = typeof(OpenAIMessageGenerator).GetMethod("BuildOptions", BindingFlags.NonPublic | BindingFlags.Static)!;

        var options = (CreateResponseOptions)build.Invoke(null, [Request("gpt-5", Tool(parameters)), null])!;

        var wire = ModelReaderWriter.Write(options.Tools.Single()).ToString();
        var schema = JsonDocument.Parse(wire).RootElement.GetProperty("parameters");
        AssertObjectSchema(schema, shape);
    }

    // ── (b) replayed provider-private continuity values ──────────────────────────────────────────

    [Fact]
    public void GoogleAI_ReplayedFunctionCall_CarriesItsThoughtSignature()
    {
        var generator = new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key" });
        var signature = new byte[] { 7, 7, 7, 7 };
        var history = new[]
        {
            Message.User("read a.txt"),
            Assistant(Call("call-1", ToolOutput.Success("contents"), Convert.ToBase64String(signature))),
        };

        var (contents, _) = generator.ToGoogleAIParams(Request("gemini-3-pro", Tool(null), history));

        var replayed = contents.SelectMany(c => c.Parts ?? []).Single(p => p.FunctionCall is not null);
        replayed.ThoughtSignature.Should().Equal(signature, "Gemini 3 refuses a played-back functionCall without the signature it issued (#326)");
    }

    [Fact]
    public void Anthropic_ReplayedThinkingBlock_KeepsItsSignature()
    {
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });
        var history = new[]
        {
            Message.User("read a.txt"),
            Assistant(
                new ThinkingMessageContent { Value = "let me read it", Signature = "sig-thinking-1" },
                Call("call-1", ToolOutput.Success("contents"))),
        };

        var wire = generator.ToMessageCreateParams(Request("claude-sonnet-4-5", Tool(null), history)).ToString();

        wire.Should().Contain("\"thinking\"").And.Contain("sig-thinking-1",
            "Anthropic requires the assistant's thinking block, with its signature, ahead of a replayed tool_use");
    }

    // Claude 5 thinks adaptively when the request says nothing, and its default display is omitted:
    // the block comes back with empty text and only a signature. Live-measured (claude-sonnet-5): replaying
    // it with the signature is accepted, replaying it without one is a 400 ("each thinking block must
    // contain thinking"), and leaving it out is accepted.

    private static JsonElement[] ReplayedAssistantBlocks(MessageGenerationRequest request)
    {
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });
        using var wire = JsonDocument.Parse(generator.ToMessageCreateParams(request).ToString());
        var root = wire.RootElement.TryGetProperty("messages", out _) ? wire.RootElement : wire.RootElement.EnumerateObject().Select(p => p.Value).First(v => v.ValueKind == JsonValueKind.Object && v.TryGetProperty("messages", out _));
        return [.. root.GetProperty("messages").EnumerateArray()
            .Single(m => m.GetProperty("role").GetString() == "assistant")
            .GetProperty("content").EnumerateArray().Select(b => b.Clone())];
    }

    [Fact]
    public void Anthropic_ReplayedOmittedDisplayThinking_KeepsItsSignatureWithEmptyText()
    {
        var blocks = ReplayedAssistantBlocks(Request("claude-sonnet-5", Tool(null),
            Message.User("read a.txt"),
            Assistant(new ThinkingMessageContent { Value = "", Signature = "sig-omitted" }, Call("call-1", ToolOutput.Success("contents")))));

        blocks[0].GetProperty("type").GetString().Should().Be("thinking");
        blocks[0].GetProperty("signature").GetString().Should().Be("sig-omitted", "an omitted-display block is replayable by its signature alone");
        blocks[0].GetProperty("thinking").GetString().Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("reasoning from another provider")]
    public void Anthropic_ReplayedThinkingWithoutSignature_IsLeftOut(string text)
    {
        var blocks = ReplayedAssistantBlocks(Request("claude-sonnet-5", Tool(null),
            Message.User("read a.txt"),
            Assistant(new ThinkingMessageContent { Value = text }, Call("call-1", ToolOutput.Success("contents")))));

        blocks.Select(b => b.GetProperty("type").GetString()).Should().Equal(["tool_use"],
            "an unsigned thinking block makes the API refuse the whole request, while a turn without it is accepted");
    }

    [Fact]
    public void Anthropic_ReplayedRedactedThinking_IsARedactedBlockWithItsData()
    {
        var blocks = ReplayedAssistantBlocks(Request("claude-sonnet-4-5", Tool(null),
            Message.User("read a.txt"),
            Assistant(
                new ThinkingMessageContent { Format = ThinkingFormat.Secure, Value = "opaque-1" },
                new ThinkingMessageContent { Format = ThinkingFormat.Secure, Value = "" },
                Call("call-1", ToolOutput.Success("contents")))));

        blocks.Select(b => b.GetProperty("type").GetString()).Should().Equal(["redacted_thinking", "tool_use"], "an empty redacted block has nothing to replay");
        blocks[0].GetProperty("data").GetString().Should().Be("opaque-1");
    }

    // ── (c) non-text tool results ────────────────────────────────────────────────────────────────

    [Fact]
    public void Anthropic_ImageToolResult_IsAnImageBlockInsideToolResult()
    {
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });
        var image = new ImageMessageContent { Format = ImageFormat.Png, Base64 = Convert.ToBase64String([1, 2, 3]) };
        var history = new[] { Message.User("look"), Assistant(Call("call-1", ToolOutput.Success([image]))) };

        var wire = generator.ToMessageCreateParams(Request("claude-sonnet-4-5", Tool(null), history)).ToString();

        wire.Should().Contain("\"tool_result\"").And.Contain("\"image\"").And.Contain("image/png",
            "the Messages API carries an image tool result natively; the bridge fix in 0.26.0 is only useful if the provider keeps it");
    }

    [Fact]
    public void GoogleAI_ImageToolResult_IsAnInlineDataPart_OnAGenerationThatTakesIt()
    {
        var generator = new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key" });
        var image = new ImageMessageContent { Format = ImageFormat.Png, Base64 = Convert.ToBase64String([1, 2, 3]) };
        var history = new[] { Message.User("look"), Assistant(Call("call-1", ToolOutput.Success([image]))) };

        var (contents, _) = generator.ToGoogleAIParams(Request("gemini-3-pro", Tool(null), history));

        var response = contents.SelectMany(c => c.Parts ?? []).Single(p => p.FunctionResponse is not null).FunctionResponse!;
        response.Parts.Should().ContainSingle().Which.InlineData!.MimeType.Should().Be("image/png");
    }

    [Fact]
    public void GoogleAI_ImageToolResult_IsNamedInText_OnAGenerationThatRejectsInlineData()
    {
        // #327 — Gemini 2.5 answers 400 "Multimodal function responses are not supported for this model" to inlineData.
        var generator = new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key" });
        var image = new ImageMessageContent { Format = ImageFormat.Png, Base64 = Convert.ToBase64String([1, 2, 3]) };
        var history = new[] { Message.User("look"), Assistant(Call("call-1", ToolOutput.Success([new TextMessageContent { Value = "front" }, image]))) };

        var (contents, _) = generator.ToGoogleAIParams(Request("gemini-2.5-flash", Tool(null), history));

        var response = contents.SelectMany(c => c.Parts ?? []).Single(p => p.FunctionResponse is not null).FunctionResponse!;
        response.Parts.Should().BeNull("no inlineData may reach a model that rejects it");
        var result = response.Response!["result"]!.ToString();
        result.Should().Contain("front").And.Contain("image/png").And.Contain("omitted");
    }

    [Fact]
    public void GoogleAI_MultimodalFunctionResponseCapability_FollowsTheGeneration_AndAnOverrideWins()
    {
        GoogleAIModelCapabilities.Resolve("gemini-2.5-flash").SupportsMultimodalFunctionResponse.Should().BeFalse();
        GoogleAIModelCapabilities.Resolve("gemini-2.0-flash").SupportsMultimodalFunctionResponse.Should().BeFalse();
        GoogleAIModelCapabilities.Resolve("gemini-3-pro").SupportsMultimodalFunctionResponse.Should().BeTrue();

        var overrides = new Dictionary<string, GoogleAIModelCapabilities> { ["gemini-2.5-flash"] = new() { SupportsMultimodalFunctionResponse = true } };
        GoogleAIModelCapabilities.Resolve("gemini-2.5-flash", overrides).SupportsMultimodalFunctionResponse.Should().BeTrue();
    }

    [Fact]
    public void ChatCompletions_ImageToolResult_IsNamedNotDropped()
    {
        var image = new ImageMessageContent { Format = ImageFormat.Png, Base64 = Convert.ToBase64String([1, 2, 3]) };
        var history = new[] { Message.User("look"), Assistant(Call("call-1", ToolOutput.Success([new TextMessageContent { Value = "front" }, image]))) };

        var chatRequest = ChatCompletionMessageGenerator.BuildRequest(Request("gpt-4o", Tool(null), history));

        var toolMessage = chatRequest.Messages.OfType<ToolChatMessage>().Single();
        var text = toolMessage.Content;
        text.Should().Contain("front").And.Contain("omitted", "a tool message is a plain string on this wire, so the image is named rather than silently dropped");
        text.Should().NotContain("AIContent");
    }
}

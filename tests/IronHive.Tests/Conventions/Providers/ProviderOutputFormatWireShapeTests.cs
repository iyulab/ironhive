using System.ClientModel.Primitives;
using System.Reflection;
using System.Text.Json;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using OpenAI.Responses;

namespace IronHive.Tests.Conventions.Providers;

/// <summary>
/// Structured-output wire shape, per provider, for both forms of <see cref="OutputFormat"/>:
/// a schema reaches the wire as that schema, and schemaless JSON mode (<see cref="OutputFormat.Json"/>)
/// reaches it as the provider's JSON mode — never as a synthesized property-less <c>{"type":"object"}</c>
/// schema. Providers that enforce the schema (Gemini, Anthropic) read that one as "an empty object" and
/// answer exactly <c>{}</c> (measured live on both, 2026-09-19).
/// </summary>
public class ProviderOutputFormatWireShapeTests
{
    private const string Schema = """{"type":"object","properties":{"city":{"type":"string"}},"required":["city"]}""";

    private static MessageGenerationRequest Request(string model, OutputFormat format) => new()
    {
        Model = model,
        Messages = [Message.User("Where is the Eiffel Tower? Answer as JSON.")],
        OutputFormat = format,
    };

    // ── GoogleAI ─────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GoogleAI_JsonMode_SendsTheMimeTypeAndNoSchema()
    {
        var generator = new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key" });

        var (_, config) = generator.ToGoogleAIParams(Request("gemini-2.5-flash", OutputFormat.Json));

        config.ResponseMimeType.Should().Be("application/json");
        config.ResponseJsonSchema.Should().BeNull("a schemaless request must not become a property-less schema — Gemini answers with an empty object");
    }

    [Fact]
    public void GoogleAI_Schema_SendsThatSchema()
    {
        var generator = new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "test-key" });

        var (_, config) = generator.ToGoogleAIParams(Request("gemini-2.5-flash", OutputFormat.For(Schema)));

        config.ResponseMimeType.Should().Be("application/json");
        JsonSerializer.Serialize(config.ResponseJsonSchema).Should().Contain("city");
    }

    // ── Anthropic ────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Anthropic_JsonMode_SendsNoFormatAndAsksForJsonInTheSystemPrompt()
    {
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });
        var request = Request("claude-sonnet-5", OutputFormat.Json);
        request.System = "You are terse.";

        var createParams = generator.ToMessageCreateParams(request);
        var wire = createParams.ToString();

        wire.Should().NotContain("json_schema", "the Messages API has no schemaless JSON mode; a property-less schema is answered with an empty object");
        wire.Should().Contain("You are terse.").And.Contain("single JSON object");
    }

    [Fact]
    public void Anthropic_CountTokens_CountsTheJsonModeInstructionToo()
    {
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });
        var request = Request("claude-sonnet-5", OutputFormat.Json);
        request.System = "You are terse.";

        var wire = generator.ToMessageCountTokensParams(request).ToString();

        wire.Should().Contain("You are terse.").And.Contain("single JSON object",
            "the count must cover the system prompt the create call actually sends");
    }

    [Fact]
    public void Anthropic_Schema_SendsThatSchemaAndNoInstruction()
    {
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });

        var wire = generator.ToMessageCreateParams(Request("claude-sonnet-5", OutputFormat.For(Schema))).ToString();

        // format is the {"type":"json_schema","schema":{...}} envelope. Putting the schema itself in the envelope's
        // place made its "type":"object" the format type, and the API rejects that (400) — every structured-output
        // request to Anthropic failed that way until 0.30.0.
        var format = JsonDocument.Parse(wire).RootElement.GetProperty("BodyData").GetProperty("output_config").GetProperty("format");
        format.GetProperty("type").GetString().Should().Be("json_schema");
        format.GetProperty("schema").GetProperty("properties").GetProperty("city").GetProperty("type").GetString().Should().Be("string");
        format.GetProperty("schema").GetProperty("additionalProperties").GetBoolean().Should().BeFalse();
        wire.Should().NotContain("single JSON object", "a schema already constrains the output");
    }

    // ── OpenAI Responses ─────────────────────────────────────────────────────────────────────────

    private static string OpenAITextFormat(OutputFormat format)
    {
        var build = typeof(OpenAIMessageGenerator).GetMethod("BuildOptions", BindingFlags.NonPublic | BindingFlags.Static)!;
        var options = (CreateResponseOptions)build.Invoke(null, [Request("gpt-5", format), null])!;
        return ModelReaderWriter.Write(options.TextOptions.TextFormat).ToString();
    }

    [Fact]
    public void OpenAIResponses_JsonMode_IsJsonObject()
    {
        var wire = JsonDocument.Parse(OpenAITextFormat(OutputFormat.Json)).RootElement;

        wire.GetProperty("type").GetString().Should().Be("json_object");
    }

    [Fact]
    public void OpenAIResponses_Schema_IsJsonSchema()
    {
        var wire = OpenAITextFormat(OutputFormat.For(Schema));

        wire.Should().Contain("\"json_schema\"").And.Contain("city");
    }

    // ── OpenAI-compatible Chat Completions ───────────────────────────────────────────────────────

    [Fact]
    public void ChatCompletions_JsonMode_IsJsonObjectWithNoSchemaMember()
    {
        var chatRequest = ChatCompletionMessageGenerator.BuildRequest(Request("gpt-4o", OutputFormat.Json));

        var wire = JsonDocument.Parse(JsonSerializer.Serialize(chatRequest.ResponseFormat)).RootElement;
        wire.GetProperty("type").GetString().Should().Be("json_object");
        wire.TryGetProperty("json_schema", out _).Should().BeFalse();
    }

    [Fact]
    public void ChatCompletions_Schema_IsJsonSchema()
    {
        var chatRequest = ChatCompletionMessageGenerator.BuildRequest(Request("gpt-4o", OutputFormat.For(Schema)));

        var wire = JsonSerializer.Serialize(chatRequest.ResponseFormat);
        wire.Should().Contain("\"json_schema\"").And.Contain("city");
    }
}

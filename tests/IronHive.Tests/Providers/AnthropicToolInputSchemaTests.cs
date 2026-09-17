using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using IronHive.Providers.Anthropic;
using NSubstitute;

namespace IronHive.Tests.Providers;

/// <summary>
/// Wire-shape facts for the Anthropic tool translation. The Messages API requires
/// <c>tools[].input_schema.type == "object"</c>; a schema that arrives as a <see cref="JsonElement"/>
/// (the shape <c>AIFunction.JsonSchema</c> and MCP tools carry) used to be sent as an empty object,
/// so any consumer with a tool got <c>400 input_schema.type: Field required</c> (#325).
/// </summary>
public class AnthropicToolInputSchemaTests
{
    private const string Schema = """{"type":"object","properties":{"path":{"type":"string"}},"required":["path"]}""";

    private static ITool Tool(string name, object? parameters)
    {
        var tool = Substitute.For<ITool>();
        tool.UniqueName.Returns(name);
        tool.Description.Returns("d");
        tool.Parameters.Returns(parameters);
        tool.RequiresApproval.Returns(false);
        return tool;
    }

    private static string WireJson(object? parameters)
    {
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "test-key" });
        var request = new MessageGenerationRequest
        {
            Model = "claude-sonnet-4-5",
            Messages = [Message.User("Hi")],
            Tools = new ToolCollection([Tool("read_file", parameters)]),
        };

        var createParams = generator.ToMessageCreateParams(request);
        createParams.Tools.Should().ContainSingle();
        // The SDK's own ToString() renders the request body it sends (System.Text.Json on the same
        // model drops the raw dictionary), so the assertion reads the wire shape.
        return createParams.ToString();
    }

    // MessageCreateParams.ToString() renders {"HeaderData":…,"QueryData":…,"BodyData":{…request body…}}.
    private static JsonElement InputSchemaOf(string wire)
        => JsonDocument.Parse(wire).RootElement.GetProperty("BodyData").GetProperty("tools").EnumerateArray().First()
            .GetProperty("input_schema");

    public static TheoryData<string, object> EveryParameterShape => new()
    {
        { "JsonObject", JsonNode.Parse(Schema)!.AsObject() },
        { "JsonElement", JsonDocument.Parse(Schema).RootElement.Clone() },
        { "JsonNode", JsonNode.Parse(Schema)! },
        { "string", Schema },
        { "POCO", new { type = "object", properties = new { path = new { type = "string" } }, required = new List<string> { "path" } } },
    };

    [Theory]
    [MemberData(nameof(EveryParameterShape))]
    public void ToolParameters_ReachTheWireAsObjectSchema_WhateverShapeTheyArriveIn(string shape, object parameters)
    {
        var schema = InputSchemaOf(WireJson(parameters));

        schema.GetProperty("type").GetString().Should().Be("object", $"{shape}: Anthropic requires input_schema.type");
        schema.GetProperty("properties").GetProperty("path").GetProperty("type").GetString().Should().Be("string", $"{shape}: the schema body must survive");
        schema.GetProperty("required").EnumerateArray().Select(e => e.GetString()).Should().Equal("path");
    }

    [Fact]
    public void ToolWithoutParameters_SendsAnEmptyObjectSchema_NotAnEmptyDictionary()
    {
        var schema = InputSchemaOf(WireJson(null));

        schema.GetProperty("type").GetString().Should().Be("object");
        schema.GetProperty("properties").ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    public void SchemaWithoutType_GetsTypeObjectAdded_AndKeepsItsBody()
    {
        var schema = InputSchemaOf(WireJson(JsonNode.Parse("""{"properties":{"q":{"type":"string"}}}""")!.AsObject()));

        schema.GetProperty("type").GetString().Should().Be("object");
        schema.GetProperty("properties").GetProperty("q").GetProperty("type").GetString().Should().Be("string");
    }

    [Fact]
    public void ToInputSchema_NonObjectShapes_FallBackToEmptyObjectSchema()
    {
        foreach (var bad in new object?[] { JsonNode.Parse("[1,2]"), JsonDocument.Parse("\"text\"").RootElement.Clone(), "not json", 42 })
        {
            var dict = AnthropicHelper.ToInputSchema(bad);
            dict["type"].GetString().Should().Be("object", $"{bad?.GetType().Name} is not an object schema");
            dict["properties"].ValueKind.Should().Be(JsonValueKind.Object);
        }
    }
}

using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using IronHive.Abstractions.Messages;

namespace IronHive.Tests.Messages;

public class OutputFormatTests
{
    private sealed class Answer
    {
        public string Value { get; set; } = string.Empty;
    }

    [Fact]
    public void For_Type_BuildsSchemaFromClrType()
    {
        var format = OutputFormat.For<Answer>();

        format.Schema["properties"]!["Value"].Should().NotBeNull();
    }

    [Fact]
    public void For_String_ParsesSchema()
    {
        var format = OutputFormat.For("""{"type":"object"}""");

        format.Schema["type"]!.GetValue<string>().Should().Be("object");
    }

    [Fact]
    public void For_JsonNode_UsesNodeDirectly()
    {
        var node = JsonNode.Parse("""{"type":"object"}""")!;

        var format = OutputFormat.For(node);

        format.Schema.Should().BeSameAs(node);
    }

    [Fact]
    public void For_JsonObject_AcceptedAsJsonNode()
    {
        var obj = new JsonObject { ["type"] = "object" };

        var format = OutputFormat.For(obj);

        format.Schema.Should().BeSameAs(obj);
    }

    [Fact]
    public void For_JsonElement_ConvertsToNode()
    {
        var element = JsonDocument.Parse("""{"type":"object"}""").RootElement;

        var format = OutputFormat.For(element);

        format.Schema["type"]!.GetValue<string>().Should().Be("object");
    }
}

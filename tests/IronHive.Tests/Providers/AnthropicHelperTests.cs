using System.Text.Json.Nodes;
using FluentAssertions;
using IronHive.Providers.Anthropic;

namespace IronHive.Tests.Providers;

/// <summary>
/// Covers AnthropicHelper.ToAnthropicCompatibleSchema, the transform that makes a generic JSON
/// Schema (as produced by OutputFormat.For&lt;T&gt;) safe for Anthropic's structured output, which
/// rejects nullable type unions and requires additionalProperties: false on every object.
/// </summary>
public class AnthropicHelperTests
{
    [Fact]
    public void FlattensNullableUnion_ToSingleType()
    {
        var schema = JsonNode.Parse("""
        {
            "type": "object",
            "properties": {
                "name": { "type": "string" },
                "nickname": { "type": ["string", "null"] }
            },
            "required": ["name", "nickname"]
        }
        """)!;

        var result = AnthropicHelper.ToAnthropicCompatibleSchema(schema);

        result["properties"]!["nickname"]!["type"]!.GetValue<string>().Should().Be("string");
    }

    [Fact]
    public void NullableProperty_ExplicitlyRequiredInSource_StaysRequired()
    {
        // A nullable property that the source schema already marks required keeps that intent
        // (matches Anthropic SDK's own StructuredOutput behavior) -- only nullable properties
        // that were optional in the source get dropped from "required".
        var schema = JsonNode.Parse("""
        {
            "type": "object",
            "properties": {
                "name": { "type": "string" },
                "nickname": { "type": ["string", "null"] }
            },
            "required": ["name", "nickname"]
        }
        """)!;

        var result = AnthropicHelper.ToAnthropicCompatibleSchema(schema);

        result["required"]!.AsArray().Select(n => n!.GetValue<string>())
            .Should().BeEquivalentTo(["name", "nickname"]);
    }

    [Fact]
    public void NullableProperty_NotOriginallyRequired_StaysOutOfRequired()
    {
        var schema = JsonNode.Parse("""
        {
            "type": "object",
            "properties": {
                "name": { "type": "string" },
                "nickname": { "type": ["null", "string"] }
            },
            "required": ["name"]
        }
        """)!;

        var result = AnthropicHelper.ToAnthropicCompatibleSchema(schema);

        result["required"]!.AsArray().Select(n => n!.GetValue<string>())
            .Should().BeEquivalentTo(["name"]);
    }

    [Fact]
    public void SetsAdditionalPropertiesFalse_OnEveryObjectSchema_IncludingNested()
    {
        var schema = JsonNode.Parse("""
        {
            "type": "object",
            "properties": {
                "address": {
                    "type": "object",
                    "properties": {
                        "city": { "type": "string" }
                    }
                }
            }
        }
        """)!;

        var result = AnthropicHelper.ToAnthropicCompatibleSchema(schema);

        result["additionalProperties"]!.GetValue<bool>().Should().BeFalse();
        result["properties"]!["address"]!["additionalProperties"]!.GetValue<bool>().Should().BeFalse();
    }

    [Fact]
    public void AppliesTransform_ToObjectsInsideArrayItems()
    {
        var schema = JsonNode.Parse("""
        {
            "type": "object",
            "properties": {
                "tags": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "label": { "type": ["string", "null"] }
                        }
                    }
                }
            }
        }
        """)!;

        var result = AnthropicHelper.ToAnthropicCompatibleSchema(schema);

        var itemSchema = result["properties"]!["tags"]!["items"]!;
        itemSchema["additionalProperties"]!.GetValue<bool>().Should().BeFalse();
        itemSchema["properties"]!["label"]!["type"]!.GetValue<string>().Should().Be("string");
    }

    [Fact]
    public void DoesNotMutate_OriginalSchema()
    {
        var schema = JsonNode.Parse("""
        {
            "type": "object",
            "properties": {
                "nickname": { "type": ["string", "null"] }
            }
        }
        """)!;

        AnthropicHelper.ToAnthropicCompatibleSchema(schema);

        schema["properties"]!["nickname"]!["type"]!.Should().BeOfType<JsonArray>();
        schema["additionalProperties"].Should().BeNull();
    }
}

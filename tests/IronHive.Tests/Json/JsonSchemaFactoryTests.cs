using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FluentAssertions;
using IronHive.Abstractions.Json;

namespace IronHive.Tests.Json;

// OpenAI-compatible self-hosted backends (llama.cpp/vLLM via GPUStack) convert
// json_schema into a constrained grammar. Non-standard schema shapes (numeric
// union+pattern, enum without type) break that conversion. These tests pin the
// schema output to a clean, spec-compliant shape.
// Ref: claudedocs/issues/ISSUE-ironhive-2026-06-07-jsonschema-llamacpp.md
public class JsonSchemaFactoryTests
{
    [JsonConverter(typeof(JsonStringEnumConverter<Verdict>))]
    public enum Verdict { OK, NG }

    public sealed record InspectionResult
    {
        public required Verdict Verdict { get; init; }
        public string Findings { get; init; } = "";
        public required double Confidence { get; init; }
    }

    public sealed record NumericShapes
    {
        public required double Dbl { get; init; }
        public required float Flt { get; init; }
        public required decimal Dec { get; init; }
        public required int IntNum { get; init; }
        public required long LongNum { get; init; }
    }

    private static JsonObject PropOf(JsonNode schema, string name)
        => schema["properties"]!.AsObject()[name]!.AsObject();

    private static string? TypeString(JsonObject prop)
        => prop["type"] is JsonValue v ? v.GetValue<string>() : null;

    [Fact]
    public void Build_double_emits_single_number_type_without_string_union_or_pattern()
    {
        var schema = JsonSchemaFactory.Build(typeof(NumericShapes));

        var dbl = PropOf(schema, "Dbl");
        TypeString(dbl).Should().Be("number");
        dbl.ContainsKey("pattern").Should().BeFalse();
        (dbl["type"] is JsonArray).Should().BeFalse("numeric types must not be a [string, number] union");
    }

    [Fact]
    public void Build_float_and_decimal_emit_number_type()
    {
        var schema = JsonSchemaFactory.Build(typeof(NumericShapes));

        TypeString(PropOf(schema, "Flt")).Should().Be("number");
        TypeString(PropOf(schema, "Dec")).Should().Be("number");
    }

    [Fact]
    public void Build_integer_emits_integer_type()
    {
        var schema = JsonSchemaFactory.Build(typeof(NumericShapes));

        TypeString(PropOf(schema, "IntNum")).Should().Be("integer");
        TypeString(PropOf(schema, "LongNum")).Should().Be("integer");
    }

    [Fact]
    public void Build_string_enum_includes_type_string_alongside_enum()
    {
        var schema = JsonSchemaFactory.Build(typeof(InspectionResult));

        var verdict = PropOf(schema, "Verdict");
        TypeString(verdict).Should().Be("string");
        verdict["enum"]!.AsArray().Select(n => n!.GetValue<string>())
            .Should().BeEquivalentTo("OK", "NG");
    }

    [Fact]
    public void Build_inspection_result_is_clean_for_grammar_backends()
    {
        var schema = JsonSchemaFactory.Build(typeof(InspectionResult));

        TypeString(PropOf(schema, "Confidence")).Should().Be("number");
        PropOf(schema, "Confidence").ContainsKey("pattern").Should().BeFalse();
        TypeString(PropOf(schema, "Findings")).Should().Be("string");
        TypeString(PropOf(schema, "Verdict")).Should().Be("string");
    }
}

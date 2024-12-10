using System.Text.Json.Serialization;

namespace Raggle.Abstractions.Json;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(StringJsonSchema), "string")]
[JsonDerivedType(typeof(IntegerJsonSchema), "integer")]
[JsonDerivedType(typeof(NumberJsonSchema), "number")]
[JsonDerivedType(typeof(BooleanJsonSchema), "boolean")]
[JsonDerivedType(typeof(ArrayJsonSchema), "array")]
[JsonDerivedType(typeof(ObjectJsonSchema), "object")]
public abstract class JsonSchema
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    public JsonSchema(string? description = null)
    {
        Description = description;
    }
}

public class StringJsonSchema : JsonSchema
{
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("minLength")]
    public int? MinLength { get; set; }

    [JsonPropertyName("maxLength")]
    public int? MaxLength { get; set; }

    [JsonPropertyName("enum")]
    public string[]? Enum { get; set; }

    public StringJsonSchema(string? description = null) : base(description) { }
}

public class IntegerJsonSchema : JsonSchema
{
    [JsonPropertyName("format")]
    public required string Format { get; set; }

    public IntegerJsonSchema(string? description = null) : base(description) { }
}

public class NumberJsonSchema : JsonSchema
{
    [JsonPropertyName("format")]
    public required string Format { get; set; }

    public NumberJsonSchema(string? description = null) : base(description) { }
}

public class BooleanJsonSchema : JsonSchema
{
    public BooleanJsonSchema(string? description = null) : base(description) { }
}

public class ArrayJsonSchema : JsonSchema
{
    private object? _items;

    [JsonPropertyName("items")]
    public object? Items
    {
        get
        {
            return _items;
        }
        set
        {
            _items = value is JsonSchema || value is JsonSchema[]? value
                : throw new ArgumentException("Items must be a JsonSchema or Array of JsonSchema");
        }
    }

    public ArrayJsonSchema(string? description = null) : base(description) { }
}

public class ObjectJsonSchema : JsonSchema
{
    [JsonPropertyName("properties")]
    public IDictionary<string, JsonSchema>? Properties { get; set; }

    [JsonPropertyName("required")]
    public string[]? Required { get; set; }

    [JsonPropertyName("additionalProperties")]
    public JsonSchema? AdditionalProperties { get; set; }

    public ObjectJsonSchema(string? description = null) : base(description) { }
}

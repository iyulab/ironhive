using Json.Schema;

namespace Microsoft.OpenApi;

public static class OpenApiSchemaExtensions
{
    /// <summary>
    /// OpenAPI 스키마를 JsonSchema로 변환합니다.
    /// </summary>
    public static JsonSchema ToJsonSchema(this IOpenApiSchema schema)
    {
        return schema.ToJsonSchemaBuilder().Build();
    }

    /// <summary>
    /// OpenAPI 스키마를 JsonSchemaBuilder로 변환합니다.
    /// </summary>
    public static JsonSchemaBuilder ToJsonSchemaBuilder(this IOpenApiSchema schema)
    {
        var builder = new JsonSchemaBuilder();

        if (schema.Type is not null)
        {
            var type = schema.Type switch
            {
                JsonSchemaType.String => SchemaValueType.String,
                JsonSchemaType.Integer => SchemaValueType.Integer,
                JsonSchemaType.Number => SchemaValueType.Number,
                JsonSchemaType.Boolean => SchemaValueType.Boolean,
                JsonSchemaType.Array => SchemaValueType.Array,
                JsonSchemaType.Object => SchemaValueType.Object,
                _ => SchemaValueType.Null,
            };
            builder = builder.Type(type);
        }

        if (!string.IsNullOrWhiteSpace(schema.Description))
            builder = builder.Description(schema.Description);

        if (!string.IsNullOrWhiteSpace(schema.Format))
            builder = builder.Format(schema.Format);

        if (schema.Enum is { Count: > 0 })
            builder = builder.Enum(schema.Enum.Select(e => e.ToString()!).ToArray());

        if (schema.Required is { Count: > 0 })
            builder = builder.Required(schema.Required.ToArray());

        if (schema.Properties is { Count: > 0 })
        {
            builder = builder.Properties(schema.Properties.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.ToJsonSchemaBuilder()));
        }

        if (schema.AdditionalProperties is not null)
            builder = builder.AdditionalProperties(schema.AdditionalProperties.ToJsonSchemaBuilder());

        if (schema.AllOf is { Count: > 0 })
            builder = builder.AllOf(schema.AllOf.Select(s => s.ToJsonSchemaBuilder()).ToArray());
        if (schema.AnyOf is { Count: > 0 })
            builder = builder.AnyOf(schema.AnyOf.Select(s => s.ToJsonSchemaBuilder()).ToArray());
        if (schema.OneOf is { Count: > 0 })
            builder = builder.OneOf(schema.OneOf.Select(s => s.ToJsonSchemaBuilder()).ToArray());

        return builder;
    }
}
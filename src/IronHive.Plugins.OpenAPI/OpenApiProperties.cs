using Json.Schema;
using Microsoft.OpenApi;

namespace IronHive.Plugins.OpenAPI;

/// <summary>
/// 각 Operation에 대한 요청 파라미터들을 그룹화한 클래스입니다.
/// </summary>
public class OpenApiProperties
{
    /// <summary> 요청 파라미터 중 Path에 해당하는 항목들입니다. </summary>
    public IDictionary<string, IOpenApiParameter> Path { get; set; }
        = new Dictionary<string, IOpenApiParameter>(StringComparer.Ordinal);

    /// <summary> 요청 파라미터 중 Query에 해당하는 항목들입니다. </summary>
    public IDictionary<string, IOpenApiParameter> Query { get; set; }
        = new Dictionary<string, IOpenApiParameter>(StringComparer.Ordinal);

    /// <summary> 요청 파라미터 중 Header에 해당하는 항목들입니다. </summary>
    public IDictionary<string, IOpenApiParameter> Header { get; set; }
        = new Dictionary<string, IOpenApiParameter>(StringComparer.OrdinalIgnoreCase);

    /// <summary> 요청 파라미터 중 Cookie에 해당하는 항목들입니다. </summary>
    public IDictionary<string, IOpenApiParameter> Cookie { get; set; }
        = new Dictionary<string, IOpenApiParameter>(StringComparer.Ordinal);

    /// <summary> 요청 본문(Request Body)에 해당하는 항목입니다. </summary>
    public IOpenApiRequestBody? Body { get; set; }

    /// <summary>
    /// 현재 설정된 요청 파라미터들을 기반으로 JsonSchema를 생성합니다.
    /// </summary>
    public JsonSchema? ToJsonSchema()
    {
        var builder = new JsonSchemaBuilder().Type(SchemaValueType.Object);
        var properties = new Dictionary<string, JsonSchema>();
        var required = new List<string>();

        if (Query is { Count: > 0 })
        {
            properties["query"] = BuildParametersSchema(Query);
            if (Query.Values.Any(p => p.Required))
                required.Add("query");
        }
        if (Path is { Count: > 0 })
        {
            properties["path"] = BuildParametersSchema(Path);
            if (Path.Values.Any(p => p.Required))
                required.Add("path");
        }
        if (Header is { Count: > 0 })
        {
            properties["header"] = BuildParametersSchema(Header);
            if (Header.Values.Any(p => p.Required))
                required.Add("header");
        }
        if (Cookie is { Count: > 0 })
        {
            properties["cookie"] = BuildParametersSchema(Cookie);
            if (Cookie.Values.Any(p => p.Required))
                required.Add("cookie");
        }

        if (Body is not null && Body.Content is { Count: > 0 })
        {
            properties["body"] = BuildRequestBodySchema(Body);
            if (Body.Required)
                required.Add("body");
        }

        if (properties.Count > 0)
            builder = builder.Properties(properties);
        if (required.Count > 0)
            builder = builder.Required(required.ToArray());
        return builder.Build();
    }

    /// <summary> 여러 요청 파라미터들의 JsonSchema를 생성합니다. </summary>
    private static JsonSchema BuildParametersSchema(IDictionary<string, IOpenApiParameter> parameters)
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(parameters.ToDictionary(kv => kv.Key, kv => kv.Value.Schema?.ToJsonSchema() ?? JsonSchema.Empty))
            .Required(parameters.Where(kv => kv.Value.Required).Select(kv => kv.Key).ToArray())
            .Build();
    }

    /// <summary> RequestBody의 JsonSchema를 생성합니다. </summary>
    private static JsonSchema BuildRequestBodySchema(IOpenApiRequestBody body)
    {
        if (body.Content is null || body.Content.Count == 0)
            return JsonSchema.Empty;

        // 스키마 하나만 지원
        // 우선순위: application/json > text/plain > 기타
        var contentType = body.Content.ContainsKey("application/json") ? "application/json"
                        : body.Content.ContainsKey("text/plain") ? "text/plain"
                        : body.Content.Keys.First();
        var media = body.Content[contentType];

        if (media.Schema is not null)
            return media.Schema.ToJsonSchema();
        else
            return new JsonSchemaBuilder().Type(SchemaValueType.String).Build();
    }
}

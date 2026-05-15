using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Json;

/// <summary>
/// JSON 스키마 생성을 위한 팩토리 클래스입니다.
/// </summary>
public static class JsonSchemaFactory
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        MaxDepth = 32,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        TypeInfoResolver = JsonSerializerOptions.Default.TypeInfoResolver,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private static readonly JsonSchemaExporterOptions _schemaOptions = new()
    {
        TreatNullObliviousAsNonNullable = true,
        TransformSchemaNode = HandleTransform,
    };

    /// <summary>
    /// 지정된 타입의 JSON 스키마를 생성합니다.
    /// options를 생략하면 non-nullable + description 기본 옵션이 적용됩니다.
    /// </summary>
    public static JsonNode Build(Type type, JsonSchemaExporterOptions? options = null)
    {
        return _jsonOptions.GetJsonSchemaAsNode(type, options ?? _schemaOptions);
    }

    /// <summary>
    /// [Description] / [Display] 어트리뷰트를 스키마 노드의 "description" 필드로 삽입합니다.
    /// JsonSchemaExporterOptions.TransformSchemaNode에 조합하여 사용합니다.
    /// </summary>
    public static JsonNode HandleTransform(JsonSchemaExporterContext ctx, JsonNode node)
    {
        var attrs = ctx.PropertyInfo?.AttributeProvider?.GetCustomAttributes(true);
        if (!TryGetDescription(attrs ?? [], out var description))
            return node;

        if (node is not JsonObject jObj)
        {
            node = jObj = new JsonObject();
            if (node.GetValueKind() is JsonValueKind.False)
                jObj.Add("not", true);
        }
        jObj.Insert(0, "description", description);
        return node;
    }

    private static bool TryGetDescription(object[] attributes, out string description)
    {
        var descAttr = attributes.OfType<DescriptionAttribute>().FirstOrDefault();
        var displayAttr = attributes.OfType<DisplayAttribute>().FirstOrDefault();
        description = descAttr?.Description ?? displayAttr?.Description ?? string.Empty;
        return !string.IsNullOrWhiteSpace(description);
    }
}

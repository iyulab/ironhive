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
    // NOTE: this options instance is used ONLY for schema generation
    // (GetJsonSchemaAsNode below), never to (de)serialize payloads. It must not
    // enable NumberHandling.AllowReadingFromString: that flag makes the exporter
    // emit numeric properties as a {"type":["string","number"], "pattern":"..."}
    // union, which grammar-enforcing OpenAI-compatible backends (llama.cpp/vLLM
    // via GPUStack) cannot constrain. Response-parsing leniency lives elsewhere.
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        MaxDepth = 32,
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
        // Normalize string-backed enums regardless of description: the exporter
        // emits {"enum":[...]} without a "type", which some grammar-enforcing
        // OpenAI-compatible backends reject. Add "type":"string" so the schema
        // matches the OpenAI structured-outputs shape {"type":"string","enum":[...]}.
        NormalizeStringEnum(node);

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

    /// <summary>
    /// 문자열 enum 노드(<c>{"enum":[...]}</c>)에 <c>"type":"string"</c>를 보강합니다.
    /// 이미 type이 있거나 enum 값에 비문자열이 섞이면(정수 backed enum 등) 변경하지 않습니다.
    /// </summary>
    private static void NormalizeStringEnum(JsonNode node)
    {
        if (node is not JsonObject obj)
            return;
        if (obj.ContainsKey("type"))
            return;
        if (obj["enum"] is not JsonArray values || values.Count == 0)
            return;

        foreach (var value in values)
        {
            if (value is not JsonValue v || v.GetValueKind() != JsonValueKind.String)
                return;
        }

        obj.Insert(0, "type", "string");
    }

    private static bool TryGetDescription(object[] attributes, out string description)
    {
        var descAttr = attributes.OfType<DescriptionAttribute>().FirstOrDefault();
        var displayAttr = attributes.OfType<DisplayAttribute>().FirstOrDefault();
        description = descAttr?.Description ?? displayAttr?.Description ?? string.Empty;
        return !string.IsNullOrWhiteSpace(description);
    }
}

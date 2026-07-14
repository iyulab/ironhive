using System.Text.Json;
using System.Text.Json.Nodes;
using IronHive.Abstractions.Json;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 구조화 출력 설정입니다.
/// MessageRequest.OutputFormat에 null이 아닌 값을 지정하면 활성화됩니다.
/// </summary>
public sealed class OutputFormat
{
    private OutputFormat(JsonNode schema) => Schema = schema;

    /// <summary>
    /// 출력을 구속하는 JSON 스키마입니다.
    /// </summary>
    public JsonNode Schema { get; }

    /// <summary>
    /// C# 타입으로부터 JSON 스키마를 생성해 출력을 구조화합니다.
    /// </summary>
    public static OutputFormat For<T>() => new(JsonSchemaFactory.Build(typeof(T)));

    /// <summary>
    /// JSON 스키마 문자열을 직접 지정해 출력을 구조화합니다.
    /// </summary>
    public static OutputFormat For(string json) => new(JsonNode.Parse(json)!);

    /// <summary>
    /// JSON 스키마 노드(JsonObject 포함)를 직접 지정해 출력을 구조화합니다.
    /// </summary>
    public static OutputFormat For(JsonNode json) => new(json);

    /// <summary>
    /// JSON 스키마 엘리먼트를 직접 지정해 출력을 구조화합니다.
    /// </summary>
    public static OutputFormat For(JsonElement json) => new(JsonSerializer.SerializeToNode(json)!);
}

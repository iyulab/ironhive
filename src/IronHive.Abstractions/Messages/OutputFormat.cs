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
    private OutputFormat(JsonNode? schema) => Schema = schema;

    /// <summary>
    /// 출력을 구속하는 JSON 스키마입니다. <see langword="null"/> 이면 스키마 없는 JSON 모드(<see cref="Json"/>)입니다 —
    /// 「JSON 객체 하나로 답하라」만 요구하고 모양은 구속하지 않습니다.
    /// </summary>
    public JsonNode? Schema { get; }

    /// <summary>
    /// 스키마 없는 JSON 모드입니다. 각 provider 의 네이티브 JSON 모드로 번역됩니다(OpenAI <c>json_object</c>,
    /// Gemini <c>responseMimeType</c> 만). 속성 없는 <c>{"type":"object"}</c> 스키마로 대신하지 않습니다 —
    /// 구조화 출력을 강제하는 provider(Gemini · Anthropic)는 그 스키마를 「빈 객체」로 읽어 <c>{}</c> 만 답합니다.
    /// </summary>
    public static OutputFormat Json { get; } = new(null);

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

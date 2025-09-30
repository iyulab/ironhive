using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.GenerateContent.Models;

/// <summary>함수 선언(이름/설명/파라미터 스키마).</summary>
internal sealed class FunctionTool
{
    /// <summary>함수명(영문/숫자/언더스코어/대시, 최대 63자).</summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>함수 설명(용도/예시를 충분히 기술 권장).</summary>
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    /// <summary>입력 파라미터(JSON Schema 서브셋).</summary>
    [JsonPropertyName("behavior")]
    public BehaviorType? Behavior { get; set; }

    /// <summary>입력 파라미터(JSON Schema).</summary>
    [JsonPropertyName("parameters")]
    public object? Parameters { get; set; }

    /// <summary>입력 파라미터(JSON Schema).</summary>
    [JsonPropertyName("parametersJsonSchema")]
    public object? ParametersJsonSchema { get; set; }

    /// <summary> 출력 파라미터(JSON Schema 서브셋).</summary>
    [JsonPropertyName("Response")]
    public object? Response { get; set; }

    /// <summary> 출력 파라미터(JSON Schema).</summary>
    [JsonPropertyName("ResponseJsonSchema")]
    public object? ResponseJsonSchema { get; set; }

    internal enum BehaviorType
    {
        UNSPECIFIED,
        BLOCKING,
        NON_BLOCKING
    }
}
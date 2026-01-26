using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

/// <summary>
/// 함수 선언(이름/설명/파라미터 스키마).
/// </summary>
internal sealed class FunctionTool
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("behavior")]
    public BehaviorType? Behavior { get; set; }

    /// <summary>입력 파라미터(Open API 3.03).</summary>
    [JsonPropertyName("parameters")]
    public object? Parameters { get; set; }

    /// <summary>입력 파라미터의 JSON 스키마.</summary>
    [JsonPropertyName("parametersJsonSchema")]
    public object? ParametersJsonSchema { get; set; }

    /// <summary>출력 응답(Open API 3.03)</summary>
    [JsonPropertyName("Response")]
    public object? Response { get; set; }

    /// <summary>출력 응답의 JSON 스키마.</summary>
    [JsonPropertyName("ResponseJsonSchema")]
    public object? ResponseJsonSchema { get; set; }

    internal enum BehaviorType
    {
        UNSPECIFIED,
        /// <summary>대화중 응답을 기다립니다.</summary>
        BLOCKING,
        /// <summary>비동기적으로 처리됩니다.</summary>
        NON_BLOCKING
    }
}
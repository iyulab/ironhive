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

    /// <summary>입력 파라미터(JSON Schema).</summary>
    [JsonPropertyName("parameters")]
    public object? Parameters { get; set; }

    [JsonPropertyName("parametersJsonSchema")]
    public object? ParametersJsonSchema { get; set; }

    [JsonPropertyName("Response")]
    public object? Response { get; set; }

    [JsonPropertyName("ResponseJsonSchema")]
    public object? ResponseJsonSchema { get; set; }

    internal enum BehaviorType
    {
        UNSPECIFIED,
        BLOCKING,
        NON_BLOCKING
    }
}
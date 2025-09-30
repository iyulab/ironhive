using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Share.Models;

/// <summary>
/// 예측된 함수 호출(모델→앱 호출).
/// </summary>
internal sealed class FunctionCall
{
    /// <summary>함수 호출 식별자(멀티콜 대응).</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>함수명(도구 선언의 name과 일치).</summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>JSON 오브젝트 형태의 인자.</summary>
    [JsonPropertyName("args")]
    public object? Args { get; set; }
}

/// <summary>
/// 함수 호출의 실행 결과(앱→모델 피드백).
/// </summary>
internal sealed class FunctionResponse
{
    /// <summary>대응되는 FunctionCall의 id(있다면).</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>함수명.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>함수 결과(JSON 오브젝트 형식).</summary>
    [JsonPropertyName("response")]
    public required JsonObject Response { get; set; }

    /// <summary> 함수 실행이 더 필요한지 여부(비동기 함수 호출 시).</summary>
    [JsonPropertyName("willContinue")]
    public bool? WillContinue { get; set; }

    /// <summary> 함수 실행 결과를 대화 컨텍스트에 추가하는 방식 지정(기본 SILENT).</summary>
    [JsonPropertyName("scheduling")]
    public SchedulingType? Scheduling { get; set; }

    internal enum SchedulingType
    {
        SCHEDULING_UNSPECIFIED,
        // 대화 컨텍스트에 결과를 추가하지만, 현재 생성 중인 응답에는 반영하지 않음.
        SILENT,
        // 대화 컨텍스트에 결과를 추가하고, 현재 생성 중인 응답에 반영(가능시)함.
        WHEN_IDLE,
        // 대화 컨텍스트에 결과를 추가하고, 현재 생성 중인 응답에 반드시 반영함.
        INTERRUPT
    }
}
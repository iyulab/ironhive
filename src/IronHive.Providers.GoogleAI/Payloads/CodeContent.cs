using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads;

/// <summary>코드 실행용 Part - 실행 코드.</summary>
internal sealed class ExecutableCode
{
    /// <summary> "LANGUAGE_UNSPECIFIED", "PYTHON" </summary>
    [JsonPropertyName("language")]
    public required string Language { get; set; }

    /// <summary>실행 코드 원문.</summary>
    [JsonPropertyName("code")]
    public required string Code { get; set; }
}

/// <summary>코드 실행 결과.</summary>
internal sealed class ExecutionResult
{
    /// <summary>
    /// 결과 상태(성공/실패 등)
    /// </summary>
    [JsonPropertyName("outcome")]
    public required Status Outcome { get; set; }

    /// <summary>표준 출력.</summary>
    [JsonPropertyName("output")]
    public string? Output { get; set; }

    internal enum Status
    {
        OUTCOME_UNSPECIFIED,
        OUTCOME_OK,
        OUTCOME_FAILED,
        OUTCOME_DEADLINE_EXCEEDED
    }
}
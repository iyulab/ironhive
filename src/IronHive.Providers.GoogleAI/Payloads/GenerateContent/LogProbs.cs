using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

/// <summary>토큰 로그확률 결과.</summary>
internal sealed class LogprobsResult
{
    /// <summary>각 디코딩 스텝별 상위 후보들.</summary>
    [JsonPropertyName("topCandidates")]
    public ICollection<TopCandidates>? TopCandidates { get; set; }

    /// <summary>각 스텝에서 실제 선택된 후보.</summary>
    [JsonPropertyName("chosenCandidates")]
    public ICollection<LogprobCandidate>? Candidates { get; set; }

    /// <summary>모든 토크의 로그 확률 합계입니다.</summary>
    [JsonPropertyName("logProbabilitySum")]
    public float? LogProbabilitySum { get; set; }
}

internal sealed class TopCandidates
{
    /// <summary>로그확률 내림차순 정렬된 후보들.</summary>
    [JsonPropertyName("candidates")]
    public ICollection<LogprobCandidate>? Candidates { get; set; }
}

internal sealed class LogprobCandidate
{
    /// <summary>토큰 문자열.</summary>
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    /// <summary>토큰 ID.</summary>
    [JsonPropertyName("tokenId")]
    public int? TokenId { get; set; }

    /// <summary>로그확률 값.</summary>
    [JsonPropertyName("logProbability")]
    public float? LogProbability { get; set; }
}
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Audio;

/// <summary>
/// OpenAI Transcription 응답
/// </summary>
public class CreateTranscriptionResponse
{
    /// <summary>
    /// 변환된 텍스트
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 토큰별 로그 확률 정보 (include=["logprobs"] 요청 시 포함)
    /// </summary>
    [JsonPropertyName("logprobs")]
    public TranscriptionLogprob[]? Logprobs { get; set; }

    /// <summary>
    /// API 사용량 정보 (토큰 수 또는 처리 시간)
    /// </summary>
    [JsonPropertyName("usage")]
    public TranscriptionUsage? Usage { get; set; }

    /// <summary>
    /// 오디오 전체 길이 (초)
    /// </summary>
    [JsonPropertyName("duration")]
    public float? Duration { get; set; }

    /// <summary>
    /// 감지된 언어 코드 (ISO-639-1)
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// 타임스탬프가 포함된 세그먼트 목록 (response_format="verbose_json" 요청 시 포함)
    /// </summary>
    [JsonPropertyName("segments")]
    public IEnumerable<TranscriptionSegment>? Segments { get; set; }

    /// <summary>
    /// 단어별 타임스탬프 목록 (timestamp_granularities=["word"] 요청 시 포함)
    /// </summary>
    [JsonPropertyName("words")]
    public IEnumerable<TranscriptionWord>? Words { get; set; }
}

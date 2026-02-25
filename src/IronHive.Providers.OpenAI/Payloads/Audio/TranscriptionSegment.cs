using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Audio;

/// <summary>
/// 오디오의 특정 시간 구간별 전사 결과를 나타냅니다.
/// <para>이 객체는 DiarizedSegment와 VerboseSegment를 통합한 형태로 일부 필드가 제외 또는 추가되었습니다.</para>
/// </summary>
public class TranscriptionSegment
{
    /// <summary>
    /// 세그먼트 종료 시간 (초)
    /// </summary>
    [JsonPropertyName("end")]
    public float End { get; set; }

    /// <summary>
    /// 발화자 식별자
    /// </summary>
    [JsonPropertyName("speaker")]
    public string? Speaker { get; set; }

    /// <summary>
    /// 세그먼트 시작 시간 (초)
    /// </summary>
    [JsonPropertyName("start")]
    public float Start { get; set; }

    /// <summary>
    /// 전사된 텍스트
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Audio;

/// <summary>
/// 단어별 타임스탬프 정보
/// <para>timestamp_granularities=["word"] 요청 시 반환됩니다.</para>
/// </summary>
public class TranscriptionWord
{
    /// <summary>
    /// 단어 종료 시간 (초)
    /// </summary>
    [JsonPropertyName("end")]
    public float End { get; set; }

    /// <summary>
    /// 단어 시작 시간 (초)
    /// </summary>
    [JsonPropertyName("start")]
    public float Start { get; set; }

    /// <summary>
    /// 전사된 단어
    /// </summary>
    [JsonPropertyName("word")]
    public string? Word { get; set; }
}

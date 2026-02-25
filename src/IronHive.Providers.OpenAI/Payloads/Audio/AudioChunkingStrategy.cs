using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Audio;

/// <summary>
/// 오디오 청킹(분할) 전략 설정
/// </summary>
public class AudioChunkingStrategy
{
    /// <summary>
    /// 청킹 전략 타입 (기본값: "server_vad" - 서버 측 음성 활동 감지)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "server_vad";

    /// <summary>
    /// 각 청크 앞에 추가할 패딩 시간 (밀리초)
    /// </summary>
    [JsonPropertyName("prefix_padding_ms")]
    public int? PrefixPaddingMs { get; set; }

    /// <summary>
    /// 청크를 분리하기 위한 침묵 구간 길이 (밀리초)
    /// </summary>
    [JsonPropertyName("silence_duration_ms")]
    public int? SilenceDurationMs { get; set; }

    /// <summary>
    /// 클립 병합 임계값 (0.0 ~ 1.0)
    /// <para>클립 간 유사도가 이 임계값을 초과하면 클립이 병합됩니다.</para>
    /// </summary>
    [JsonPropertyName("threshold")]
    public double? Threshold { get; set; }
}

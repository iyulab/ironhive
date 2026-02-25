using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Audio;

/// <summary>
/// OpenAI Text-to-Speech 요청
/// </summary>
public class CreateSpeechRequest
{
    /// <summary>
    /// 변환할 텍스트 (최대 4096자)
    /// </summary>
    [JsonPropertyName("input")]
    public required string Input { get; set; }

    /// <summary>
    /// 모델 ID 
    /// <para>"tts-1", "tts-1-hd", "gpt-4o-mini-tts"</para>
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// 사용할 음성
    /// <para>"alloy", "ash", "ballad", "coral", "echo", "sage", "shimmer", "verse", "marin", "cedar"</para>
    /// </summary>
    [JsonPropertyName("voice")]
    public required string Voice { get; set; }

    /// <summary>
    /// 생성된 오디오의 음성을 제어하기 위한 추가 지시사항
    /// <para>tts-1 또는 tts-1-hd 모델에서는 작동하지 않습니다.</para>
    /// </summary>
    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    /// <summary>
    /// 출력 포맷
    /// <para>"mp3", "opus", "aac", "flac", "wav", "pcm"</para>
    /// </summary>
    [JsonPropertyName("response_format")]
    public string? ResponseFormat { get; set; }

    /// <summary>
    /// 재생 속도 (0.25 ~ 4.0)
    /// </summary>
    [JsonPropertyName("speed")]
    public float? Speed { get; set; }

    /// <summary>
    /// 스트리밍 응답 형식 (sse, audio)
    /// </summary>
    [JsonPropertyName("stream_format")]
    public string? StreamFormat { get; set; }
}

namespace IronHive.Abstractions.Audio;

/// <summary>
/// 음성을 텍스트로 변환 요청 (STT).
/// </summary>
public class SpeechToTextRequest
{
    /// <summary>
    /// 모델 ID (e.g. "whisper-1", "gpt-4o-transcribe", "gpt-4o-mini-transcribe" for OpenAI)
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// 오디오 데이터 (mp3, wav, flac, ogg, webm 등)
    /// </summary>
    public required GeneratedAudio Audio { get; set; }
}

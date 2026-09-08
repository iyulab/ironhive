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

    /// <summary>
    /// 화자 분리(diarization)를 요청할지 여부입니다. (Default: false)
    /// </summary>
    /// <remarks>
    /// true인 경우 <see cref="SpeechToTextResponse.Segments"/>에 화자별로 분리된 세그먼트가 채워집니다.
    /// 이 값을 지원하지 않는 provider/모델은 무시할 수 있습니다.
    /// </remarks>
    public bool Diarized { get; set; }
}

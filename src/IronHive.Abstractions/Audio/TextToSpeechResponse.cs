namespace IronHive.Abstractions.Audio;

/// <summary>
/// TTS 응답. 바이너리 오디오 데이터를 포함합니다.
/// </summary>
public class TextToSpeechResponse
{
    /// <summary>
    /// 생성된 오디오
    /// </summary>
    public required GeneratedAudio Audio { get; set; }
}

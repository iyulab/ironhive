namespace IronHive.Abstractions.Audio;

/// <summary>
/// 텍스트를 음성으로 변환 요청 (TTS).
/// </summary>
public class TextToSpeechRequest
{
    /// <summary>
    /// 모델 ID 
    /// <para>OpenAI: "gpt-4o-mini-tts", "gpt-4o-tts", "tts-1"</para>
    /// <para>Google: "gemini-2.5-flash-preview-tts", "gemini-2.5-pro-preview-tts"</para>
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// 변환할 텍스트
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// 음성 ID
    /// <para>OpenAI: "nova", "clio", "lyra", "orion"</para>
    /// <para>Google: "Zephyr", "Charon", "Callirrhoe", "Autonoe"</para>
    /// </summary>
    public required string Voice { get; set; }
}

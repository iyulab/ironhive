using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions.Messages.Content;

/// <summary>
/// 오디오 콘텐츠 블록을 나타냅니다
/// </summary>
public class AudioMessageContent : MessageContent
{
    /// <summary>
    /// 오디오 형식을 나타냅니다.
    /// </summary>
    public required AudioFormat Format { get; set; }

    /// <summary>
    /// base64 인코딩된 오디오 데이터를 나타냅니다
    /// </summary>
    public required string Base64 { get; set; }
}

/// <summary>
/// 오디오 콘텐츠 블록의 형식을 나타내는 열거형입니다.
/// </summary>
public enum AudioFormat
{
    /// <summary>
    /// WAV 오디오
    /// </summary>
    Wav,

    /// <summary>
    /// MP3 오디오
    /// </summary>
    Mp3,

    /// <summary>
    /// FLAC 오디오
    /// </summary>
    Flac,

    /// <summary>
    /// AAC 오디오
    /// </summary>
    Aac,

    /// <summary>
    /// OGG 오디오
    /// </summary>
    Ogg,

    /// <summary>
    /// AIFF 오디오
    /// </summary>
    Aiff
}

namespace IronHive.Abstractions.Audio;

/// <summary>
/// 생성된 오디오 데이터
/// </summary>
public class GeneratedAudio
{
    /// <summary>
    /// "audio/flac", "audio/mp3", "audio/ogg", "audio/wav" 등
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// 오디오 바이너리 데이터
    /// </summary>
    public byte[] Data { get; set; } = [];
}

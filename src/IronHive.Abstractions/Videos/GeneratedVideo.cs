namespace IronHive.Abstractions.Videos;

/// <summary>
/// 생성된 비디오를 나타냅니다.
/// </summary>
public class GeneratedVideo
{
    /// <summary>
    /// 생성된 비디오의 컨텐츠 타입 (예: "video/mp4")
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// 비디오 바이너리 데이터
    /// </summary>
    public byte[] Data { get; set; } = [];
}

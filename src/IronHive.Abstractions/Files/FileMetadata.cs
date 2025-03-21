namespace IronHive.Abstractions.Files;

/// <summary>
/// 파일 메타데이터를 나타내는 클래스입니다.
/// FileInfo, FileDetails, FileProfile, FileOverview
/// </summary>
public class FileMetadata
{
    /// <summary>
    /// 파일 이름을 나타냅니다.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// 파일의 MIME 타입을 나타냅니다.
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// 파일 크기를 바이트 단위로 나타냅니다.
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// 파일의 마지막 수정 일자를 나타냅니다.
    /// </summary>
    public DateTimeOffset? LastModifiedDate { get; set; }
}

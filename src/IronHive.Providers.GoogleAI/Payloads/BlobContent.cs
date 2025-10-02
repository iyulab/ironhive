using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads;

/// <summary>
/// 인라인 바이트 데이터.
/// </summary>
internal sealed class BlobData
{
    /// <summary>
    /// image/png, image/jpeg, image/webp, image/heic, image/heif
    /// video/mp4, video/mpeg, video/mov, video/avi, video/x-flv, video/mpg, video/webm, video/wmv, video/3gpp
    /// audio/wav, audio/mp3, audio/aiff, audio/aac, audio/ogg, audio/flac
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Base64 인코딩된 데이터.
    /// </summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }
}

/// <summary>
/// 파일 참조(URI 기반).
/// </summary>
internal sealed class FileData
{
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Files API로 업로드한 File URI or Youtube 등의 공개 URL.
    /// </summary>
    [JsonPropertyName("fileUri")]
    public required string FileUri { get; set; }
}
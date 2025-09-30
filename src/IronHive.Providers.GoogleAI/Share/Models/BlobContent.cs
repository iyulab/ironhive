using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Share.Models;

/// <summary>인라인 바이트 데이터.</summary>
internal sealed class BlobData
{
    /// <summary>
    /// MIME 타입. 지원: "image/png", "image/jpeg", ...
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>Base64 인코딩된 데이터.</summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }
}

/// <summary>파일 참조(URI 기반).</summary>
internal sealed class FileData
{
    /// <summary>MIME 타입(가능시).</summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>Files API가 반환하는 file URI.</summary>
    [JsonPropertyName("fileUri")]
    public required string FileUri { get; set; }
}
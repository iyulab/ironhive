using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads;

/// <summary>
/// 비디오 컨텐츠의 메타데이터.
/// </summary>
internal sealed class VideoMetadata
{
    /// <summary> 시작 시간(초). ex) "3.2s"</summary>
    [JsonPropertyName("startOffset")]
    public string? StartOffset { get; set; }

    /// <summary>종료 시간(초). ex) "7.5s"</summary>
    [JsonPropertyName("endOffset")]
    public string? EndOffset { get; set; }

    /// <summary>프레임 속도(예: "30fps").</summary>
    [JsonPropertyName("fps")]
    public float? Fps { get; set; }
}
using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.GenerateContent.Models;

/// <summary>구글 검색 도구(필드 없음 – presence로 활성화).</summary>
internal sealed class GoogleRetrievalTool
{
    [JsonPropertyName("dynamicRetrievalConfig")]
    public Config? DynamicConfig { get; set; }

    internal class Config
    {
        /// <summary>검색 모드. 미지정 시 기본값 MODE_UNSPECIFIED.</summary>
        [JsonPropertyName("mode")]
        public Mode? Mode { get; set; }

        /// <summary>동적 검색 시 임계치(0~1). 기본 0.3. 높을수록 엄격.</summary>
        [JsonPropertyName("dynamicThreshold")]
        public float? Threshold { get; set; }
    }

    internal enum Mode
    {
        MODE_UNSPECIFIED,
        MODE_DYNAMIC,
    }
}

/// <summary>구글 검색 도구(필드 없음 – presence로 활성화).</summary>
internal sealed class GoogleSearchTool
{
    [JsonPropertyName("timeRangeFilter")]
    public Interval? TimeFilter { get; set; }

    /// <summary> RFC 3339 format: “{year}-{month}-{day}T{hour}:{min}:{sec}[.{frac_sec}]Z”</summary>
    internal class Interval
    {
        [JsonPropertyName("startTime")]
        public string? StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public string? EndTime { get; set; }
    }
}
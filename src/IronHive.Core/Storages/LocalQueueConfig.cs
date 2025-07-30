using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Core.Storages;

public class LocalQueueConfig
{
    /// <summary>
    /// 큐 저장소의 디렉토리 경로입니다.
    /// </summary>
    public required string DirectoryPath { get; set; }

    /// <summary>
    /// 큐 메시지가 살아 있는 시간(Time To Live, TTL)입니다.
    /// 지정하지 않는 경우 메시지는 만료되지 않습니다.
    /// </summary>
    public TimeSpan? TimeToLive { get; set; }

    /// <summary>
    /// 메시지 파일의 변환 옵션입니다.
    /// </summary>
    public JsonSerializerOptions JsonOptions { get; set; } = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}

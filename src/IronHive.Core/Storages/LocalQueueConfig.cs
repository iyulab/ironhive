using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Core.Storages;

/// <summary>
/// 로컬 큐 스토리지의 설정을 정의하는 클래스입니다.
/// </summary>
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
    /// 큐 메시지를 캐싱하기 위한 큐의 크기입니다. (기본값: 100)
    /// </summary>
    public int CacheSize { get; set; } = 100;

    /// <summary>
    /// 이벤트를 발생시킬지 여부를 나타냅니다. (기본값: false)
    /// </summary>
    public bool EnableRaisingEvents { get; set; } = false;

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

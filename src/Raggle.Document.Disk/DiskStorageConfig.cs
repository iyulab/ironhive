namespace Raggle.Document.Disk;

public class DiskStorageConfig
{
    /// <summary>
    /// 스토리지의 디렉토리 경로입니다.
    /// </summary>
    public required string DirectoryPath { get; set; }

    /// <summary>
    /// Azure Blob 작업 실패 시 최대 재시도 횟수입니다.
    /// </summary>
    public int BlobMaxRetryAttempts { get; set; } = 10;

    /// <summary>
    /// Azure Blob 작업 재시도 간 기본 지연 시간(밀리초)입니다.
    /// 지수적으로 증가합니다.
    /// </summary>
    public int BlobDelayMilliseconds { get; set; } = 200;
}

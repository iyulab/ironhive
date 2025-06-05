namespace IronHive.Abstractions.Message.Content;

/// <summary>
/// FileStorageManager 에서 관리되는 파일 메시지 콘텐츠입니다.
/// </summary>
public class FileMessageContent : MessageContent
{
    /// <summary>
    /// 파일이 저장된 스토리지의 이름을 나타냅니다
    /// </summary>
    public required string Storage { get; set; }

    /// <summary>
    /// 파일이 저장된 경로를 나타냅니다
    /// </summary>
    public required string FilePath { get; set; }
}

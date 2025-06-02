using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Message.Content;

public class ToolMessageContent : MessageContent
{
    /// <summary>
    /// 도구의 완료 상태를 나타냅니다.
    /// </summary>
    public required bool IsCompleted { get; set; }

    /// <summary>
    /// 도구 승인 상태입니다.
    /// </summary>
    public required bool IsApproved { get; set; }

    /// <summary>
    /// 콘텐츠 블록의 고유 ID입니다. 서비스 제공자의 ID를 사용합니다.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 도구 이름입니다.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 도구 실행에 필요한 매개변수입니다.
    /// </summary>
    public string? Input { get; set; }

    /// <summary>
    /// 도구 실행 결과입니다.
    /// </summary>
    public ToolOutput? Output { get; set; }
}

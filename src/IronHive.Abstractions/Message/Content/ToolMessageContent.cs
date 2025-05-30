using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Message.Content;

public class ToolMessageContent : MessageContent
{
    /// <summary>
    /// 도구의 완료 상태를 나타냅니다.
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// 도구 승인 상태입니다.
    /// </summary>
    public ToolApprovalStatus? ApprovalStatus { get; set; }

    /// <summary>
    /// 콘텐츠 블록의 고유 ID입니다. 서비스 제공자의 ID를 사용합니다.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 도구 이름입니다.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 도구 실행에 필요한 매개변수입니다.
    /// </summary>
    public string? Input { get; set; }

    /// <summary>
    /// 도구 실행 결과입니다.
    /// </summary>
    public ToolOutput? Output { get; set; }
}

public enum ToolApprovalStatus
{
    /// <summary>
    /// 도구 승인이 필요하지 않은 경우
    /// </summary>
    NotRequired,

    /// <summary>
    /// 도구 승인이 필요한 경우, 사용자에게 승인을 요청하는 상태
    /// </summary>
    Requires,

    /// <summary>
    /// 도구 실행을 승인한 경우
    /// </summary>
    Approved,

    /// <summary>
    /// 도구 실행을 거부한 경우
    /// </summary>
    Rejected
}

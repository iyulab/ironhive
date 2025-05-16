using IronHive.Abstractions.Tools;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 콘텐츠 블록의 기본 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AssistantTextContent), "text")]
[JsonDerivedType(typeof(AssistantToolContent), "tool")]
[JsonDerivedType(typeof(AssistantThinkingContent), "thinking")]
public interface IAssistantContent
{
    /// <summary>
    /// 콘텐츠 블록의 고유 ID입니다. 서비스 제공자의 ID를 사용합니다.
    /// </summary>
    string? Id { get; set; }

    /// <summary>
    /// 콘텐츠 블록의 인덱스 번호입니다.
    /// </summary>
    int? Index { get; set; }
}

/// <summary>
/// 콘텐츠 블록의 기본 클래스입니다.
/// </summary>
public abstract class AssistantContentBase : IAssistantContent
{
    /// <inheritdoc />
    public string? Id { get; set; }

    /// <inheritdoc />
    public int? Index { get; set; }
}

/// <summary>
/// 텍스트 콘텐츠 블록을 나타냅니다
/// </summary>
public class AssistantTextContent : AssistantContentBase
{
    /// <summary>
    /// 텍스트 콘텐츠 블록의 내용입니다.
    /// </summary>
    public string? Value { get; set; }
}

public enum ToolExecutionStatus
{
    /// <summary>
    /// 도구가 대기 중인 상태
    /// </summary>
    Pending,

    /// <summary>
    /// 도구가 실행 중인 상태
    /// </summary>
    Running,

    /// <summary>
    /// 도구 실행이 완료된 상태
    /// </summary>
    Completed,
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

public class AssistantToolContent : AssistantContentBase
{
    /// <summary>
    /// 도구 실행 상태입니다.
    /// </summary>
    public ToolExecutionStatus ExecutionStatus { get; set; } = ToolExecutionStatus.Pending;

    /// <summary>
    /// 도구 승인 상태입니다.
    /// </summary>
    public ToolApprovalStatus ApprovalStatus { get; set; } = ToolApprovalStatus.NotRequired;

    /// <summary>
    /// 도구 이름입니다.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 도구 실행에 필요한 매개변수입니다.
    /// </summary>
    public string? Arguments { get; set; }

    /// <summary>
    /// 도구 실행 결과입니다.
    /// </summary>
    public ToolResult? Result { get; set; }
}

public enum ThinkingMode
{
    /// <summary>
    /// 전체 추론 맥락 
    /// </summary>
    Detailed,

    /// <summary>
    /// 보안 추론 컨텐츠
    /// </summary>
    Secure,

    /// <summary>
    /// 추론 요약 콘텐츠
    /// </summary>
    Summary
}

/// <summary>
/// 추론 결과 콘텐츠 블록을 나타냅니다
/// </summary>
public class AssistantThinkingContent : AssistantContentBase
{
    /// <summary>
    /// 추론의 형식 입니다.
    /// </summary>
    public ThinkingMode Mode { get; set; } = ThinkingMode.Detailed;

    /// <summary>
    /// 추론 콘텐츠 블록의 내용입니다.
    /// </summary>
    public string? Value { get; set; }
}

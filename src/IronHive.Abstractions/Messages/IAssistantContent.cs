using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 콘텐츠 블록의 기본 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AssistantTextContent), "text")]
[JsonDerivedType(typeof(AssistantToolContent), "tool")]
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

/// <summary>
/// 도구 콘텐츠 블록을 나타냅니다
/// </summary>
public class AssistantToolContent : AssistantContentBase
{
    /// <summary>
    /// 현재 도구가 실행을 완료했는지 여부를 나타냅니다.
    /// </summary>
    public bool IsFinished { get; set; } = false;

    /// <summary>
    /// 도구 콘텐츠 블록의 상태입니다.
    /// </summary>
    public ToolStatus Status { get; set; } = ToolStatus.Pending;

    /// <summary>
    /// 도구 콘텐츠 블록의 이름입니다.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 도구 콘텐츠 블록의 인수입니다.
    /// </summary>
    public string? Arguments { get; set; }

    /// <summary>
    /// 도구 콘텐츠 블록의 결과입니다.
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// 도구 실행을 사용자가 거부했는지 여부를 나타냅니다.
    /// </summary>
    public bool UserDenied { get; set; } = true;

    public void Completed(string data)
    {
        Status = ToolStatus.Completed;
        Result = data;
    }

    public void Failed(string error)
    {
        Status = ToolStatus.Failed;
        Result = error;
    }

    public void DeniedToolInvoke()
        => Failed("User denied the tool call request. Please check to the user for reasons.");

    public void NotFoundTool()
        => Failed($"Tool [{Name}] not found. Please check the tool name for any typos or consult the documentation for available tools.");

    public void TooMuchResult()
        => Failed("The result contains too much information. Please change the parameters or specify additional filters to obtain a smaller result set.");
}

/// <summary>
/// Tool Status
/// </summary>
public enum ToolStatus
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
    /// 도구가 완료된 상태
    /// </summary>
    Completed,

    /// <summary>
    /// 도구가 실패한 상태
    /// </summary>
    Failed
}

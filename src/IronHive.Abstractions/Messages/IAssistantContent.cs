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
    string? Id { get; set; }

    int? Index { get; set; }
}

/// <summary>
/// 콘텐츠 블록의 기본 클래스입니다.
/// </summary>
public abstract class AssistantContentBase : IAssistantContent
{
    public string? Id { get; set; }

    public int? Index { get; set; }
}

/// <summary>
/// 텍스트 콘텐츠 블록을 나타냅니다
/// </summary>
public class AssistantTextContent : AssistantContentBase
{
    public string? Value { get; set; }
}

/// <summary>
/// 도구 콘텐츠 블록을 나타냅니다
/// </summary>
public class AssistantToolContent : AssistantContentBase
{
    public ToolStatus Status { get; set; } = ToolStatus.Pending;

    public string? Name { get; set; }

    public string? Arguments { get; set; }

    public string? Result { get; set; }

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

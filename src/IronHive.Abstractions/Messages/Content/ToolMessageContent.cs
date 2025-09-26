using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Messages.Content;

/// <summary>
/// 도구(툴) 실행을 위한 메시지 콘텐츠 블록입니다.
/// </summary>
public class ToolMessageContent : MessageContent
{
    /// <summary>
    /// 도구 실행이 완료되었는지를 나타냅니다.
    /// </summary>
    public bool IsCompleted => Output is not null;
    
    /// <summary>
    /// 현재 도구가 실행이 승인되었는지를 나타냅니다.
    /// </summary>
    public required bool IsApproved { get; set; }

    /// <summary>
    /// 현재 툴 블록의 고유 식별자입니다. 
    /// TollCall과 ToolResult 메시지에서 동시에 참조되는데 사용됩니다.
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
    /// 현재 도구의 결과 데이터입니다.
    /// </summary>
    public ToolOutput? Output { get; set; }
}

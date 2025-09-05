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
    /// 블록의 고유 ID입니다. 서비스 제공자가 ID를 제공할 경우 해당 ID를 사용합니다.
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

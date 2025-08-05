using IronHive.Abstractions.Message.Content;

namespace IronHive.Abstractions.Message.Roles;

/// <summary>
/// 어시스턴트 메시지를 나타내는 클래스입니다.
/// </summary>
public class AssistantMessage : Message
{
    /// <summary>
    /// 어시스턴트의 이름입니다.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 메시지 생성에 사용된 모델의 이름입니다.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// "text", "thinking" or "tool" 형태의 콘텐츠 블록들을 포함하는 컬렉션입니다.
    /// </summary>
    public ICollection<MessageContent> Content { get; set; } = [];

    /// <summary>
    /// 메시지가 생성된 시간입니다.
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// 승인이 필요한 Tool이 하나라도 있을 경우 true, 이외 false 입니다.
    /// </summary>
    public bool RequiresApproval
    {
        get
        {
            // 완료되지 않은 툴중에 승인되지 않은 경우가 하나라도 있을 경우
            return Content.OfType<ToolMessageContent>()
                .Any(tool => tool.IsCompleted == false && tool.Status != ToolContentStatus.Approved);
        }
    }

    /// <summary>
    /// Tool 사용을 기준으로 아이템을 분리합니다.
    /// Tool 컨텐츠들 다음 항목부터 새 그룹으로 분리됩니다.
    /// 예) [ThinkingContent, ToolContent, ToolContent, TextContent, ToolContent, TextContent]
    ///  => [ThinkingContent, ToolContent, ToolContent], [TextContent, ToolContent], [TextContent]
    /// </summary>
    public IEnumerable<IEnumerable<MessageContent>> SplitContentByTool()
    {
        if (Content.Count == 0)
            return Enumerable.Empty<IEnumerable<MessageContent>>();

        var result = new List<IEnumerable<MessageContent>>();
        var currentGroup = new List<MessageContent>();
        var previousWasTool = false;

        foreach (var item in Content)
        {
            var isTool = item is ToolMessageContent;

            // 이전 항목이 Tool이었고 현재 항목이 Tool이 아닌 경우 새 그룹 시작
            if (previousWasTool && !isTool)
            {
                result.Add(currentGroup);
                currentGroup = new List<MessageContent>();
            }

            currentGroup.Add(item);
            previousWasTool = isTool;
        }

        // 마지막 그룹 추가
        if (currentGroup.Count > 0)
        {
            result.Add(currentGroup);
        }

        return result;
    }
}

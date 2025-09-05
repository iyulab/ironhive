using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;

namespace IronHive.Abstractions.Messages.Roles;

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
    /// "text", "thinking" or "tool" 형태의 콘텐츠 블록들을 포함하는 컬렉션입니다.
    /// </summary>
    public ICollection<MessageContent> Content { get; set; } = [];

    /// <summary>
    /// 메시지 생성에 사용된 모델의 이름입니다.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// LLM의 툴 사용 템플릿에 맞추기 위해 Tool 컨텐츠를 기준으로 컨텐츠들을 그룹화합니다.
    /// Tool 컨텐츠들 다음 항목부터 새 그룹으로 분리됩니다.
    /// 예) [ThinkingContent, ToolContent, ToolContent, TextContent, ToolContent, TextContent]
    ///  => [ThinkingContent, ToolContent, ToolContent], [TextContent, ToolContent], [TextContent]
    /// </summary>
    public IEnumerable<IEnumerable<MessageContent>> GroupContentByToolBoundary()
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

using System.Text.Json.Serialization;
using IronHive.Abstractions.Messages.Content;

namespace IronHive.Abstractions.Messages;

public enum MessageRole
{
    User,
    Assistant
}

public class Message
{
    public MessageRole Role { get; set; }

    public ICollection<MessageContent> Content { get; set; } = [];

    public static Message User(string text)
        => new() { Role = MessageRole.User, Content = [new TextMessageContent { Value = text }] };

    public static Message User(params MessageContent[] content)
        => new() { Role = MessageRole.User, Content = [.. content] };

    public static Message Assistant(string text)
        => new() { Role = MessageRole.Assistant, Content = [new TextMessageContent { Value = text }] };

    public static Message Assistant(params MessageContent[] content)
        => new() { Role = MessageRole.Assistant, Content = [.. content] };

    /// <summary>
    /// LLM의 툴 사용 템플릿에 맞추기 위해 Tool 컨텐츠를 기준으로 컨텐츠들을 그룹화합니다.
    /// Tool 컨텐츠들 다음 항목부터 새 그룹으로 분리됩니다.
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

            if (previousWasTool && !isTool)
            {
                result.Add(currentGroup);
                currentGroup = new List<MessageContent>();
            }

            currentGroup.Add(item);
            previousWasTool = isTool;
        }

        if (currentGroup.Count > 0)
            result.Add(currentGroup);

        return result;
    }
}

namespace IronHive.Abstractions.ChatCompletion.Messages;

public class MessageContext
{
    /// <summary>
    /// 메시지 본체
    /// </summary>
    public MessageCollection Messages { get; set; } = new();

    /// <summary>
    /// 메시지 제목
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 전체 메시지 요약
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 실행 횟수 제한
    /// </summary>
    public int MaxLoopCount { get; set; } = 3;

    /// <summary>
    /// 메시지의 전체 토큰 수
    /// </summary>
    public int TotalTokens { get; set; } = 0;

    /// <summary>
    /// 토큰 수 제한으로 인해 잘린 메시지의 마지막 인덱스
    /// </summary>
    public int? TruncatedMessageIndex { get; set; }

    public MessageContext()
    { }

    public MessageContext(IEnumerable<Message> messages)
    {
        Messages = new(messages);
    }
}

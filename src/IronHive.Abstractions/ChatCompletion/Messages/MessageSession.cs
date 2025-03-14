namespace IronHive.Abstractions.ChatCompletion.Messages;

public class MessageSession
{
    /// <summary>
    /// 대화 메시지 입니다.
    /// </summary>
    public MessageCollection Messages { get; set; } = new();

    /// <summary>
    /// 대화의 제목입니다.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 전체 메시지에 대한 요약 정보입니다.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 최대 반복 실행(루프) 횟수입니다.
    /// </summary>
    public int MaxIterationCount { get; set; } = 5;

    /// <summary>
    /// 대화 내 모든 메시지의 총 토큰 수입니다.
    /// </summary>
    public int TotalTokens { get; set; } = 0;

    /// <summary>
    /// 토큰 수 제한으로 인해 잘린 마지막 메시지의 인덱스입니다.
    /// </summary>
    public int? LastTruncatedMessageIndex { get; set; }

    public MessageSession()
    { }

    public MessageSession(IEnumerable<Message> messages)
    {
        Messages = new(messages);
    }
}

namespace IronHive.Abstractions.ChatCompletion.Messages;

public class MessageSession
{
    /// <summary>
    /// 대화 메시지 입니다.
    /// </summary>
    public MessageCollection Messages { get; set; } = new();

    /// <summary>
    /// 현재 세션의 제목 입니다.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 토큰수 제한으로 대화 메시지를 요약한 정보 입니다.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 실패한 도구 사용에 대한 최대 시도 횟수입니다.
    /// </summary>
    public int MaxToolAttempts { get; set; } = 3;

    /// <summary>
    /// 메시지 전체의 총 토큰 수입니다.
    /// </summary>
    public int? TotalTokens { get; set; }

    /// <summary>
    /// 요약이된 메시지의 마지막 인덱스 입니다.
    /// </summary>
    public int? LastTruncatedIndex { get; set; }

    /// <summary>
    /// 현재 객체의 값을 업데이트 합니다.
    /// </summary>
    public bool AutoUpdate { get; set; } = true;

    public MessageSession()
    { }

    public MessageSession(IEnumerable<Message> messages)
    {
        Messages = new(messages);
    }
}

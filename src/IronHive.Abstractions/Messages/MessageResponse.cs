namespace IronHive.Abstractions.Messages;

public class MessageResponse
{
    /// <summary>
    /// 프로바이더 발급 ID ({provider}_ prefix 포함). tool_call 루프의 마지막 값.
    /// 다음 요청의 PreviousId로 사용하면 비용 절감 가능. Google은 미지원으로 null.
    /// </summary>
    public string? ResponseId { get; set; }

    public MessageDoneReason? DoneReason { get; set; }

    public Message? Message { get; set; }

    public MessageTokenUsage? TokenUsage { get; set; }

    public string? Model { get; set; }

    public TimeSpan? Duration { get; set; }
    
    public DateTime? Timestamp { get; set; }
}

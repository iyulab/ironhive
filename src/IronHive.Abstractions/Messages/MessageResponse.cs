using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 메시지 응답의 결과를 나타내는 클래스입니다.
/// </summary>
public class MessageResponse
{
    /// <summary>
    /// 응답의 고유 식별자입니다.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 응답이 종료된 사유입니다.
    /// </summary>
    public MessageDoneReason? DoneReason { get; set; }

    /// <summary>
    /// 응답 메시지 데이터입니다.
    /// </summary>
    public required AssistantMessage Message { get; set; }

    /// <summary>
    /// 요청 시 사용된 토큰 정보입니다.
    /// </summary>
    public MessageTokenUsage? TokenUsage { get; set; }

    /// <summary>
    /// 응답이 생성된 시간입니다.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
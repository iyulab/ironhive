using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(StreamingMessageBeginResponse), "message.begin")]
[JsonDerivedType(typeof(StreamingContentAddedResponse), "message.content.added")]
[JsonDerivedType(typeof(StreamingContentDeltaResponse), "message.content.delta")]
[JsonDerivedType(typeof(StreamingContentUpdatedResponse), "message.content.updated")]
[JsonDerivedType(typeof(StreamingContentInProgressResponse), "message.content.in_progress")]
[JsonDerivedType(typeof(StreamingContentCompletedResponse), "message.content.completed")]
[JsonDerivedType(typeof(StreamingMessageDoneResponse), "message.done")]
[JsonDerivedType(typeof(StreamingMessageErrorResponse), "message.error")]
public abstract class StreamingMessageResponse
{ }

/// <summary>
/// 스트리밍 메시지 시작 - 제너레이터 정상 진입 시 emit
/// </summary>
public class StreamingMessageBeginResponse : StreamingMessageResponse
{ }

/// <summary>
/// 스트리밍 에러 응답
/// </summary>
public class StreamingMessageErrorResponse : StreamingMessageResponse
{
    public string? Code { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// 메시지 컨텐츠 추가
/// </summary>
public class StreamingContentAddedResponse : StreamingMessageResponse
{
    public required int Index { get; set; }

    public required MessageContent Content { get; set; }
}

/// <summary>
/// 메시지 컨텐츠 부분 추가
/// </summary>
public class StreamingContentDeltaResponse : StreamingMessageResponse
{
    public required int Index { get; set; }

    public required MessageDeltaContent Delta { get; set; }
}

/// <summary>
/// 메시지 컨텐츠 전체 업데이트
/// </summary>
public class StreamingContentUpdatedResponse : StreamingMessageResponse
{
    public required int Index { get; set; }

    public required MessageUpdatedContent Updated { get; set; }
}

/// <summary>
/// 메시지 컨텐츠 진행 중
/// </summary>
public class StreamingContentInProgressResponse : StreamingMessageResponse
{
    public required int Index { get; set; }
}

/// <summary>
/// 메시지 컨텐츠 종료 - 축적된 전체 content 포함
/// </summary>
public class StreamingContentCompletedResponse : StreamingMessageResponse
{
    public required int Index { get; set; }

    public MessageContent? Content { get; set; }
}

/// <summary>
/// 스트리밍 메시지 종료 - 최종 축적 message 및 provider ResponseId 포함
/// </summary>
public class StreamingMessageDoneResponse : StreamingMessageResponse
{
    /// <summary>
    /// 프로바이더 발급 ID ({provider}_ prefix 포함). tool_call 루프의 마지막 값.
    /// 다음 요청의 PreviousId로 사용하면 비용 절감 가능. Google은 미지원으로 null.
    /// </summary>
    public string? ResponseId { get; set; }

    public MessageDoneReason? DoneReason { get; set; }

    public Message? Message { get; set; }

    /// <summary>
    /// 모델이 생성한 제안 목록입니다. MessageRequest.Suggestions가 설정된 경우에만 포함됩니다.
    /// </summary>
    public List<Suggestion>? Suggestions { get; set; }

    public MessageTokenUsage? TokenUsage { get; set; }

    public string? Model { get; set; }

    public TimeSpan? Duration { get; set; }

    public DateTime? Timestamp { get; set; }
}

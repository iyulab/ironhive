using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Message;

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
/// 스트리밍 메시지 시작
/// </summary>
public class StreamingMessageBeginResponse : StreamingMessageResponse
{
    public required string Id { get; set; }
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
/// 메시지 컨텐츠 부분 업데이트
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
/// 메시지 컨텐츠 종료
/// </summary>
public class StreamingContentCompletedResponse : StreamingMessageResponse
{
    public required int Index { get; set; }
}

/// <summary>
/// 스트리밍 메시지 종료
/// </summary>
public class StreamingMessageDoneResponse : StreamingMessageResponse
{
    public MessageDoneReason? DoneReason { get; set; }
    public MessageTokenUsage? TokenUsage { get; set; }
    public required string Id { get; set; }
    public required string Model { get; set; }
    public required DateTime Timestamp { get; set; }
}

/// <summary>
/// 스트리밍 에러 응답
/// </summary>
public class StreamingMessageErrorResponse : StreamingMessageResponse
{
    public required int Code { get; set; }
    public string? Message { get; set; }
}

using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Message;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(StreamingMessageErrorResponse), "message.error")]
[JsonDerivedType(typeof(StreamingMessageBeginResponse), "message.begin")]
[JsonDerivedType(typeof(StreamingMessageDoneResponse), "message.done")]

[JsonDerivedType(typeof(StreamingContentAddedResponse), "message.content.added")]
[JsonDerivedType(typeof(StreamingContentDeltaResponse), "message.content.delta")]
[JsonDerivedType(typeof(StreamingContentUpdatedResponse), "message.content.updated")]
[JsonDerivedType(typeof(StreamingContentInProgressResponse), "message.content.in_progress")]
[JsonDerivedType(typeof(StreamingContentCompletedResponse), "message.content.completed")]
public abstract class StreamingMessageResponse
{ }

// 스트리밍 메시지 시작
public class StreamingMessageBeginResponse : StreamingMessageResponse
{
    public required string Id { get; set; }
    public string? Name { get; set; }
}

// 스트리밍 메시지 종료
public class StreamingMessageDoneResponse : StreamingMessageResponse
{
    public required string Id { get; set; }
    public string? Name { get; set; }
    public MessageDoneReason? DoneReason { get; set; }
    public MessageTokenUsage? TokenUsage { get; set; }
    public required DateTime Timestamp { get; set; }
}

// 메시지 컨텐츠 추가
public class StreamingContentAddedResponse : StreamingMessageResponse
{
    public required int Index { get; set; }
    
    public required MessageContent Content { get; set; }
}

// 메시지 컨텐츠 부분 추가
public class StreamingContentDeltaResponse : StreamingMessageResponse
{
    public required int Index { get; set; }
    
    public required MessageDeltaContent Delta { get; set; }
}

// 메시지 컨텐츠 부분 업데이트
public class StreamingContentUpdatedResponse : StreamingMessageResponse
{
    public required int Index { get; set; }

    public required MessageUpdatedContent Updated { get; set; }
}

// 메시지 컨텐츠 진행 중
public class StreamingContentInProgressResponse : StreamingMessageResponse
{
    public required int Index { get; set; }

    public string? Message { get; set; }
}

// 메시지 컨텐츠 종료
public class StreamingContentCompletedResponse : StreamingMessageResponse
{
    public required int Index { get; set; }
}

// 스트리밍 에러 응답
public class StreamingMessageErrorResponse : StreamingMessageResponse
{
    public required int Code { get; set; }
    public string? Message { get; set; }
}

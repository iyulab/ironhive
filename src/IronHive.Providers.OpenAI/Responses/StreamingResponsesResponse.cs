using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses;

/// <summary>
/// 스트리밍으로 전달되는 모든 이벤트의 베이스 클래스
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(StreamingCreatedResponse), "response.created")]
[JsonDerivedType(typeof(StreamingInProgressResponse), "response.in_progress")]
[JsonDerivedType(typeof(StreamingCompletedResponse), "response.completed")]
[JsonDerivedType(typeof(StreamingFailedResponse), "response.failed")]
[JsonDerivedType(typeof(StreamingIncompletedResponse), "response.incompleted")]
[JsonDerivedType(typeof(StreamingOutputAddedResponse), "response.output_item.added")]
[JsonDerivedType(typeof(StreamingOutputDoneResponse), "response.output_item.done")]
[JsonDerivedType(typeof(StreamingContentAddedResponse), "response.content_part.added")]
[JsonDerivedType(typeof(StreamingContentDoneResponse), "response.content_part.done")]
[JsonDerivedType(typeof(StreamingTextDeltaResponse), "response.output_text.delta")]
[JsonDerivedType(typeof(StreamingTextDoneResponse), "response.output_text.done")]
[JsonDerivedType(typeof(StreamingRefusalDeltaResponse), "response.refusal.delta")]
[JsonDerivedType(typeof(StreamingRefusalDoneResponse), "response.refusal.done")]
[JsonDerivedType(typeof(StreamingFunctionToolDeltaResponse), "response.function_call_arguments.delta")]
[JsonDerivedType(typeof(StreamingFunctionToolDoneResponse), "response.function_call_arguments.done")]
[JsonDerivedType(typeof(StreamingReasoningAddedResponse), "response.reasoning_summary_part.added")]
[JsonDerivedType(typeof(StreamingReasoningDoneResponse), "response.reasoning_summary_part.done")]
[JsonDerivedType(typeof(StreamingReasoningSummaryDeltaResponse), "response.reasoning_summary_text.delta")]
[JsonDerivedType(typeof(StreamingReasoningSummaryDoneResponse), "response.reasoning_summary_text.done")]
[JsonDerivedType(typeof(StreamingReasoningTextDeltaResponse), "response.reasoning_text.delta")]
[JsonDerivedType(typeof(StreamingReasoningTextDoneResponse), "response.reasoning_text.done")]
[JsonDerivedType(typeof(StreamingAnnotationAddedResponse), "response.output_text.annotation.added")]
[JsonDerivedType(typeof(StreamingQueuedResponse), "response.queued")]
[JsonDerivedType(typeof(StreamingCustomToolDeltaResponse), "response.custom_tool_call_input.delta")]
[JsonDerivedType(typeof(StreamingCustomToolDoneResponse), "response.custom_tool_call_input.done")]
[JsonDerivedType(typeof(StreamingErrorResponse), "error")]
internal abstract class StreamingResponsesResponse
{
    [JsonPropertyName("response")]
    public required ResponsesResponse Response { get; set; }

    [JsonPropertyName("sequence_number")]
    public int SequenceNumber { get; set; }
}

#region Status Related

internal class StreamingCreatedResponse : StreamingResponsesResponse
{ }

internal class StreamingInProgressResponse : StreamingResponsesResponse
{ }

internal class StreamingCompletedResponse : StreamingResponsesResponse
{ }

internal class StreamingFailedResponse : StreamingResponsesResponse
{ }

internal class StreamingIncompletedResponse : StreamingResponsesResponse
{ }

#endregion

# region Output Item Related

internal abstract class StreamingOutputResponse : StreamingResponsesResponse
{
    [JsonPropertyName("output_index")]
    public int OutputIndex { get; set; }
}

internal class StreamingOutputAddedResponse : StreamingOutputResponse
{
    [JsonPropertyName("item")]
    public required ResponsesItem Item { get; set; }
}

internal class StreamingOutputDoneResponse : StreamingOutputResponse
{
    [JsonPropertyName("item")]
    public required ResponsesItem Item { get; set; }
}

# endregion

#region Content Related

internal abstract class StreamingContentResponse : StreamingOutputResponse
{
    [JsonPropertyName("content_index")]
    public required int ContentIndex { get; set; }

    [JsonPropertyName("item_id")]
    public required string ItemId { get; set; }
}

internal class StreamingContentAddedResponse : StreamingContentResponse
{
    [JsonPropertyName("part")]
    public required ResponsesItemPart Part { get; set; }
}

internal class StreamingContentDoneResponse : StreamingContentResponse
{
    [JsonPropertyName("part")]
    public required ResponsesItemPart Part { get; set; }
}

internal class StreamingTextDeltaResponse : StreamingContentResponse
{
    [JsonPropertyName("delta")]
    public required string Delta { get; set; }

    [JsonPropertyName("logprobs")]
    public ResponsesLogProbs? Logprobs { get; set; }
}

internal class StreamingTextDoneResponse : StreamingContentResponse
{
    [JsonPropertyName("logprobs")]
    public ResponsesLogProbs? Logprobs { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

internal class StreamingRefusalDeltaResponse : StreamingContentResponse
{
    [JsonPropertyName("delta")]
    public required string Delta { get; set; }
}

internal class StreamingRefusalDoneResponse : StreamingContentResponse
{
    [JsonPropertyName("refusal")]
    public required string Refusal { get; set; }
}

internal class StreamingFunctionToolDeltaResponse : StreamingContentResponse
{
    [JsonPropertyName("delta")]
    public required string Delta { get; set; }
}

internal class StreamingFunctionToolDoneResponse : StreamingContentResponse
{
    [JsonPropertyName("arguments")]
    public required string Arguments { get; set; }
}

#endregion

#region Reasoning Related

internal abstract class StreamingReasoningResponse : StreamingOutputResponse
{
    [JsonPropertyName("summary_index")]
    public required int SummaryIndex { get; set; }

    [JsonPropertyName("item_id")]
    public required string ItemId { get; set; }
}

internal class StreamingReasoningAddedResponse : StreamingReasoningResponse
{
    [JsonPropertyName("part")]
    public required ResponsesSummaryReasoningItemPart Part { get; set; }
}

internal class StreamingReasoningDoneResponse : StreamingReasoningResponse
{
    [JsonPropertyName("part")]
    public required ResponsesSummaryReasoningItemPart Part { get; set; }
}

internal class StreamingReasoningSummaryDeltaResponse : StreamingReasoningResponse
{
    [JsonPropertyName("delta")]
    public required string Delta { get; set; }
}

internal class StreamingReasoningSummaryDoneResponse : StreamingReasoningResponse
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

internal class StreamingReasoningTextDeltaResponse : StreamingReasoningResponse
{
    [JsonPropertyName("delta")]
    public required string Delta { get; set; }
}

internal class StreamingReasoningTextDoneResponse : StreamingReasoningResponse
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

#endregion ETC Related

#region ETC Related

internal class StreamingAnnotationAddedResponse : StreamingContentResponse
{
    [JsonPropertyName("annotation")]
    public required object Annotation { get; set; }

    [JsonPropertyName("annotation_index")]
    public required int AnnotationIndex { get; set; }
}

internal class StreamingQueuedResponse : StreamingResponsesResponse
{ }

internal class StreamingCustomToolDeltaResponse : StreamingContentResponse
{
    [JsonPropertyName("delta")]
    public required string Delta { get; set; }
}

internal class StreamingCustomToolDoneResponse : StreamingContentResponse
{
    [JsonPropertyName("input")]
    public required string Input { get; set; }
}

internal class StreamingErrorResponse : StreamingResponsesResponse
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("param")]
    public required string Param { get; set; }
}

#endregion
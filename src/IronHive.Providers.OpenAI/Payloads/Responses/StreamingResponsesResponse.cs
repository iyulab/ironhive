using IronHive.Abstractions.Json;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

/// <summary>
/// 스트리밍으로 전달되는 모든 이벤트의 베이스 클래스
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(StreamingCreatedResponse), "response.created")]
[JsonDerivedType(typeof(StreamingInProgressResponse), "response.in_progress")]
[JsonDerivedType(typeof(StreamingCompletedResponse), "response.completed")]
[JsonDerivedType(typeof(StreamingFailedResponse), "response.failed")]
[JsonDerivedType(typeof(StreamingIncompleteResponse), "response.incomplete")]
[JsonDerivedType(typeof(StreamingQueuedResponse), "response.queued")]
[JsonDerivedType(typeof(StreamingOutputAddedResponse), "response.output_item.added")]
[JsonDerivedType(typeof(StreamingOutputDoneResponse), "response.output_item.done")]
[JsonDerivedType(typeof(StreamingContentAddedResponse), "response.content_part.added")]
[JsonDerivedType(typeof(StreamingContentDoneResponse), "response.content_part.done")]
[JsonDerivedType(typeof(StreamingTextDeltaResponse), "response.output_text.delta")]
[JsonDerivedType(typeof(StreamingTextDoneResponse), "response.output_text.done")]
[JsonDerivedType(typeof(StreamingAnnotationAddedResponse), "response.output_text.annotation.added")]
[JsonDerivedType(typeof(StreamingRefusalDeltaResponse), "response.refusal.delta")]
[JsonDerivedType(typeof(StreamingRefusalDoneResponse), "response.refusal.done")]
[JsonDerivedType(typeof(StreamingFunctionToolDeltaResponse), "response.function_call_arguments.delta")]
[JsonDerivedType(typeof(StreamingFunctionToolDoneResponse), "response.function_call_arguments.done")]
[JsonDerivedType(typeof(StreamingWebsearchInProgressResponse), "response.web_search_call.in_progress")]
[JsonDerivedType(typeof(StreamingWebsearchSearchingResponse), "response.web_search_call.searching")]
[JsonDerivedType(typeof(StreamingWebsearchCompletedResponse), "response.web_search_call.completed")]
[JsonDerivedType(typeof(StreamingImageGenerationCompletedResponse), "response.image_generation_call.completed")]
[JsonDerivedType(typeof(StreamingImageGenerationGeneratingResponse), "response.image_generation_call.generating")]
[JsonDerivedType(typeof(StreamingImageGenerationInProgressResponse), "response.image_generation_call.in_progress")]
[JsonDerivedType(typeof(StreamingImageGenerationPartialResponse), "response.image_generation_call.partial_image")]
[JsonDerivedType(typeof(StreamingCodeInterpeterInProgressResponse), "response.code_interpreter_call.in_progress")]
[JsonDerivedType(typeof(StreamingCodeInterpeterInterpretingResponse), "response.code_interpreter_call.interpreting")]
[JsonDerivedType(typeof(StreamingCodeInterpeterCompletedResponse), "response.code_interpreter_call.completed")]
[JsonDerivedType(typeof(StreamingCodeInterpeterDeltaResponse), "response.code_interpreter_call_code.delta")]
[JsonDerivedType(typeof(StreamingCodeInterpeterDoneResponse), "response.code_interpreter_call_code.done")]
[JsonDerivedType(typeof(StreamingCustomToolDeltaResponse), "response.custom_tool_call_input.delta")]
[JsonDerivedType(typeof(StreamingCustomToolDoneResponse), "response.custom_tool_call_input.done")]
[JsonDerivedType(typeof(StreamingReasoningAddedResponse), "response.reasoning_summary_part.added")]
[JsonDerivedType(typeof(StreamingReasoningDoneResponse), "response.reasoning_summary_part.done")]
[JsonDerivedType(typeof(StreamingReasoningSummaryDeltaResponse), "response.reasoning_summary_text.delta")]
[JsonDerivedType(typeof(StreamingReasoningSummaryDoneResponse), "response.reasoning_summary_text.done")]
[JsonDerivedType(typeof(StreamingReasoningTextDeltaResponse), "response.reasoning_text.delta")]
[JsonDerivedType(typeof(StreamingReasoningTextDoneResponse), "response.reasoning_text.done")]
[JsonDerivedType(typeof(StreamingErrorResponse), "error")]
public abstract class StreamingResponsesResponse: JsonExtensibleBase
{
    [JsonPropertyName("sequence_number")]
    public int SequenceNumber { get; set; }
}

#region Status Related

///<summary> 스트리밍 상태 관련 응답의 Base </summary>
public abstract class StreamingStatusResponse : StreamingResponsesResponse
{
    [JsonPropertyName("response")]
    public required ResponsesResponse Response { get; set; }
}

public class StreamingCreatedResponse : StreamingStatusResponse
{ }

public class StreamingInProgressResponse : StreamingStatusResponse
{ }

public class StreamingCompletedResponse : StreamingStatusResponse
{ }

public class StreamingFailedResponse : StreamingStatusResponse
{ }

public class StreamingIncompleteResponse : StreamingStatusResponse
{ }

public class StreamingQueuedResponse : StreamingResponsesResponse
{ }

#endregion

# region Output Item Related

//// <summary> 스트리밍 Output Item 관련 응답의 Base </summary>
public abstract class StreamingOutputResponse : StreamingResponsesResponse
{
    [JsonPropertyName("output_index")]
    public int OutputIndex { get; set; }
}

public class StreamingOutputAddedResponse : StreamingOutputResponse
{
    [JsonPropertyName("item")]
    public required ResponsesItem Item { get; set; }
}

public class StreamingOutputDoneResponse : StreamingOutputResponse
{
    [JsonPropertyName("item")]
    public required ResponsesItem Item { get; set; }
}

# endregion

#region Output Item Content Related

/// <summary> 스트리밍 Output Item Content 관련 응답의 Base </summary>
public abstract class StreamingContentResponse : StreamingOutputResponse
{
    [JsonPropertyName("content_index")]
    public int? ContentIndex { get; set; }

    [JsonPropertyName("item_id")]
    public required string ItemId { get; set; }
}

public class StreamingContentAddedResponse : StreamingContentResponse
{
    [JsonPropertyName("part")]
    public required ResponsesItemPart Part { get; set; }
}

public class StreamingContentDoneResponse : StreamingContentResponse
{
    [JsonPropertyName("part")]
    public required ResponsesItemPart Part { get; set; }
}

public class StreamingTextDeltaResponse : StreamingContentResponse
{
    [JsonPropertyName("delta")]
    public required string Delta { get; set; }

    [JsonPropertyName("logprobs")]
    public IEnumerable<ResponsesLogProb>? Logprobs { get; set; }
}

public class StreamingTextDoneResponse : StreamingContentResponse
{
    [JsonPropertyName("logprobs")]
    public IEnumerable<ResponsesLogProb>? Logprobs { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class StreamingAnnotationAddedResponse : StreamingContentResponse
{
    [JsonPropertyName("annotation")]
    public required ResponsesAnnotation Annotation { get; set; }

    [JsonPropertyName("annotation_index")]
    public required int AnnotationIndex { get; set; }
}

public class StreamingRefusalDeltaResponse : StreamingContentResponse
{
    [JsonPropertyName("delta")]
    public required string Delta { get; set; }
}
public class StreamingRefusalDoneResponse : StreamingContentResponse
{
    [JsonPropertyName("refusal")]
    public required string Refusal { get; set; }
}

#endregion

#region Output Item Tool Related

/// <summary> 스트리밍 Tool 관련 응답의 Base </summary>
public abstract class StreamingToolResponse : StreamingOutputResponse
{
    [JsonPropertyName("item_id")]
    public required string ItemId { get; set; }
}

public class StreamingFunctionToolDeltaResponse : StreamingToolResponse
{
    [JsonPropertyName("delta")]
    public required string Delta { get; set; }
}

public class StreamingFunctionToolDoneResponse : StreamingToolResponse
{
    [JsonPropertyName("arguments")]
    public required string Arguments { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class StreamingWebsearchInProgressResponse : StreamingToolResponse
{ }

public class StreamingWebsearchSearchingResponse : StreamingToolResponse
{ }

public class StreamingWebsearchCompletedResponse : StreamingToolResponse
{ }

public class StreamingImageGenerationCompletedResponse : StreamingToolResponse
{ }

public class StreamingImageGenerationGeneratingResponse : StreamingToolResponse
{ }

public class StreamingImageGenerationInProgressResponse : StreamingToolResponse
{ }

public class StreamingImageGenerationPartialResponse : StreamingToolResponse
{
    [JsonPropertyName("partial_image_b64")]
    public required string PartialBase64 { get; set; }

    [JsonPropertyName("partial_image_index")]
    public int ImageIndex { get; set; }
}

public class StreamingCodeInterpeterInProgressResponse : StreamingToolResponse
{ }

public class StreamingCodeInterpeterInterpretingResponse : StreamingToolResponse
{ }

public class StreamingCodeInterpeterCompletedResponse : StreamingToolResponse
{ }

public class StreamingCodeInterpeterDeltaResponse : StreamingToolResponse
{
    [JsonPropertyName("delta")]
    public required string Delta { get; set; }
}

public class StreamingCodeInterpeterDoneResponse : StreamingToolResponse
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }
}

public class StreamingCustomToolDeltaResponse : StreamingToolResponse
{
    [JsonPropertyName("delta")]
    public required string Delta { get; set; }
}

public class StreamingCustomToolDoneResponse : StreamingToolResponse
{
    [JsonPropertyName("input")]
    public required string Input { get; set; }
}

#endregion

#region Output Item Reasoning Related

/// <summary> 스트리밍 Reasoning 관련 응답의 Base </summary>
public abstract class StreamingReasoningResponse : StreamingOutputResponse
{
    [JsonPropertyName("summary_index")]
    public required int SummaryIndex { get; set; }

    [JsonPropertyName("item_id")]
    public required string ItemId { get; set; }
}

public class StreamingReasoningAddedResponse : StreamingReasoningResponse
{
    [JsonPropertyName("part")]
    public required ResponsesSummaryReasoningItemPart Part { get; set; }
}

public class StreamingReasoningDoneResponse : StreamingReasoningResponse
{
    [JsonPropertyName("part")]
    public required ResponsesSummaryReasoningItemPart Part { get; set; }
}

public class StreamingReasoningSummaryDeltaResponse : StreamingReasoningResponse
{
    [JsonPropertyName("delta")]
    public required string Delta { get; set; }
}

public class StreamingReasoningSummaryDoneResponse : StreamingReasoningResponse
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class StreamingReasoningTextDeltaResponse : StreamingReasoningResponse
{
    [JsonPropertyName("delta")]
    public required string Delta { get; set; }
}

public class StreamingReasoningTextDoneResponse : StreamingReasoningResponse
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

#endregion

public class StreamingErrorResponse : StreamingResponsesResponse
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("param")]
    public required string Param { get; set; }
}

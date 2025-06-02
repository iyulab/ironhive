using System.Text.Json.Serialization;
using System.Text.Json;

namespace IronHive.Providers.OpenAI.Responses;

/// <summary>
/// 스트리밍으로 전달되는 모든 이벤트의 베이스 클래스
/// </summary>
internal class StreamingResponsesResponse
{
    /// <summary>
    /// 이벤트 타입 (예: "response.created", "response.output_text.delta", "response.completed", "response.error" 등)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// 타입별 데이터를 담고 있는 필드
    /// </summary>
    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }
}

/// <summary>
/// "response.output_text.delta" 이벤트의 데이터 구조
/// </summary>
public class ResponseTextDeltaEvent
{
    /// <summary>
    /// 항상 "response.output_text.delta"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// 모델이 생성한 텍스트 델타 조각
    /// </summary>
    [JsonPropertyName("delta")]
    public string Delta { get; set; }

    /// <summary>
    /// 이 델타가 속한 출력 아이템의 ID
    /// </summary>
    [JsonPropertyName("item_id")]
    public string ItemId { get; set; }

    /// <summary>
    /// 이 델타가 속한 출력 아이템 중 몇 번째인지 (인덱스)
    /// </summary>
    [JsonPropertyName("output_index")]
    public int OutputIndex { get; set; }

    /// <summary>
    /// 텍스트 델타가 콘텐츠 중 몇 번째 파트에 속하는지 (인덱스)
    /// </summary>
    [JsonPropertyName("content_index")]
    public int ContentIndex { get; set; }
}

/// <summary>
/// "response.completed" 이벤트의 데이터 구조
/// </summary>
public class ResponseCompletedEvent
{
    /// <summary>
    /// 항상 "response.completed"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// 완료된 응답 객체의 정보
    /// </summary>
    [JsonPropertyName("response")]
    public CompletedResponseInfo Response { get; set; }
}

/// <summary>
/// 응답 완료 시 함께 전달되는 응답 정보
/// </summary>
public class CompletedResponseInfo
{
    /// <summary>
    /// 응답 ID (예: "resp_67cbc9705fc08190bbe455c5ba3d6daf")
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// 생성된 시각(Unix timestamp, 초 단위)
    /// </summary>
    [JsonPropertyName("created_at")]
    public double CreatedAt { get; set; }

    /// <summary>
    /// 사용된 모델 식별자 (예: "gpt-4o-2024-08-06")
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; }

    /// <summary>
    /// 응답 상태 (예: "completed", "failed", "in_progress" 등)
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    /// <summary>
    /// (선택) 메시지 배열, 최종 결과를 포함한 전체 메시지 정보
    /// </summary>
    [JsonPropertyName("output")]
    public JsonElement? Output { get; set; }

    /// <summary>
    /// (선택) 도구 호출 관련 정보 등 추가 메타데이터
    /// </summary>
    [JsonPropertyName("tools")]
    public JsonElement? Tools { get; set; }

    /// <summary>
    /// (선택) 커스텀 메타데이터
    /// </summary>
    [JsonPropertyName("metadata")]
    public JsonElement? Metadata { get; set; }

    /// <summary>
    /// (선택) 기타 필요한 필드를 추가로 정의할 수 있음
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalProperties { get; set; }
}

/// <summary>
/// "response.error" 이벤트의 데이터 구조
/// </summary>
public class ResponseErrorEvent
{
    /// <summary>
    /// 항상 "response.error"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// 에러 상세 정보
    /// </summary>
    [JsonPropertyName("error")]
    public ErrorDetail Error { get; set; }
}

/// <summary>
/// 오류 발생 시 전달되는 에러 상세 객체
/// </summary>
public class ErrorDetail
{
    /// <summary>
    /// 에러 메시지
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    /// <summary>
    /// 에러 유형 (예: "invalid_request_error", "rate_limit_error" 등)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// 문제가 된 파라미터 이름 (옵션)
    /// </summary>
    [JsonPropertyName("param")]
    public string Param { get; set; }

    /// <summary>
    /// 에러 코드 (옵션)
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; }
}

/// <summary>
/// 함수 호출 인자 델타 이벤트 ("response.function_call_arguments.delta")
/// </summary>
public class ResponseFunctionCallArgumentsDeltaEvent
{
    /// <summary>
    /// 항상 "response.function_call_arguments.delta"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// 이 델타가 속한 호출 ID (message 또는 도구 호출 내에서의 식별자)
    /// </summary>
    [JsonPropertyName("call_id")]
    public string CallId { get; set; }

    /// <summary>
    /// JSON 문자열 형태로 점진적으로 전송되는 함수 호출 인자의 델타 조각
    /// </summary>
    [JsonPropertyName("delta")]
    public string Delta { get; set; }
}

/// <summary>
/// 함수 호출 인자 완료 이벤트 ("response.function_call_arguments.done")
/// </summary>
public class ResponseFunctionCallArgumentsDoneEvent
{
    /// <summary>
    /// 항상 "response.function_call_arguments.done"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// 최종 완성된 인자(input) JSON
    /// </summary>
    [JsonPropertyName("arguments")]
    public JsonElement Arguments { get; set; }

    /// <summary>
    /// 호출 ID (message 또는 도구 호출 내에서의 식별자)
    /// </summary>
    [JsonPropertyName("call_id")]
    public string CallId { get; set; }
}

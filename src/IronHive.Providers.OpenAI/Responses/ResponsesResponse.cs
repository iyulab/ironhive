using System.Text.Json.Serialization;
using System.Text.Json;

namespace IronHive.Providers.OpenAI.Responses;

/// <summary>
/// POST /v1/responses 응답 바디에 해당하는 클래스
/// </summary>
internal class ResponsesResponse
{
    /// <summary>
    /// 생성된 응답 리소스의 고유 ID
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// 항상 "response"
    /// </summary>
    [JsonPropertyName("object")]
    public required string Object { get; set; }

    /// <summary>
    /// Unix 타임스탬프(초 단위)로 생성 시각
    /// </summary>
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    /// <summary>
    /// 사용된 모델 식별자 (예: "gpt-4o-2024-08-06")
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// 오류가 발생했을 경우 오류 객체, 정상일 때는 null
    /// </summary>
    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }

    /// <summary>
    /// 출력을 중간에 멈춘 경우 그 사유를 담은 객체 또는 null
    /// </summary>
    [JsonPropertyName("incomplete_details")]
    public IncompleteDetails? IncompleteDetails { get; set; }

    /// <summary>
    /// 응답 생성 시 사용된 지시사항(문자열) 또는 null
    /// </summary>
    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    /// <summary>
    /// 커스텀 메타데이터용 빈 객체 (현재는 빈 딕셔너리)
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, JsonElement>? Metadata { get; set; }

    /// <summary>
    /// 모델이 생성한 메시지들의 배열
    /// </summary>
    [JsonPropertyName("output")]
    public List<OutputMessage>? Output { get; set; }
}

/// <summary>
/// 출력 메시지 하나를 나타내는 클래스
/// </summary>
public class OutputMessage
{
    /// <summary>
    /// 메시지 고유 ID (예: "msg_xxx")
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// "output_text", "function_call", "tool", "output_image" 등
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// 실제 출력 내용. 일반 텍스트인 경우 리스트 안에 { type="output_text", text="..." } 형태
    /// </summary>
    [JsonPropertyName("content")]
    public List<OutputContent>? Content { get; set; }

    /// <summary>
    /// 함수 호출인 경우 name 필드 (예: 호출된 함수 이름)
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 함수 호출 인자(JSON)인 경우 arguments 필드
    /// </summary>
    [JsonPropertyName("arguments")]
    public JsonElement? Arguments { get; set; }
}

/// <summary>
/// OutputMessage.Content 배열의 각 항목 클래스
/// </summary>
public class OutputContent
{
    /// <summary>
    /// 예: "output_text" 등
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// 실제 텍스트(출력) 내용
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 필요 시 추가적인 주석 등(일반적으로 빈 배열)
    /// </summary>
    [JsonPropertyName("annotations")]
    public List<JsonElement>? Annotations { get; set; }
}

/// <summary>
/// 응답 중 에러가 있을 때 반환되는 구조
/// (OpenAI Error 객체 포맷)
/// </summary>
public class ErrorObject
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("param")]
    public string? Param { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

/// <summary>
/// incomplete_details 구조 (출력 중단 시 사유)
/// </summary>
public class IncompleteDetails
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

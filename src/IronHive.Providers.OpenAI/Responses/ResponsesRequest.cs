using System.Text.Json.Serialization;
using System.Text.Json;

namespace IronHive.Providers.OpenAI.Responses;

/// <summary>
/// POST /v1/responses 요청 바디에 해당하는 클래스
/// </summary>
internal class ResponsesRequest
{
    /// <summary>
    /// 사용할 모델 식별자 (예: "gpt-4o-mini")
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// 입력 메시지 또는 단일 문자열. 
    /// 메시지 배열 형태인 경우 각 Message 객체를 사용
    /// </summary>
    [JsonPropertyName("input")]
    public JsonElement Input { get; set; }
    // 주의: 간단히 문자열만 보낼 수도 있고, 
    // [{ "role": "...", "content": "..." }, ...] 배열로 보낼 수도 있으므로
    // JsonElement로 두어 유연하게 처리합니다.

    /// <summary>
    /// 추가 지시사항(선택). 문자열 형태로만 전달
    /// </summary>
    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    /// <summary>
    /// 호출할 도구(tool) 정의 배열(선택)
    /// </summary>
    [JsonPropertyName("tools")]
    public List<ToolDefinition>? Tools { get; set; }

    /// <summary>
    /// 출력 형식 지정(예: JSON 스키마) (선택)
    /// </summary>
    [JsonPropertyName("text")]
    public TextFormat? Text { get; set; }

    /// <summary>
    /// 이전에 생성된 응답 ID를 지정 (대화 상태 유지) (선택)
    /// </summary>
    [JsonPropertyName("previous_response_id")]
    public string? PreviousResponseId { get; set; }

    /// <summary>
    /// 스트리밍 응답 여부 (선택)
    /// </summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    /// <summary>
    /// 샘플링 온도(선택)
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Top-p 샘플링 확률(선택)
    /// </summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    /// <summary>
    /// 생성할 응답 개수(선택)
    /// </summary>
    [JsonPropertyName("n")]
    public int? N { get; set; }

    /// <summary>
    /// 최대 토큰 수 제한(선택)
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// 중단 토큰(들) (선택)
    /// </summary>
    [JsonPropertyName("stop")]
    public List<string>? Stop { get; set; }

    /// <summary>
    /// 로그 확률 수집(선택)
    /// </summary>
    [JsonPropertyName("logprobs")]
    public int? Logprobs { get; set; }

    /// <summary>
    /// Presence Penalty(선택)
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public double? PresencePenalty { get; set; }

    /// <summary>
    /// Frequency Penalty(선택)
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; set; }

    /// <summary>
    /// Logit Bias(선택)  
    /// 키: 토큰 ID 문자열, 값: bias 값
    /// </summary>
    [JsonPropertyName("logit_bias")]
    public Dictionary<string, double>? LogitBias { get; set; }

    /// <summary>
    /// 사용자 식별자(선택)
    /// </summary>
    [JsonPropertyName("user")]
    public string? User { get; set; }
}

/// <summary>
/// 메시지 하나를 나타내는 클래스
/// </summary>
public class Message
{
    /// <summary>
    /// 역할: "system", "developer", "user" 중 하나
    /// </summary>
    [JsonPropertyName("role")]
    public required string Role { get; set; }

    /// <summary>
    /// 콘텐츠: 단순 문자열 또는 rich content 객체 배열
    /// </summary>
    [JsonPropertyName("content")]
    public JsonElement Content { get; set; }
}

/// <summary>
/// 도구(tool) 호출 시 사용하는 정의 클래스
/// </summary>
public class ToolDefinition
{
    /// <summary>
    /// 도구의 타입 (예: "web_search_preview", "file_search", "image_generation", "function" 등)
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// 추가적인 속성들은 JSON 확장 데이터를 통해 저장  
    /// 예) file_search의 vector_store_ids, max_num_results 등
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>
/// 출력 형식(text) 지정 클래스 (선택)
/// </summary>
public class TextFormat
{
    /// <summary>
    /// 예: "json_schema" 등
    /// </summary>
    [JsonPropertyName("format")]
    public required FormatDetail Format { get; set; }
}

public class FormatDetail
{
    /// <summary>
    /// "json_schema" 등
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// 스키마 이름
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

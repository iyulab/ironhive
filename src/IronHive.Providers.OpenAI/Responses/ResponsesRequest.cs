using IronHive.Providers.OpenAI.ChatCompletion;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses;

/// <summary>
/// POST /v1/responses 요청 바디에 해당하는 클래스
/// <see href="https://platform.openai.com/docs/api-reference/responses/create"/>"/>
/// </summary>
internal class ResponsesRequest
{
    /// <summary>
    /// 백그라운드로 처리할지 여부 (선택, 기본값: false).
    /// </summary>
    [JsonPropertyName("background")]
    public bool Background { get; set; } = false;

    /// <summary>
    /// 이전 대화 기록 ID (선택).
    /// </summary>
    [JsonPropertyName("conversation")]
    public string? Conversation { get; set; }

    /// <summary>
    /// 응답에 포함할 추가 정보 (선택).
    /// </summary>
    [JsonPropertyName("include")]
    public ICollection<string>? Include { get; set; }

    /// <summary>
    /// 모델의 입력 정보 (필수).
    /// </summary>
    [JsonPropertyName("input")]
    public ICollection<ResponsesItem>? Input { get; set; }

    /// <summary>
    /// 시스템 프롬프트 (선택).
    /// </summary>
    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    /// <summary>
    /// </summary>
    [JsonPropertyName("max_output_tokens")]
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// </summary>
    [JsonPropertyName("max_tool_calls")]
    public int? MaxToolCalls { get; set; }

    /// <summary>
    /// 최대 16개의 키-밸류 쌍을 포함할 수 있는 메타데이터 (선택).
    /// </summary>
    [JsonPropertyName("metadata")]
    public IDictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// OpenAI 모델 ID
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// </summary>
    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; set; }

    /// <summary>
    /// conversation과 혼합하여 사용할 수 없습니다.
    /// </summary>
    [JsonPropertyName("previous_response_id")]
    public string? PreviousResponseId { get; set; }

    /// <summary>
    /// </summary>
    [JsonPropertyName("prompt")]
    public ResponsesPrompt? Prompt { get; set; }

    /// <summary>
    /// </summary>
    [JsonPropertyName("prompt_cache_key")]
    public string? PromptCacheKey { get; set; }

    /// <summary>
    /// </summary>
    [JsonPropertyName("reasoning")]
    public ResponsesReasoning? Reasoning { get; set; }

    /// <summary>
    /// </summary>
    [JsonPropertyName("safety_identifier")]
    public string? SafetyIdentifier { get; set; }

    /// <summary>
    /// "auto", "default", "flex", "priority"
    /// </summary>
    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    /// <summary>
    /// (Not Use) store the output for model distillation
    /// </summary>
    [JsonPropertyName("store")]
    public bool? Store { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("stream_options")]
    public ResponsesStreamOptions? StreamOptions { get; set; }

    /// <summary>
    /// 0.0 to 2.0, default is 1.0
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    /// <summary>
    /// 출력 텍스트 형태
    /// </summary>
    [JsonPropertyName("text")]
    public object? Text { get; set; }

    /// <summary>
    /// "none", "auto", "required", ...
    /// </summary>
    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; }

    [JsonPropertyName("tools")]
    public IEnumerable<OpenAITool>? Tools { get; set; }

    /// <summary>
    /// 0 to 20
    /// </summary>
    [JsonPropertyName("top_logprobs")]
    public int? TopLogProbs { get; set; }

    /// <summary>
    /// 0.0 to 1.0, do not use with temperature
    /// </summary>
    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }

    /// <summary>
    /// "auto", "disabled"(default)
    /// </summary>
    [JsonPropertyName("truncation")]
    public string? Truncation { get; set; }
}

public class ResponsesStreamOptions
{
    [JsonPropertyName("include_obfuscation")]
    public bool? IncludeObfuscation { get; set; }
}
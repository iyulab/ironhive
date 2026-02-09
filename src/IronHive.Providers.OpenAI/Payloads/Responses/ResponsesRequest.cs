using IronHive.Abstractions.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

/// <summary>
/// POST /v1/responses 요청 바디에 해당하는 클래스
/// <see href="https://platform.openai.com/docs/api-reference/responses/create"/>"/>
/// </summary>
public class ResponsesRequest : JsonExtensionBase
{
    /// <summary>
    /// 백그라운드로 처리할지 여부 (기본값: false). true일시, store=true로 강제 됩니다.
    /// <para>
    /// codex 또는 deepresearch 모델등 응답에 시간이 오래 걸리는 모델에 유용합니다.
    /// <see href="https://platform.openai.com/docs/guides/background"/>
    /// </para>
    /// </summary>
    [JsonPropertyName("background")]
    public bool? Background { get; set; }

    /// <summary>
    /// 이전 대화 기록 ID (선택).
    /// </summary>
    [JsonPropertyName("conversation")]
    public ResponsesConversation? Conversation { get; set; }

    /// <summary>
    /// 응답에 포함할 추가 정보.
    /// "web_search_call.action.sources": 웹 검색 도구 호출의 소스 포함.
    /// "code_interpreter_call.outputs": 코드 인터프리터 도구 호출의 출력 포함.
    /// "computer_call_output.output.image_url": 컴퓨터 비전 도구 호출의 출력 이미지 URL 포함.
    /// "file_search_call.results": 파일 검색 도구 호출의 결과 포함.
    /// "message.input_image.image_url": 입력 이미지의 URL 포함.
    /// "message.output_text.logprobs": 출력 텍스트의 로그 확률 포함.
    /// "reasoning.encrypted_content": 추론 암호화된 콘텐츠 포함(store: false 일시 포함).
    /// </summary>
    [JsonPropertyName("include")]
    public ICollection<string>? Include { get; set; }

    /// <summary>
    /// 모델의 입력 정보
    /// </summary>
    [JsonPropertyName("input")]
    public required ICollection<ResponsesItem> Input { get; set; }

    /// <summary>
    /// 시스템 프롬프트.
    /// </summary>
    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    [JsonPropertyName("max_output_tokens")]
    public int? MaxOutputTokens { get; set; }

    [JsonPropertyName("max_tool_calls")]
    public int? MaxToolCalls { get; set; }

    /// <summary>
    /// 최대 16개의 키-밸류 쌍을 포함할 수 있는 메타데이터 (선택).
    /// </summary>
    [JsonPropertyName("metadata")]
    public KeyValuePair<string, string>? Metadata { get; set; }

    /// <summary>
    /// OpenAI 모델 ID
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; set; }

    /// <summary>
    /// conversation과 혼합하여 사용할 수 없습니다.
    /// </summary>
    [JsonPropertyName("previous_response_id")]
    public string? PreviousResponseId { get; set; }

    [JsonPropertyName("prompt")]
    public ResponsesPrompt? Prompt { get; set; }

    /// <summary>
    /// 캐시를 사용할 경우
    /// </summary>
    [JsonPropertyName("prompt_cache_key")]
    public string? PromptCacheKey { get; set; }

    /// <summary>
    /// 캐시를 사용할 경우 유지 기간 ("24h" 이내)
    /// </summary>
    [JsonPropertyName("prompt_cache_retention")]
    public string? PromptCacheRetention { get; set; }

    /// <summary>
    /// gpt-5 또는 o 시리즈에서만 작동
    /// </summary>
    [JsonPropertyName("reasoning")]
    public ResponsesReasoning? Reasoning { get; set; }

    [JsonPropertyName("safety_identifier")]
    public string? SafetyIdentifier { get; set; }

    /// <summary>
    /// "auto", "default", "flex", "priority"
    /// </summary>
    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    /// <summary>
    /// OpenAI에 해당 요청을 저장합니다. default: false
    /// Store가 true인 경우, previous_response_id를 사용합니다(매뉴얼 Input을 지양).
    /// Store가 false인 경우, Include에 "reasoning.encrypted_content"를 반드시 포함해야 합니다.
    /// </summary>
    [JsonPropertyName("store")]
    public bool Store { get; set; } = false;

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

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
    public ResponsesText? Text { get; set; }

    [JsonPropertyName("tool_choice")]
    public ResponsesToolChoice? ToolChoice { get; set; }

    [JsonPropertyName("tools")]
    public IEnumerable<ResponsesTool>? Tools { get; set; }

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
    /// <summary>
    /// 스트림 난독화 포함 여부, OpenAI와의 연결을 신뢰할 수 없는 환경에서 사용
    /// </summary>
    [JsonPropertyName("include_obfuscation")]
    public bool? IncludeObfuscation { get; set; }
}
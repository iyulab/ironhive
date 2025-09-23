﻿using IronHive.Providers.OpenAI.ChatCompletion;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses;

/// <summary>
/// POST /v1/responses 응답 바디에 해당하는 클래스
/// <see href="https://platform.openai.com/docs/api-reference/responses/object"/>
/// </summary>
internal class ResponsesResponse
{
    /// <summary>
    /// </summary>
    [JsonPropertyName("background")]
    public required bool Background { get; set; }

    /// <summary>
    /// </summary>
    [JsonPropertyName("conversation")]
    public required string Conversation { get; set; }

    /// <summary>
    /// Unix 타임스탬프(초 단위)
    /// </summary>
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    /// <summary>
    /// 오류가 발생했을 경우 오류 객체, 정상일 때는 null
    /// </summary>
    [JsonPropertyName("error")]
    public ResponsesError? Error { get; set; }

    /// <summary>
    /// 유니크 id
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// 출력을 중간에 멈춘 경우 그 사유를 담은 객체 또는 null
    /// </summary>
    [JsonPropertyName("incomplete_details")]
    public ResponsesIncompleteDetails? IncompleteDetails { get; set; }

    /// <summary>
    /// 응답 생성 시 사용된 지시사항(문자열), 또는 array?
    /// TODO: 배열 형식 응답 확인
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
    /// 최대 16개의 키-값 쌍 데이터
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// 사용된 모델 식별자
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// always "response"
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; } = "response";

    /// <summary>
    /// 모델이 생성한 메시지들의 배열
    /// </summary>
    [JsonPropertyName("output")]
    public ICollection<ResponsesItem>? Output { get; set; }

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
    /// </summary>
    [JsonPropertyName("status")]
    public ResponsesStatus? Status { get; set; }

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

    /// <summary>
    /// </summary>
    [JsonPropertyName("usage")]
    public ResponsesTokenUsage? Usage { get; set; }
}
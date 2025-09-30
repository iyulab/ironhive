using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.GenerateContent.Models;

/// <summary>생성 제어 파라미터.</summary>
internal sealed class GenerationConfig
{
    /// <summary>생성을 중지할 시퀀스(최대 5개).</summary>
    [JsonPropertyName("stopSequences")]
    public ICollection<string>? StopSequences { get; set; }

    /// <summary>응답 MIME 타입(구조화 출력 시 "application/json").</summary>
    [JsonPropertyName("responseMimeType")]
    public string? ResponseMimeType { get; set; }

    /// <summary>응답 JSON 스키마(구조화 출력 시). OpenAPI 3.0 서브셋/Schema 객체.</summary>
    [JsonPropertyName("responseSchema")]
    public object? ResponseSchema { get; set; }

    /// <summary>응답 JSON Value 스키마(구조화 출력 시).</summary>
    [JsonPropertyName("responseJsonSchema")]
    public object? ResponseJsonSchema { get; set; }

    [JsonPropertyName("responseModalities")]
    public ICollection<Modality>? ResponseModalities { get; set; }

    /// <summary>응답 생성 갯수</summary>
    [JsonPropertyName("candidateCount")]
    public int? CandidateCount { get; set; }

    /// <summary>최대 출력 토큰 수.</summary>
    [JsonPropertyName("maxOutputTokens")]
    public int? MaxOutputTokens { get; set; }

    /// <summary>샘플링 온도(0.0~2.0).</summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    /// <summary>Top-p (뉴클리어스 샘플링).</summary>
    [JsonPropertyName("topP")]
    public float? TopP { get; set; }

    /// <summary>Top-k (상위 k 토큰 제한). 일부 모델은 비활성.</summary>
    [JsonPropertyName("topK")]
    public int? TopK { get; set; }

    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    [JsonPropertyName("presencePenalty")]
    public float? PresencePenalty { get; set; }

    [JsonPropertyName("frequencyPenalty")]
    public float? FrequencyPenalty { get; set; }

    [JsonPropertyName("responseLogprobs")]
    public bool? ResponseLogprobs { get; set; }

    [JsonPropertyName("logprobs")]
    public int? Logprobs { get; set; }

    [JsonPropertyName("enableEnhancedCivicAnswers")]
    public bool? EnableEnhancedCivicAnswers { get; set; }

    /// <summary>후처리: 음성 합성/보이스 설정 등(필요 모델에서 사용).</summary>
    [JsonPropertyName("speechConfig")]
    public SpeechConfig? SpeechConfig { get; set; }

    /// <summary>Thinking 설정(사고 토큰 예산 등).</summary>
    [JsonPropertyName("thinkingConfig")]
    public ThinkingConfig? ThinkingConfig { get; set; }

    /// <summary>
    /// MEDIA_RESOLUTION_UNSPECIFIED, 
    /// MEDIA_RESOLUTION_LOW, 
    /// MEDIA_RESOLUTION_MEDIUM,
    /// MEDIA_RESOLUTION_HIGH
    /// </summary>
    [JsonPropertyName("mediaResolution")]
    public string? MediaResolution { get; set; }
}
using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

/// <summary>유해 카테고리.</summary>
internal enum HarmCategory
{
    HARM_CATEGORY_UNSPECIFIED,

    HARM_CATEGORY_DEROGATORY, // PaLM
    HARM_CATEGORY_TOXICITY, // PaLM
    HARM_CATEGORY_VIOLENCE, // PaLM
    HARM_CATEGORY_SEXUAL, // PaLM
    HARM_CATEGORY_MEDICAL, // PaLM
    HARM_CATEGORY_DANGEROUS, // PaLM

    HARM_CATEGORY_HARASSMENT, // Gemini
    HARM_CATEGORY_HATE_SPEECH, // Gemini
    HARM_CATEGORY_SEXUALLY_EXPLICIT, // Gemini
    HARM_CATEGORY_DANGEROUS_CONTENT // Gemini
}

/// <summary>유해 확률(차단 임계치).</summary>
internal enum HarmThreshold
{
    HARM_BLOCK_THRESHOLD_UNSPECIFIED,
    BLOCK_LOW_AND_ABOVE,
    BLOCK_MEDIUM_AND_ABOVE,
    BLOCK_ONLY_HIGH,
    BLOCK_NONE,
    OFF
}

/// <summary>유해 확률(평가 결과).</summary>
internal enum HarmProbability
{
    HARM_PROBABILITY_UNSPECIFIED,
    NEGLIGIBLE,
    LOW,
    MEDIUM,
    HIGH
}

/// <summary>안전 설정(카테고리별 차단 임계치).</summary>
internal sealed class SafetySetting
{
    [JsonPropertyName("category")]
    public required HarmCategory Category { get; set; }

    [JsonPropertyName("threshold")]
    public required HarmThreshold Threshold { get; set; }
}

/// <summary>안전도 평가(카테고리/확률/차단 여부).</summary>
internal sealed class SafetyRating
{
    [JsonPropertyName("category")]
    public required HarmCategory Category { get; set; }

    [JsonPropertyName("probability")]
    public required HarmProbability Probability { get; set; }

    /// <summary>이 카테고리 때문에 컨텐츠가 차단되었는지.</summary>
    [JsonPropertyName("blocked")]
    public bool? Blocked { get; set; }
}
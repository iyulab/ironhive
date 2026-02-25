using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Audio;

/// <summary>
/// 전사(Transcription) API 사용량 정보 베이스 클래스
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TranscriptionTokenUsage), "tokens")]
[JsonDerivedType(typeof(TranscriptionDurationUsage), "duration")]
public abstract class TranscriptionUsage
{ }

/// <summary>
/// 토큰 기반 사용량 정보
/// <para>gpt-4o-transcribe 등 토큰 기반 모델의 사용량을 나타냅니다.</para>
/// </summary>
public class TranscriptionTokenUsage : TranscriptionUsage
{
    /// <summary>
    /// 입력 토큰 수
    /// </summary>
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }
    
    /// <summary>
    /// 출력 토큰 수
    /// </summary>
    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }

    /// <summary>
    /// 총 토큰 수 (입력 + 출력)
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    /// <summary>
    /// 입력 토큰 상세 정보
    /// </summary>
    [JsonPropertyName("input_token_details")]
    public TranscriptionTokenDetails? InputTokenDetails { get; set; }
}

/// <summary>
/// 토큰 상세 정보
/// <para>오디오 토큰과 텍스트 토큰을 구분하여 제공합니다.</para>
/// </summary>
public class TranscriptionTokenDetails
{
    /// <summary>
    /// 오디오 처리에 사용된 토큰 수
    /// </summary>
    [JsonPropertyName("audio_tokens")]
    public int? AudioTokens { get; set; }

    /// <summary>
    /// 텍스트 처리에 사용된 토큰 수 (프롬프트 등)
    /// </summary>
    [JsonPropertyName("text_tokens")]
    public int? TextTokens { get; set; }
}

/// <summary>
/// 시간 기반 사용량 정보
/// <para>whisper-1 등 시간 기반 모델의 사용량을 나타냅니다.</para>
/// </summary>
public class TranscriptionDurationUsage : TranscriptionUsage
{
    /// <summary>
    /// 처리된 오디오 길이 (초)
    /// </summary>
    [JsonPropertyName("seconds")]
    public int Seconds { get; set; }
}
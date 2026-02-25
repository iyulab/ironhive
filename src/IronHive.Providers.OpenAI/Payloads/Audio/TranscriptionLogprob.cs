using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Audio;

/// <summary>
/// 전사(Transcription) 토큰의 로그 확률 정보
/// </summary>
public class TranscriptionLogprob
{
    /// <summary>
    /// 토큰 문자열
    /// </summary>
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    /// <summary>
    /// 토큰의 바이트 표현
    /// </summary>
    [JsonPropertyName("bytes")]
    public byte[]? Bytes { get; set; }

    /// <summary>
    /// 로그 확률 값
    /// <para>값이 클수록 모델의 확신도가 높음을 의미합니다.</para>
    /// </summary>
    [JsonPropertyName("logprob")]
    public float? Logprob { get; set; }
}

using IronHive.Abstractions.Audio;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Audio;

/// <summary>
/// OpenAI Speech-to-Text 요청 (multipart/form-data)
/// </summary>
public class CreateTranscriptionRequest
{
    /// <summary>
    /// 오디오 파일 (최대 25MB)
    /// </summary>
    [JsonIgnore]
    public required GeneratedAudio Audio { get; set; }

    /// <summary>
    /// 모델 ID 
    /// <para>"whisper-1", "gpt-4o-transcribe", "gpt-4o-mini-transcribe"</para>
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// 오디오 청킹(분할) 전략
    /// <para>오디오 데이터를 처리 중 청크로 나누는 방식을 설정합니다.</para>
    /// <para>적절한 청킹 전략을 선택하면 성능과 메모리 사용량에 영향을 줄 수 있습니다.</para>
    /// </summary>
    [JsonPropertyName("chunking_strategy")]
    public AudioChunkingStrategy? ChunkingStrategy { get; set; }

    /// <summary>
    /// 응답에 포함할 추가 정보
    /// <para>현재 "logprobs"만 지원됩니다.</para>
    /// </summary>
    [JsonPropertyName("include")]
    public string[]? Include { get; set; }

    /// <summary>
    /// 알려진 화자 이름 목록
    /// <para>known_speaker_references[]에 제공된 오디오 샘플에 대응하는 화자 이름입니다.</para>
    /// <para>각 항목은 짧은 식별자여야 하며, 최대 4명의 화자를 지원합니다.</para>
    /// </summary>
    [JsonPropertyName("known_speaker_names")]
    public string[]? KnownSpeakerNames { get; set; }

    /// <summary>
    /// 알려진 화자 참조 오디오 샘플 목록 (Data URL 형식)
    /// <para>각 샘플은 2~10초 사이여야 합니다.</para>
    /// </summary>
    [JsonPropertyName("known_speaker_references")]
    public string[]? KnownSpeakerReferences { get; set; }

    /// <summary>
    /// 언어 코드 (ISO-639-1)
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// 프롬프트 (문맥 힌트)
    /// </summary>
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    /// <summary>
    /// 응답 형식
    /// <para>"json", "text", "srt", "verbose_json", "vtt", "diarized_json"</para>
    /// <para>gpt-4o-transcribe-diarize 모델의 경우 diarized_json 형식이 지원되며, 화자 구분 정보를 받으려면 반드시 diarized_json을 사용해야 합니다.</para>
    /// </summary>
    [JsonPropertyName("response_format")]
    public string? ResponseFormat { get; set; }

    /// <summary>
    /// 스트리밍 여부
    /// </summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    /// <summary>
    /// 샘플링 온도 (0~1)
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    /// <summary>
    /// 타임스탬프 세분성 ("word", "segment")
    /// </summary>
    [JsonPropertyName("timestamp_granularities")]
    public string[]? TimestampGranularities { get; set; }
}
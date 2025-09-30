using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Share.Models;

/// <summary>
/// Content를 구성하는 유니온 파트.
/// </summary>
internal sealed class ContentPart
{
    /// <summary>
    /// 현재 파트가 사고(Thinking) 토큰인지 여부(Thinking 기능 사용 시).
    /// </summary>
    [JsonPropertyName("thought")]
    public bool? Thought { get; set; }

    /// <summary>
    /// Base64 인코딩된 사고 토큰(Thinking 기능 사용 시).
    /// </summary>
    [JsonPropertyName("thoughtSignature")]
    public string? ThoughtSignature { get; set; }

    #region Modality Union Type (아래 prop중 하나만)

    /// <summary>순수 텍스트.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>바이트 인라인 데이터(이미지/오디오 등). data는 base64.</summary>
    [JsonPropertyName("inlineData")]
    public BlobData? InlineData { get; set; }

    /// <summary>모델이 예측한 함수 호출.</summary>
    [JsonPropertyName("functionCall")]
    public FunctionCall? FunctionCall { get; set; }

    /// <summary>호출된 함수의 실행 결과를 모델 컨텍스트로 제공.</summary>
    [JsonPropertyName("functionResponse")]
    public FunctionResponse? FunctionResponse { get; set; }

    /// <summary>Files API로 올린 파일 참조.</summary>
    [JsonPropertyName("fileData")]
    public FileData? FileData { get; set; }

    /// <summary>코드 실행 도구 사용 시, 실행 가능한 코드 블록.</summary>
    [JsonPropertyName("executableCode")]
    public ExecutableCode? ExecutableCode { get; set; }

    /// <summary>코드 실행 결과(표준 출력/에러/상태 등).</summary>
    [JsonPropertyName("codeExecutionResult")]
    public ExecutionResult? CodeExecutionResult { get; set; }

    #endregion

    #region Metadata Union Type (아래 prop중 하나만)

    /// <summary>
    /// Video 컨텐츠의 메타데이터(시작/종료 오프셋, 프레임 속도 등).
    /// </summary>
    [JsonPropertyName("videoMetadata")]
    public VideoMetadata? VideoMetadata { get; set; }

    #endregion
}
namespace IronHive.Abstractions.ChatCompletion;

/// <summary>
/// 챗 컴플리션 모델에 대한 정보입니다.
/// </summary>
public class ChatCompletionModel
{
    /// <summary>
    /// 모델의 이름(또는 식별자)입니다.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// 모델의 표시 이름입니다.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 모델에 대한 설명입니다.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 모델이 지원하는 최대 컨텍스트 길이입니다.
    /// </summary>
    public int? MaxContextLength { get; set; }

    /// <summary>
    /// 모델이 생성할 수 있는 최대 출력 토큰 수입니다.
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// 모델의 기능들입니다.
    /// </summary>
    public ChatCompletionCapabilities? Capabilities { get; set; }

    /// <summary>
    /// 모델의 생성 일자입니다.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 모델의 소유자입니다.
    /// </summary>
    public string? Owner { get; set; }
}

/// <summary>
/// 챗 컴플리션 모델의 기능들을 나타냅니다.
/// </summary>
public class ChatCompletionCapabilities
{
    /// <summary>
    /// 도구 호출 지원 여부.
    /// </summary>
    public bool SupportsToolCall { get; set; } = false;

    /// <summary>
    /// 이미지 입력 지원 여부.
    /// </summary>
    public bool SupportsVision { get; set; } = false;

    /// <summary>
    /// 오디오 출력 지원 여부.
    /// </summary>
    public bool SupportsAudio { get; set; } = false;

    /// <summary>
    /// 추론(사고) 지원 여부.
    /// </summary>
    public bool SupportsReasoning { get; set; } = false;
}

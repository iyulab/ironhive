namespace IronHive.Abstractions.Audio;

/// <summary>
/// STT 응답. 변환된 텍스트를 포함합니다.
/// </summary>
public class SpeechToTextResponse
{
    /// <summary>
    /// 변환된 전체 텍스트
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// 변환된 텍스트의 세그먼트 정보 (화자 분리, 타임스탬프 등). 지원되는 경우에만 포함됩니다.
    /// </summary>
    public IReadOnlyList<TranscriptionSegment>? Segments { get; set; }
}

/// <summary>
/// 오디오에서 변환된 텍스트의 세그먼트 정보입니다. 화자 분리, 타임스탬프 등이 포함될 수 있습니다.
/// </summary>
public class TranscriptionSegment
{
    /// <summary>
    /// 화자 ID (화자 분리가 지원되는 경우)
    /// </summary>
    public string? Speaker { get; set; }

    /// <summary>
    /// 세그먼트 텍스트
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 시작 시간 (초)
    /// </summary>
    public float? Start { get; set; }

    /// <summary>
    /// 종료 시간 (초)
    /// </summary>
    public float? End { get; set; }
}
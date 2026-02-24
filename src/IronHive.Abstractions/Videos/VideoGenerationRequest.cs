using IronHive.Abstractions.Images;

namespace IronHive.Abstractions.Videos;

/// <summary>
/// 텍스트 프롬프트와 이미지를 모두 포함하여 비디오를 생성할 수 있습니다.
/// - Text-to-Video: Prompt만 설정
/// - Image-to-Video: Image만 설정
/// - Text+Image-to-Video: Prompt와 Image 모두 설정
/// </summary>
public class VideoGenerationRequest
{
    /// <summary>
    /// 모델 ID (e.g. "sora-2", "sora-2-pro", "veo-3.1-generate-preview")
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// 텍스트 프롬프트
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// 입력 이미지 (첫 프레임 / Image-to-Video),
    /// 이미지의 크기/비율이 비디오의 해상도/비율과 일치하도록 권장.
    /// </summary>
    public GeneratedImage? Image { get; set; }

    /// <summary>
    /// 비디오의 해상도 및 가로세로 비율,
    /// 서비스 제공자별 지원하는 형식에 따라 다른 객체를 사용하세요.
    /// </summary>
    public GeneratedVideoSize? Size { get; set; }

    /// <summary>
    /// 클립 길이(초). OpenAI: 4/8/12, Google: 4/6/8
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// 비디오 생성 작업이 완료될 때까지 상태를 폴링하는 간격.
    /// 기본값: 10초 (서비스 제공자의 권장값을 따르세요).
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(10);
}

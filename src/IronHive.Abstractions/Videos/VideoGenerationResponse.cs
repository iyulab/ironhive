namespace IronHive.Abstractions.Videos;

/// <summary>
/// 비디오 생성 응답. 완성된 비디오 데이터를 포함합니다.
/// </summary>
public class VideoGenerationResponse
{
    public required GeneratedVideo Video { get; set; }
}

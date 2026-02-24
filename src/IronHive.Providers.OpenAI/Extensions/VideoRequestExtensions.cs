using IronHive.Providers.OpenAI.Payloads.Videos;

namespace IronHive.Abstractions.Videos;

public static class VideoRequestExtensions
{
    /// <summary>
    /// 비디오 생성 요청을 OpenAI의 CreateVideoRequest로 변환합니다.
    /// </summary>
    public static CreateVideoRequest ToOpenAI(this VideoGenerationRequest request)
    {
        return new CreateVideoRequest
        {
            Model = request.Model,
            Prompt = request.Prompt,
            Image = request.Image,
            Seconds = request.DurationSeconds,
            Size = request.Size is GeneratedVideoCustomSize customSize
                ? customSize.Value
                : null
        };
    }
}

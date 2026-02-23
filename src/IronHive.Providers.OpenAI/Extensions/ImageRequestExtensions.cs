using IronHive.Providers.OpenAI.Payloads.Images;

namespace IronHive.Abstractions.Images;

public static class ImageRequestExtensions
{
    /// <summary>
    /// 이미지 생성 요청을 OpenAI의 CreateImagesRequest로 변환합니다.
    /// </summary>
    public static CreateImagesRequest ToOpenAI(this ImageGenerationRequest request)
    {
        return new CreateImagesRequest
        {
            Model = request.Model,
            Prompt = request.Prompt,
            N = request.N,
            Size = request.Size is GeneratedImageCustomSize customSize
                ? customSize.Value
                : null,

            // DALL-E만 지원하는 필드로, Base64를 반환받도록 설정합니다.
            ResponseFormat = request.Model.StartsWith("dall-e", StringComparison.OrdinalIgnoreCase)
                ? "b64_json"
                : null
        };
    }

    /// <summary>
    /// 이미지 편집 요청을 OpenAI의 EditImagesRequest로 변환합니다.
    /// </summary>
    public static EditImagesRequest ToOpenAI(this ImageEditRequest request)
    {
        return new EditImagesRequest
        {
            Model = request.Model,
            Prompt = request.Prompt,
            Images = request.Images
                .Where(img => img.Data.Length > 0 && !string.IsNullOrEmpty(img.MimeType))
                .Select(img => new OpenAIInputImageData 
                { 
                    ImageUrl = img.ToBase64()
                })
                .ToList(),
            Mask = request.Mask != null && request.Mask.Data.Length > 0 && !string.IsNullOrEmpty(request.Mask.MimeType)
                ? new OpenAIInputImageData 
                { 
                    ImageUrl = request.Mask.ToBase64()
                }
                : null,
            N = request.N,
            Size = request.Size is GeneratedImageCustomSize customSize
                ? customSize.Value
                : null,
        };
    }
}

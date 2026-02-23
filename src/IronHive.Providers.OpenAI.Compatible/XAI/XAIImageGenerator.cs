using IronHive.Abstractions.Images;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Payloads.Images;

namespace IronHive.Providers.OpenAI.Compatible.XAI;

/// <summary>
/// xAI (Grok) 서비스의 이미지 생성기입니다.
/// grok-2-vision-1212 모델을 사용하여 이미지를 생성합니다.
/// </summary>
public class XAIImageGenerator : OpenAIImageGenerator
{
    private readonly XAIConfig _config;

    public XAIImageGenerator(XAIConfig config) : base(config.ToOpenAI())
    {
        _config = config;
    }

    protected override CreateImagesRequest OnBeforeGenerate(
        ImageGenerationRequest source,
        CreateImagesRequest request)
    {
        // XAI에서 지원하지 않는 매개변수 제거
        request.Background = null;
        request.Moderation = null;
        request.OutputCompression = null;
        request.OutputFormat = null;
        request.PartialImages = null;
        request.Stream = null;
        request.Size = null;
        request.Style = null;

        // XAI는 기본적으로 b64_json 응답 형식 사용
        request.ResponseFormat = "b64_json";

        return base.OnBeforeGenerate(source, request);
    }

    protected override EditImagesRequest OnBeforeEdit(
        ImageEditRequest source,
        EditImagesRequest request)
    {
        // XAI에서 지원하지 않는 매개변수 제거
        request.Background = null;
        request.InputFidelity = null;
        request.Moderation = null;
        request.OutputCompression = null;
        request.OutputFormat = null;
        request.PartialImages = null;
        request.Stream = null;
        request.Size = null;

        return base.OnBeforeEdit(source, request);
    }
}

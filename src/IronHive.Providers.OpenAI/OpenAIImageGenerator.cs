using IronHive.Abstractions.Images;
using IronHive.Providers.OpenAI.Clients;
using IronHive.Providers.OpenAI.Payloads.Images;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// OpenAI DALL-E를 사용하여 이미지를 생성하는 클래스입니다.
/// </summary>
public class OpenAIImageGenerator : IImageGenerator
{
    private readonly OpenAIImageClient _client;

    public OpenAIImageGenerator(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIImageGenerator(OpenAIConfig config)
    {
        _client = new OpenAIImageClient(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual async Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = OnBeforeGenerate(request, new CreateImagesRequest
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
        });

        var response = await _client.PostCreateImagesAsync(payload, cancellationToken);
        
        return OnAfterGenerate(response, new ImageGenerationResponse
        {
            Images = response.Data?
                .Where(d => !string.IsNullOrEmpty(d.B64Json))
                .Select(d => new GeneratedImage
                {
                    Data = Convert.FromBase64String(d.B64Json!),
                    MimeType = $"image/{response.OutputFormat ?? "png"}",
                    RevisedPrompt = d.RevisedPrompt
                })
                .ToList() ?? []
        });
    }

    /// <inheritdoc />
    public virtual async Task<ImageGenerationResponse> EditImageAsync(
        ImageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = OnBeforeEdit(request, new EditImagesRequest
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
        });

        var response = await _client.PostEditImagesAsync(payload, cancellationToken);

        return OnAfterEdit(response, new ImageGenerationResponse
        {
            Images = response.Data?
                .Where(d => !string.IsNullOrEmpty(d.B64Json))
                .Select(d => new GeneratedImage
                {
                    Data = Convert.FromBase64String(d.B64Json!),
                    MimeType = $"image/{response.OutputFormat ?? "png"}",
                    RevisedPrompt = d.RevisedPrompt
                })
                .ToList() ?? []
        });
    }

    /// <summary>
    /// 이미지 생성 요청을 보내기 전에 처리할 수 있는 가상 메서드입니다.
    /// </summary>
    protected virtual CreateImagesRequest OnBeforeGenerate(
        ImageGenerationRequest source,
        CreateImagesRequest request)
        => request;

    /// <summary>
    /// 이미지 편집 요청을 보내기 전에 처리할 수 있는 가상 메서드입니다.
    /// </summary>
    protected virtual EditImagesRequest OnBeforeEdit(
        ImageEditRequest source,
        EditImagesRequest request)
        => request;

    /// <summary>
    /// 이미지 생성 응답을 받은 후 처리할 수 있는 가상 메서드입니다.
    /// </summary>
    protected virtual ImageGenerationResponse OnAfterGenerate(
        CreateImagesResponse source,
        ImageGenerationResponse response)
        => response;

    /// <summary>
    /// 이미지 편집 응답을 받은 후 처리할 수 있는 가상 메서드입니다.
    /// </summary>
    protected virtual ImageGenerationResponse OnAfterEdit(
        EditImagesResponse source,
        ImageGenerationResponse response)
        => response;
}

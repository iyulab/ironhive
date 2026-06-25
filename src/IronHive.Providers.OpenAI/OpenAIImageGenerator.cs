using System.ClientModel;
using IronHive.Abstractions.Images;
using OpenAI;
using OpenAI.Images;
using OpenAIGeneratedImage = OpenAI.Images.GeneratedImage;
using OpenAIGeneratedImageSize = OpenAI.Images.GeneratedImageSize;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// OpenAI를 사용하여 이미지를 생성하는 클래스입니다.
/// </summary>
public class OpenAIImageGenerator : IImageGenerator
{
    private readonly OpenAIClient _openai;

    public OpenAIImageGenerator(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIImageGenerator(OpenAIConfig config)
    {
        _openai = OpenAIClientFactory.Create(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual async Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = _openai.GetImageClient(request.Model);
        var options = new ImageGenerationOptions();

        if (request.Size is GeneratedImagePixelSize pixelSize && pixelSize.Width > 0 && pixelSize.Height > 0)
        {
            options.Size = new OpenAIGeneratedImageSize(pixelSize.Width, pixelSize.Height);
        }

        var result = await client.GenerateImagesAsync(
            request.Prompt,
            request.N ?? 1,
            options,
            cancellationToken);
        var images = result.Value;

        return new ImageGenerationResponse
        {
            Images = images
                .Where(img => img.ImageBytes != null)
                .Select(img => new IronHive.Abstractions.Images.GeneratedImage
                {
                    Data = img.ImageBytes!.ToArray(),
                    MimeType = "image/png",
                    RevisedPrompt = img.RevisedPrompt
                })
                .ToList()
        };
    }

    /// <inheritdoc />
    public virtual async Task<ImageGenerationResponse> EditImageAsync(
        ImageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var images = request.Images?.Where(img => img.Data.Length > 0).ToList()
            ?? throw new ArgumentException("At least one input image is required for editing.");
        if (images.Count == 0)
            throw new ArgumentException("At least one input image is required for editing.");

        var client = _openai.GetImageClient(request.Model);
        var options = new ImageEditOptions();

        if (request.Size is GeneratedImagePixelSize pixelSize && pixelSize.Width > 0 && pixelSize.Height > 0)
        {
            options.Size = new OpenAIGeneratedImageSize(pixelSize.Width, pixelSize.Height);
        }

        var mask = request.Mask;
        using var maskStream = mask?.Data.Length > 0 ? new MemoryStream(mask.Data) : null;
        var maskFilename = mask != null ? $"mask.{GetExtension(mask.MimeType)}" : null;

        var tasks = images.Select(async image =>
        {
            using var stream = new MemoryStream(image.Data);
            var ext = GetExtension(image.MimeType);
            var filename = $"image.{ext}";

            ClientResult<OpenAIGeneratedImage> result;
            if (maskStream != null && maskFilename != null)
            {
                result = await client.GenerateImageEditAsync(
                    stream, filename,
                    request.Prompt,
                    maskStream, maskFilename,
                    options,
                    cancellationToken);
            }
            else
            {
                result = await client.GenerateImageEditAsync(
                    stream, filename,
                    request.Prompt,
                    options,
                    cancellationToken);
            }

            var img = result.Value;
            return img.ImageBytes != null
                ? new IronHive.Abstractions.Images.GeneratedImage
                {
                    Data = img.ImageBytes.ToArray(),
                    MimeType = $"image/{ext}",
                    RevisedPrompt = img.RevisedPrompt
                }
                : null;
        });
        var results = await Task.WhenAll(tasks);

        return new ImageGenerationResponse
        {
            Images = results.Where(img => img != null).ToList()!
        };
    }

    private static string GetExtension(string? mimeType) => mimeType switch
    {
        "image/png" => "png",
        "image/jpeg" or "image/jpg" => "jpg",
        "image/webp" => "webp",
        _ => "png"
    };
}

using Google.GenAI;
using Google.GenAI.Types;
using IronHive.Abstractions.Images;
using System.Text;
using GeneratedImage = IronHive.Abstractions.Images.GeneratedImage;

namespace IronHive.Providers.GoogleAI;

/// <summary>
/// Google AI를 사용한 이미지 생성 제공자입니다.
/// Imagen 3 모델과 Gemini Image 모델을 지원합니다.
/// </summary>
public class GoogleAIImageGenerator : IImageGenerator
{
    private readonly Client _client;

    public GoogleAIImageGenerator(string apiKey)
        : this(new GoogleAIConfig { ApiKey = apiKey })
    { }

    public GoogleAIImageGenerator(GoogleAIConfig config)
    {
        _client = GoogleAIClientFactory.CreateClient(config);
    }

    public GoogleAIImageGenerator(VertexAIConfig config)
    {
        _client = GoogleAIClientFactory.CreateClient(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = request.Model;
        var preset = GetPresetSize(request.Size);

        // Imagen 모델 사용
        if (IsImagenModel(model))
        {
            var imagenResponse = await _client.Models.GenerateImagesAsync(
                model: model,
                prompt: request.Prompt,
                config: new GenerateImagesConfig
                {
                    NumberOfImages = request.N,
                    ImageSize = preset.Size,
                    AspectRatio = preset.Ratio,
                },
                cancellationToken: cancellationToken);

            return new ImageGenerationResponse
            {
                Images = imagenResponse?.GeneratedImages?
                    .Where(img => img.Image?.ImageBytes != null)
                    .Select(img => new GeneratedImage
                    {
                        MimeType = img.Image!.MimeType,
                        Data = img.Image!.ImageBytes!,
                    })
                    .ToList() ?? [],
            };
        }

        // Gemini Image 모델 사용
        var response = await _client.Models.GenerateContentAsync(
            model: model,
            contents:
            [
                new Content
                {
                    Parts =
                    [
                        new Part
                        {
                            Text = request.Prompt,
                        }
                    ]
                }
            ],
            config: new GenerateContentConfig
            {
                ResponseModalities = ["TEXT", "IMAGE"],
                ImageConfig = new ImageConfig
                {
                    ImageSize = preset.Size,
                    AspectRatio = preset.Ratio,
                }
            },
            cancellationToken: cancellationToken);

        return MapGeminiResponse(response);
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResponse> EditImageAsync(
        ImageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = request.Model;
        var preset = GetPresetSize(request.Size);

        // Imagen 모델 사용
        if (IsImagenModel(model))
        {
            var refImages = new List<IReferenceImage>();
            foreach (var img in request.Images)
            {
                refImages.Add(new RawReferenceImage
                {
                    ReferenceImage = new Image
                    {
                        MimeType = img.MimeType,
                        ImageBytes = img.Data,
                    }
                });
            }
            if (request.Mask != null)
            {
                refImages.Add(new MaskReferenceImage
                {
                    ReferenceImage = new Image
                    {
                        MimeType = request.Mask.MimeType,
                        ImageBytes = request.Mask.Data,
                    },
                });
            }

            var imagenResponse = await _client.Models.EditImageAsync(
                model: model,
                prompt: request.Prompt,
                referenceImages: refImages,
                config: new EditImageConfig
                {
                    NumberOfImages = request.N,
                    AspectRatio = preset.Ratio,
                },
                cancellationToken: cancellationToken);

            return new ImageGenerationResponse
            {
                Images = imagenResponse?.GeneratedImages?
                    .Where(img => img.Image?.ImageBytes != null && !string.IsNullOrEmpty(img.Image.MimeType))
                    .Select(img => new GeneratedImage
                    {
                        MimeType = img.Image!.MimeType,
                        Data = img.Image!.ImageBytes!,
                    })
                    .ToList() ?? [],
            };
        }

        // Gemini Image 모델 사용
        var response = await _client.Models.GenerateContentAsync(
            model: model,
            contents:
            [
                new Content
                {
                    Parts =
                    [
                        new Part
                        {
                            Text = request.Prompt,
                        }
                    ]
                },
                new Content
                {
                    Parts = request.Images.Select(img => new Part
                    {
                        InlineData = new Blob
                        {
                            MimeType = img.MimeType,
                            Data = img.Data,
                        }
                    }).ToList()
                },
            ],
            config: new GenerateContentConfig
            {
                ResponseModalities = ["TEXT", "IMAGE"],
                ImageConfig = new ImageConfig
                {
                    ImageSize = preset.Size,
                    AspectRatio = preset.Ratio,
                }
            },
            cancellationToken: cancellationToken);
        
        return MapGeminiResponse(response);
    }

    private static bool IsImagenModel(string model)
    {
        return model.Contains("imagen", StringComparison.OrdinalIgnoreCase);
    }

    private static (string? Size, string? Ratio) GetPresetSize(GeneratedImageSize? size)
    {
        if (size is GeneratedImagePresetSize preset)
        {
            return (preset.Resolution, preset.AspectRatio);
        }
        return (null, null);
    }

    private static ImageGenerationResponse MapGeminiResponse(GenerateContentResponse? response)
    {
        var promptBuilder = new StringBuilder();
        var images = new List<GeneratedImage>();

        foreach (var part in response?.Candidates?.FirstOrDefault()?.Content?.Parts ?? [])
        {
            if (part.Text != null) promptBuilder.Append(part.Text);
            if (part.InlineData?.Data != null && !string.IsNullOrEmpty(part.InlineData.MimeType))
            {
                images.Add(new GeneratedImage
                {
                    MimeType = part.InlineData.MimeType,
                    Data = part.InlineData.Data,
                });
            }
        }

        return new ImageGenerationResponse
        {
            Prompt = promptBuilder.Length > 0 ? promptBuilder.ToString() : null,
            Images = images,
        };
    }
}

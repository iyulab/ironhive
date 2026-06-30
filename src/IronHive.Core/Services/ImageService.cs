using IronHive.Abstractions.Images;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class ImageService : IImageService
{
    private readonly IReadOnlyDictionary<string, IImageGenerator> _generators;

    internal ImageService(IReadOnlyDictionary<string, IImageGenerator> generators)
    {
        _generators = generators;
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResponse> GenerateImageAsync(
        string provider,
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_generators.TryGetValue(provider, out var generator))
            throw new KeyNotFoundException($"Image generator '{provider}' not found.");

        var result = await generator.GenerateImageAsync(request, cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResponse> EditImageAsync(
        string provider,
        ImageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_generators.TryGetValue(provider, out var generator))
            throw new KeyNotFoundException($"Image generator '{provider}' not found.");

        var result = await generator.EditImageAsync(request, cancellationToken).ConfigureAwait(false);
        return result;
    }
}

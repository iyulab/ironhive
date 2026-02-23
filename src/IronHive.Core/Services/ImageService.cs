using IronHive.Abstractions.Images;
using IronHive.Abstractions.Registries;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class ImageService : IImageService
{
    private readonly IProviderRegistry _providers;

    public ImageService(IProviderRegistry providers)
    {
        _providers = providers;
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResponse> GenerateImageAsync(
        string provider,
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGet<IImageGenerator>(provider, out var generator))
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
        if (!_providers.TryGet<IImageGenerator>(provider, out var generator))
            throw new KeyNotFoundException($"Image generator '{provider}' not found.");

        var result = await generator.EditImageAsync(request, cancellationToken).ConfigureAwait(false);
        return result;
    }
}

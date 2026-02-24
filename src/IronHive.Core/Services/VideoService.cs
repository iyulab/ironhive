using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Videos;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class VideoService : IVideoService
{
    private readonly IProviderRegistry _providers;

    public VideoService(IProviderRegistry providers)
    {
        _providers = providers;
    }

    /// <inheritdoc />
    public async Task<VideoGenerationResponse> GenerateVideoAsync(
        string provider,
        VideoGenerationRequest request,
        IProgress<VideoGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGet<IVideoGenerator>(provider, out var generator))
            throw new KeyNotFoundException($"Video generator '{provider}' not found.");

        var result = await generator.GenerateVideoAsync(request, progress, cancellationToken).ConfigureAwait(false);
        return result;
    }
}

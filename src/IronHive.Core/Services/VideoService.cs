using IronHive.Abstractions.Videos;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class VideoService : IVideoService
{
    private readonly IReadOnlyDictionary<string, IVideoGenerator> _generators;

    internal VideoService(IReadOnlyDictionary<string, IVideoGenerator> generators)
    {
        _generators = generators;
    }

    /// <inheritdoc />
    public async Task<VideoGenerationResponse> GenerateVideoAsync(
        string provider,
        VideoGenerationRequest request,
        IProgress<VideoGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!_generators.TryGetValue(provider, out var generator))
            throw new KeyNotFoundException($"Video generator '{provider}' not found.");

        var result = await generator.GenerateVideoAsync(request, progress, cancellationToken).ConfigureAwait(false);
        return result;
    }
}

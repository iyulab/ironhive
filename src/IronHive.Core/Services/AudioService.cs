using IronHive.Abstractions.Audio;
using IronHive.Abstractions.Registries;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class AudioService : IAudioService
{
    private readonly IProviderRegistry _providers;

    public AudioService(IProviderRegistry providers)
    {
        _providers = providers;
    }

    /// <inheritdoc />
    public async Task<TextToSpeechResponse> GenerateSpeechAsync(
        string provider,
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGet<IAudioProcessor>(provider, out var processor))
            throw new KeyNotFoundException($"Audio processor '{provider}' not found.");

        var result = await processor.GenerateSpeechAsync(request, cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc />
    public async Task<SpeechToTextResponse> TranscribeAsync(
        string provider,
        SpeechToTextRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGet<IAudioProcessor>(provider, out var processor))
            throw new KeyNotFoundException($"Audio processor '{provider}' not found.");

        var result = await processor.TranscribeAsync(request, cancellationToken).ConfigureAwait(false);
        return result;
    }
}

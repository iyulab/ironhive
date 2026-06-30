using IronHive.Abstractions.Audio;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class AudioService : IAudioService
{
    private readonly IReadOnlyDictionary<string, IAudioProcessor> _processors;

    internal AudioService(IReadOnlyDictionary<string, IAudioProcessor> processors)
    {
        _processors = processors;
    }

    /// <inheritdoc />
    public async Task<TextToSpeechResponse> GenerateSpeechAsync(
        string provider,
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_processors.TryGetValue(provider, out var processor))
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
        if (!_processors.TryGetValue(provider, out var processor))
            throw new KeyNotFoundException($"Audio processor '{provider}' not found.");

        var result = await processor.TranscribeAsync(request, cancellationToken).ConfigureAwait(false);
        return result;
    }
}

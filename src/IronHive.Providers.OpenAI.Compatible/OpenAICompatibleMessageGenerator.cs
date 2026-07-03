using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// Message generator for a generic OpenAI-compatible endpoint. Delegates to
/// <see cref="ChatCompletionMessageGenerator"/> (Chat Completions surface) over the resolved <c>/v1</c> base URL.
/// When <see cref="OpenAICompatibleConfig.BaseUrlResolver"/> or <see cref="OpenAICompatibleConfig.ApiKeyResolver"/>
/// is set, the endpoint/key is re-resolved per request and the inner generator is swapped on change.
/// </summary>
public class OpenAICompatibleMessageGenerator : IMessageGenerator
{
    private readonly OpenAICompatibleConfig _config;

    private string _lastResolvedSignature;
    private ChatCompletionMessageGenerator _inner;
    private readonly Lock _lock = new();

    private static string BuildSignature(string baseUrl, string apiKey) => baseUrl + '\0' + apiKey;

    public OpenAICompatibleMessageGenerator(OpenAICompatibleConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _inner = new ChatCompletionMessageGenerator(config.ToOpenAI());
        _lastResolvedSignature = BuildSignature(config.ResolveBaseUrl(), config.ResolveApiKey());
    }

    private ChatCompletionMessageGenerator GetOrUpdateInner()
    {
        if (_config.BaseUrlResolver == null && _config.ApiKeyResolver == null)
            return _inner;

        var current = BuildSignature(_config.ResolveBaseUrl(), _config.ResolveApiKey());
        if (current == _lastResolvedSignature)
            return _inner;

        lock (_lock)
        {
            current = BuildSignature(_config.ResolveBaseUrl(), _config.ResolveApiKey());
            if (current == _lastResolvedSignature)
                return _inner;

            _inner = new ChatCompletionMessageGenerator(_config.ToOpenAI());
            _lastResolvedSignature = current;
        }

        return _inner;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _inner.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
        => GetOrUpdateInner().GenerateMessageAsync(request, cancellationToken);

    /// <inheritdoc/>
    public IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
        => GetOrUpdateInner().GenerateStreamingMessageAsync(request, cancellationToken);

    /// <inheritdoc/>
    public Task<int> CountTokensAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
        => GetOrUpdateInner().CountTokensAsync(request, cancellationToken);
}

using IronHive.Abstractions.Messages;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// OpenAI message generator facade. Dispatches to the API surface selected by <see cref="OpenAIConfig.Api"/>:
/// <see cref="OpenAIApiSurface.Responses"/> (first-party default, <see cref="OpenAIResponseMessageGenerator"/>) or
/// <see cref="OpenAIApiSurface.ChatCompletions"/> (<see cref="OpenAIChatMessageGenerator"/>, the surface
/// OpenAI-compatible / self-hosted endpoints implement).
/// <para>
/// This indirection is why <c>AddOpenAICompatibleProviders</c> and the GPUStack provider — which construct this
/// type over a config whose <see cref="OpenAIConfig.Api"/> is <see cref="OpenAIApiSurface.ChatCompletions"/> —
/// automatically target <c>/v1/chat/completions</c> instead of the OpenAI-proprietary <c>/v1/responses</c>.
/// </para>
/// </summary>
public class OpenAIMessageGenerator : IMessageGenerator
{
    private readonly IMessageGenerator _inner;

    /// <summary>The API surface this generator dispatches to.</summary>
    internal OpenAIApiSurface Surface { get; }

    public OpenAIMessageGenerator(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIMessageGenerator(OpenAIConfig config)
    {
        Surface = config.Api;
        _inner = config.Api switch
        {
            OpenAIApiSurface.ChatCompletions => new OpenAIChatMessageGenerator(config),
            _ => new OpenAIResponseMessageGenerator(config),
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _inner.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
        => _inner.GenerateMessageAsync(request, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
        => _inner.GenerateStreamingMessageAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<int> CountTokensAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
        => _inner.CountTokensAsync(request, cancellationToken);
}

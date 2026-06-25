using IronHive.Abstractions.Messages;

namespace IronHive.Providers.OpenAI.Compatible.GpuStack;

/// <summary>
/// GPUStack 서비스를 위한 메시지 생성기입니다.
/// <see cref="GpuStackConfig.BaseUrlResolver"/> 또는 <see cref="GpuStackConfig.ApiKeyResolver"/>가
/// 설정된 경우 매 요청 시 동적으로 엔드포인트·키를 조회하고, 변경 시 내부 generator를 교체합니다.
/// </summary>
public class GpuStackMessageGenerator : IMessageGenerator
{
    private readonly GpuStackConfig _config;

    private string _lastResolvedSignature = string.Empty;
    private OpenAIMessageGenerator _inner;
    private readonly Lock _lock = new();

    private static string BuildSignature(string baseUrl, string apiKey) => baseUrl + '\0' + apiKey;

    public GpuStackMessageGenerator(GpuStackConfig config)
    {
        _config = config;
        _inner = new OpenAIMessageGenerator(config.ToOpenAI());
        _lastResolvedSignature = BuildSignature(config.ResolveBaseUrl(), config.ResolveApiKey());
    }

    private OpenAIMessageGenerator GetOrUpdateInner()
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

            _inner = new OpenAIMessageGenerator(_config.ToOpenAI());
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
}

using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI.Clients;

namespace IronHive.Providers.OpenAI.Compatible.GpuStack;

/// <summary>
/// GPUStack 서비스를 위한 메시지 생성기입니다.
/// <see cref="GpuStackConfig.BaseUrlResolver"/> 또는 <see cref="GpuStackConfig.ApiKeyResolver"/>가
/// 설정된 경우 매 요청 시 동적으로 엔드포인트·키를 조회하고, 변경 시 내부 generator를 교체합니다.
/// </summary>
public class GpuStackMessageGenerator : IMessageGenerator
{
    private readonly GpuStackConfig _config;

    // 마지막으로 사용된 (URL, ApiKey) 시그니처와 그 설정으로 생성된 내부 generator를 캐시합니다.
    // URL 또는 키가 변경되면 새 generator로 교체합니다.
    private string _lastResolvedSignature = string.Empty;
    private OpenAIChatMessageGenerator _inner;
    private readonly Lock _lock = new();

    // URL과 키를 합쳐 재생성 판단용 시그니처를 만듭니다. 키는 비교 용도로만 메모리에 머뭅니다.
    private static string BuildSignature(string baseUrl, string apiKey) => baseUrl + '\0' + apiKey;

    public GpuStackMessageGenerator(GpuStackConfig config)
    {
        _config = config;
        // 초기 generator를 현재 설정으로 생성합니다.
        _inner = new OpenAIChatMessageGenerator(config.ToOpenAI());
        _lastResolvedSignature = BuildSignature(config.ResolveBaseUrl(), config.ResolveApiKey());
    }

    /// <summary>
    /// 현재 설정에서 resolv된 (URL, ApiKey)를 확인하고, 변경된 경우 새 inner generator로 교체합니다.
    /// </summary>
    private OpenAIChatMessageGenerator GetOrUpdateInner()
    {
        // resolver가 하나도 없으면 정적 설정이므로 초기 generator를 그대로 사용합니다.
        if (_config.BaseUrlResolver == null && _config.ApiKeyResolver == null)
            return _inner;

        var current = BuildSignature(_config.ResolveBaseUrl(), _config.ResolveApiKey());
        if (current == _lastResolvedSignature)
            return _inner;

        lock (_lock)
        {
            // double-check 패턴
            current = BuildSignature(_config.ResolveBaseUrl(), _config.ResolveApiKey());
            if (current == _lastResolvedSignature)
                return _inner;

            // NOTE: 이전 inner는 Dispose하지 않습니다.
            // 락 밖에서 이전 _inner 참조를 이미 획득한 다른 스레드가 in-flight 요청 중일 수 있으며,
            // Dispose 시 ObjectDisposedException이 발생합니다.
            // 엔드포인트·키 변경은 드문 이벤트이므로, GC가 참조가 없어진 시점에 자동 회수합니다.
            _inner = new OpenAIChatMessageGenerator(_config.ToOpenAI());
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

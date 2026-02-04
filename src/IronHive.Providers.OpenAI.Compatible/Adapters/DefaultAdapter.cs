using System.Text.Json.Nodes;

namespace IronHive.Providers.OpenAI.Compatible.Adapters;

/// <summary>
/// vLLM, GPUStack, Custom 등 기본 OpenAI 호환 서비스용 어댑터입니다.
/// </summary>
public class DefaultAdapter : BaseProviderAdapter
{
    private readonly CompatibleProvider _provider;

    public DefaultAdapter(CompatibleProvider provider = CompatibleProvider.Custom)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public override CompatibleProvider Provider => _provider;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => _provider switch
    {
        CompatibleProvider.vLLM => "http://localhost:8000/v1",
        CompatibleProvider.GPUStack => string.Empty, // 사용자 정의 필수
        _ => string.Empty
    };

    /// <inheritdoc />
    public override string GetBaseUrl(CompatibleConfig config)
    {
        // vLLM, GPUStack, Custom은 BaseUrl 필수
        if (string.IsNullOrEmpty(config.BaseUrl))
        {
            var defaultUrl = DefaultBaseUrl;
            if (string.IsNullOrEmpty(defaultUrl))
            {
                throw new InvalidOperationException(
                    $"BaseUrl is required for provider '{_provider}'.");
            }
            return defaultUrl;
        }

        return config.BaseUrl;
    }
}

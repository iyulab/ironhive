using System.Text.Json;

namespace IronHive.Abstractions.Http;

/// <summary>
/// AI Provider 클라이언트의 공통 기본 클래스
/// </summary>
public abstract class ProviderClientBase : IDisposable
{
    protected readonly HttpClient _client;
    protected readonly JsonSerializerOptions _jsonOptions;

    protected ProviderClientBase(IProviderConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _client = CreateHttpClient(config);
        _jsonOptions = config.JsonOptions;
    }

    public virtual void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    private static HttpClient CreateHttpClient(IProviderConfig config)
    {
        var baseUrl = string.IsNullOrWhiteSpace(config.BaseUrl)
            ? config.GetDefaultBaseUrl()
            : config.BaseUrl;

        var client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.EnsureSuffix('/'))
        };

        config.ConfigureHttpClient(client);
        return client;
    }
}

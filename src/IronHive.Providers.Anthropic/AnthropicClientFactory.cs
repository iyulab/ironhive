using IronHive.Abstractions.Http;
using Anthropic;
using Anthropic.Core;

namespace IronHive.Providers.Anthropic;

internal static class AnthropicClientFactory
{
    internal static IAnthropicClient Create(AnthropicConfig config)
    {
        var options = new ClientOptions();

        if (!string.IsNullOrWhiteSpace(config.BaseUrl))
            options.BaseUrl = config.BaseUrl;
        var initialKey = config.ResolveApiKey();
        if (!string.IsNullOrWhiteSpace(initialKey))
            options.ApiKey = initialKey;
        if (!string.IsNullOrWhiteSpace(config.AuthToken))
            options.AuthToken = config.AuthToken;
        var headers = ProviderRequestHeaders.Resolve(
            nameof(AnthropicConfig), nameof(AnthropicConfig.ApiKey), ["Authorization", "x-api-key"],
            config.ExtraHeaders, config.Headers);
        if (headers is not null)
            options.ExtraHeaders = headers;
        if (config.MaxRetries.HasValue)
            options.MaxRetries = config.MaxRetries.Value;
        if (config.Timeout != System.Threading.Timeout.InfiniteTimeSpan)
            options.Timeout = config.Timeout;

        if (config.ApiKeyResolver != null && config.HttpClient != null)
            throw ResolvedCredentialHandler.ConflictsWithCustomHttpClient(nameof(AnthropicConfig), nameof(AnthropicConfig.HttpClient));

        HttpMessageHandler transport = new SocketsHttpHandler { ConnectTimeout = config.ConnectTimeout };
        if (config.ApiKeyResolver != null)
            transport = new ResolvedCredentialHandler("x-api-key", config.ApiKeyResolver, config.ApiKey, format: null, transport);

        options.HttpClient = config.HttpClient ?? new HttpClient(transport)
        {
            // A bare HttpClient's 100-second default would cap time-to-first-byte ahead of
            // options.Timeout and win. Disabling it here leaves options.Timeout (unset by default —
            // see AnthropicConfig.Timeout) as the only request-level ceiling.
            Timeout = System.Threading.Timeout.InfiniteTimeSpan
        };

        return new AnthropicClient(options);
    }
}

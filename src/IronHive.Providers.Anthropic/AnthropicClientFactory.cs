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
        if (!string.IsNullOrWhiteSpace(config.ApiKey))
            options.ApiKey = config.ApiKey;
        if (!string.IsNullOrWhiteSpace(config.AuthToken))
            options.AuthToken = config.AuthToken;
        if (config.ExtraHeaders != null)
            options.ExtraHeaders = config.ExtraHeaders.AsReadOnly();
        if (config.MaxRetries.HasValue)
            options.MaxRetries = config.MaxRetries.Value;
        if (config.Timeout != System.Threading.Timeout.InfiniteTimeSpan)
            options.Timeout = config.Timeout;

        options.HttpClient = config.HttpClient ?? new HttpClient(new SocketsHttpHandler
        {
            ConnectTimeout = config.ConnectTimeout
        })
        {
            // A bare HttpClient's 100-second default would cap time-to-first-byte ahead of
            // options.Timeout and win. Disabling it here leaves options.Timeout (unset by default —
            // see AnthropicConfig.Timeout) as the only request-level ceiling.
            Timeout = System.Threading.Timeout.InfiniteTimeSpan
        };

        return new AnthropicClient(options);
    }
}

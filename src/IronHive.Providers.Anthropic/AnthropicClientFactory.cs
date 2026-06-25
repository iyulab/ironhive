using Anthropic;
using Anthropic.Core;

namespace IronHive.Providers.Anthropic;

internal static class AnthropicClientFactory
{
    internal static IAnthropicClient Create(AnthropicConfig config)
    {
        var options = new ClientOptions();

        if (!string.IsNullOrWhiteSpace(config.BaseUrl))
            options.ApiKey = config.BaseUrl;
        if (!string.IsNullOrWhiteSpace(config.ApiKey))
            options.ApiKey = config.ApiKey;
        if (!string.IsNullOrWhiteSpace(config.AuthToken))
            options.AuthToken = config.AuthToken;
        if (config.ExtraHeaders != null)
            options.ExtraHeaders = config.ExtraHeaders.AsReadOnly();
        if (config.MaxRetries.HasValue)
            options.MaxRetries = config.MaxRetries.Value;
        if (config.Timeout.HasValue)
            options.Timeout = config.Timeout.Value;
        if (config.HttpClient != null)
            options.HttpClient = config.HttpClient;

        return new AnthropicClient(options);
    }
}

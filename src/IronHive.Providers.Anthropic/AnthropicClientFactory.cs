using Anthropic;

namespace IronHive.Providers.Anthropic;

internal static class AnthropicClientFactory
{
    internal static IAnthropicClient CreateClient(AnthropicConfig config)
    {
        var client = new AnthropicClient();
        return client.WithOptions(options =>
        {
            var updated = options with { };
            if (!string.IsNullOrWhiteSpace(config.ApiKey))
                updated = updated with { ApiKey = config.ApiKey };
            if (!string.IsNullOrWhiteSpace(config.AuthToken))
                updated = updated with { AuthToken = config.AuthToken };
            if (!string.IsNullOrWhiteSpace(config.BaseUrl))
                updated = updated with { BaseUrl = config.BaseUrl };
            if (config.MaxRetries.HasValue)
                updated = updated with { MaxRetries = config.MaxRetries.Value };
            if (config.Timeout.HasValue)
                updated = updated with { Timeout = config.Timeout.Value };

            return updated;
        });
    }
}

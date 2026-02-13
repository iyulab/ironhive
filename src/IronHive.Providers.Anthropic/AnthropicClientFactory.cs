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
            if (!string.IsNullOrWhiteSpace(config.BaseUrl))
                updated = updated with { BaseUrl = config.BaseUrl };
            return updated;
        });
    }
}

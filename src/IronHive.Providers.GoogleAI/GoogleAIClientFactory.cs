using Google.GenAI;

namespace IronHive.Providers.GoogleAI;

internal static class GoogleAIClientFactory
{
    internal static Client CreateClient(GoogleAIConfig config)
    {
        var httpOptions = string.IsNullOrWhiteSpace(config.BaseUrl)
            ? null
            : new Google.GenAI.Types.HttpOptions { BaseUrl = config.BaseUrl };

        return new Client(apiKey: config.ApiKey, httpOptions: httpOptions);
    }
}

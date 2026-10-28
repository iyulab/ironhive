using IronHive.Abstractions.Http;
using Anthropic;
using Anthropic.Core;

namespace IronHive.Providers.Anthropic;

internal static class AnthropicClientFactory
{
    internal const string BaseUrlVariable = "ANTHROPIC_BASE_URL";
    internal const string DefaultBaseUrl = "https://api.anthropic.com";
    internal const string UnresolvedKeyPlaceholder = "unresolved";

    internal static IAnthropicClient Create(AnthropicConfig config)
    {
        var options = new ClientOptions();

        if (!string.IsNullOrWhiteSpace(config.BaseUrl))
            options.BaseUrl = config.BaseUrl;
        else if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(BaseUrlVariable)))
            // The SDK reads this variable itself. One that exists but is blank (a CI template, a shell profile) is taken as
            // a base URL: every request then throws UriFormatException before it is sent. Blank means unset.
            options.BaseUrl = DefaultBaseUrl;
        var initialKey = config.ResolveApiKey();
        // The SDK (13.x) takes one credential and throws when a second is set; say which settings collide.
        if ((!string.IsNullOrWhiteSpace(initialKey) || config.ApiKeyResolver != null) && !string.IsNullOrWhiteSpace(config.AuthToken))
            throw new ArgumentException(
                $"{nameof(AnthropicConfig)}: set either {nameof(AnthropicConfig.ApiKey)} (or {nameof(AnthropicConfig.ApiKeyResolver)}) or {nameof(AnthropicConfig.AuthToken)}, not both - both are sent as 'Authorization: Bearer'.");
        if (!string.IsNullOrWhiteSpace(initialKey))
            options.ApiKey = initialKey;
        else if (config.ApiKeyResolver != null && string.IsNullOrWhiteSpace(config.AuthToken))
            // A resolver with nothing to say (a declaration whose credential is not the vendor's, a secret store still empty) must
            // not leave the SDK an empty slot: it would fill it from ANTHROPIC_API_KEY / ANTHROPIC_AUTH_TOKEN or the machine's
            // Anthropic profile and send that to whatever address this client was pointed at. A placeholder stops the search; the
            // handler below replaces the header the SDK writes with the resolver's answer, or with none.
            options.ApiKey = UnresolvedKeyPlaceholder;
        if (!string.IsNullOrWhiteSpace(config.AuthToken))
            options.AuthToken = config.AuthToken;
        var headers = ProviderRequestHeaders.Resolve(
            nameof(AnthropicConfig), nameof(AnthropicConfig.ApiKey), ["Authorization", "x-api-key"],
            config.ExtraHeaders, config.Headers);
        if (headers is not null)
            options.ExtraHeaders = headers;
        if (config.MaxRetries.HasValue)
            options.MaxRetries = config.MaxRetries.Value;
        // Always set: left unset, the SDK applies its own 10-minute default, which contradicts AnthropicConfig.Timeout's
        // documented default of no limit.
        options.Timeout = config.Timeout;

        if (config.ApiKeyResolver != null && config.HttpClient != null)
            throw ResolvedCredentialHandler.ConflictsWithCustomHttpClient(nameof(AnthropicConfig), nameof(AnthropicConfig.HttpClient));

        HttpMessageHandler transport = ProviderConnect.CreateHandler(config.ConnectTimeout);
        if (config.ApiKeyResolver != null)
            transport = new ResolvedCredentialHandler("Authorization", config.ApiKeyResolver, config.ApiKey, k => $"Bearer {k}", transport);

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

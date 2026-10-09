using IronHive.Abstractions.Http;
using System.ClientModel;
using System.ClientModel.Primitives;
using OpenAI;

namespace IronHive.Providers.OpenAI;

public static class OpenAIClientFactory
{
    /// <summary>
    /// Stands in for an absent key. <see cref="ApiKeyCredential"/> rejects an empty string, but an
    /// endpoint that requires no credential is a first-class configuration — the OpenAI-compatible
    /// provider documents the key as optional, and a base URL may point at a gateway that supplies
    /// the credential upstream.
    /// </summary>
    /// <remarks>
    /// This is sent, not dropped: requests carry <c>Authorization: Bearer</c> with this value.
    /// Runtimes that need no credential ignore it; one that rejects an unexpected header answers
    /// with an error naming the credential, which is still far better than the alternative — the
    /// vendor's <see cref="ArgumentException"/> aborted service registration before any request,
    /// naming neither the provider nor the field.
    /// </remarks>
    private const string NoCredential = "no-credential-required";

    public static OpenAIClient Create(OpenAIConfig config)
    {
        var initial = config.ResolveApiKey();
        var key = string.IsNullOrWhiteSpace(initial) ? NoCredential : initial;
        return new OpenAIClient(new ApiKeyCredential(key), BuildOptions(config));
    }

    /// <summary>
    /// Maps the config onto the vendor client's options. Split out so the mapping can be asserted:
    /// the constructed client exposes none of these values, and a field routed to the wrong slot is
    /// invisible at compile time.
    /// </summary>
    /// <summary>The longest budget a timer accepts — about 24.8 days, i.e. no limit in practice.</summary>
    internal static readonly TimeSpan NoNetworkTimeout = TimeSpan.FromMilliseconds(int.MaxValue);

    internal static OpenAIClientOptions BuildOptions(OpenAIConfig config)
    {
        var options = new OpenAIClientOptions();

        if (!string.IsNullOrWhiteSpace(config.BaseUrl))
            options.Endpoint = new Uri(config.BaseUrl.EnsureSuffix('/'));
        if (!string.IsNullOrWhiteSpace(config.Organization))
            options.OrganizationId = config.Organization;
        if (!string.IsNullOrWhiteSpace(config.Project))
            options.ProjectId = config.Project;
        // Always set: left unset, the pipeline applies its own 100-second default to every network operation (each
        // read of a stream included), which contradicts OpenAIConfig.Timeout's documented default of no limit. "No limit"
        // is the longest finite budget rather than InfiniteTimeSpan: with an infinite one the pipeline stops wrapping a
        // streamed body in its read-timeout stream, and then disposing a fully read stream that happens to be seekable (a
        // handler that buffered the body) throws "Content stream position is not at beginning of stream".
        options.NetworkTimeout = config.Timeout == System.Threading.Timeout.InfiniteTimeSpan ? NoNetworkTimeout : config.Timeout;
        if (config.MaxRetries is { } maxRetries)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxRetries, $"{nameof(OpenAIConfig)}.{nameof(OpenAIConfig.MaxRetries)}");
            options.RetryPolicy = new ClientRetryPolicy(maxRetries);
        }

        var placement = config.ApiKeyPlacement ?? CredentialPlacement.Bearer;
        var defaultPlacement = placement.Equals(CredentialPlacement.Bearer);

        // The SDK writes Authorization: Bearer itself. Any other placement is written by a pipeline policy after the
        // SDK's credential policy, which reads the resolver too — so it needs no handler and works with an injected
        // HttpClient. The default placement keeps the resolver in a transport handler, as before.
        if (defaultPlacement && config.ApiKeyResolver != null && config.HttpClient != null)
            throw ResolvedCredentialHandler.ConflictsWithCustomHttpClient(nameof(OpenAIConfig), nameof(OpenAIConfig.HttpClient));

        HttpMessageHandler transport = ProviderConnect.CreateHandler(config.ConnectTimeout);
        if (defaultPlacement && config.ApiKeyResolver != null)
            transport = new ResolvedCredentialHandler("Authorization", config.ApiKeyResolver, config.ApiKey, k => $"Bearer {k}", transport);

        var httpClient = config.HttpClient ?? new HttpClient(transport)
        {
            // A bare HttpClient's 100-second default would cap time-to-first-byte ahead of
            // NetworkTimeout and win. Disabling it here leaves NetworkTimeout (unset by default —
            // see OpenAIConfig.Timeout) as the only request-level ceiling, matching what the SDK's
            // own default transport already does when no client is injected at all.
            Timeout = System.Threading.Timeout.InfiniteTimeSpan
        };
        options.Transport = new HttpClientPipelineTransport(httpClient);

        // Gateway headers ride BeforeTransport: the SDK's credential policy runs after the per-call
        // stage, so a header added there is overwritten by the bearer token. Here it is applied to the
        // assembled request, and Authorization itself is refused at resolution (see ProviderRequestHeaders).
        if (!defaultPlacement)
            options.AddPolicy(new CredentialPlacementPolicy(placement, config.ApiKeyResolver, config.ApiKey), PipelinePosition.BeforeTransport);

        // Always registered: a request's own headers (MessageGenerationRequest.Headers) ride the same policy.
        var headers = ProviderRequestHeaders.Resolve(nameof(OpenAIConfig), nameof(OpenAIConfig.ApiKey), placement.ReservedHeaderNames, config.Headers);
        options.AddPolicy(new ExtraRequestHeadersPolicy(headers), PipelinePosition.BeforeTransport);

        return options;
    }
}

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
        var key = string.IsNullOrWhiteSpace(config.ApiKey) ? NoCredential : config.ApiKey;
        return new OpenAIClient(new ApiKeyCredential(key), BuildOptions(config));
    }

    /// <summary>
    /// Maps the config onto the vendor client's options. Split out so the mapping can be asserted:
    /// the constructed client exposes none of these values, and a field routed to the wrong slot is
    /// invisible at compile time.
    /// </summary>
    internal static OpenAIClientOptions BuildOptions(OpenAIConfig config)
    {
        var options = new OpenAIClientOptions();

        if (!string.IsNullOrWhiteSpace(config.BaseUrl))
            options.Endpoint = new Uri(config.BaseUrl.EnsureSuffix('/'));
        if (!string.IsNullOrWhiteSpace(config.Organization))
            options.OrganizationId = config.Organization;
        if (!string.IsNullOrWhiteSpace(config.Project))
            options.ProjectId = config.Project;
        if (config.Timeout != System.Threading.Timeout.InfiniteTimeSpan)
            options.NetworkTimeout = config.Timeout;

        var httpClient = config.HttpClient ?? new HttpClient(new SocketsHttpHandler
        {
            ConnectTimeout = config.ConnectTimeout
        })
        {
            // A bare HttpClient's 100-second default would cap time-to-first-byte ahead of
            // NetworkTimeout and win. Disabling it here leaves NetworkTimeout (unset by default —
            // see OpenAIConfig.Timeout) as the only request-level ceiling, matching what the SDK's
            // own default transport already does when no client is injected at all.
            Timeout = System.Threading.Timeout.InfiniteTimeSpan
        };
        options.Transport = new HttpClientPipelineTransport(httpClient);

        return options;
    }
}

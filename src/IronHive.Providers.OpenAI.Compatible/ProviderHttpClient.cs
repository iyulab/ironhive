using IronHive.Abstractions.Http;
using IronHive.Providers.OpenAI;

namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// The HTTP side of this assembly's hand-written clients (Chat Completions, rerank), with one rule for
/// <see cref="OpenAIConfig.HttpClient"/>.
/// </summary>
/// <remarks>
/// An injected client is the consumer's — typically from <c>IHttpClientFactory</c> and shared — so it is used as given:
/// nothing is set on it (a client that has sent a request refuses changes) and it is never disposed. The endpoint, the
/// credentials and <see cref="OpenAIConfig.Timeout"/> travel with each request instead, which is also why several
/// clients can share one <see cref="HttpClient"/>. Without an injected client one is created and owned here, with
/// <see cref="OpenAIConfig.ConnectTimeout"/> and no client-level timeout, so the per-request timeout is the only one.
/// </remarks>
internal sealed class ProviderHttpClient : IDisposable
{
    private readonly bool _ownsHttp;
    private readonly Uri _endpoint;
    private readonly TimeSpan _timeout;
    private readonly TimeSpan _streamIdleTimeout;
    private readonly string? _apiKey;
    private readonly Func<string?>? _apiKeyResolver;
    private readonly CredentialPlacement _placement;
    private readonly string? _organization;
    private readonly string? _project;
    private readonly IReadOnlyDictionary<string, string>? _headers;

    /// <param name="config">The resolved configuration.</param>
    /// <param name="path">The operation's path relative to <see cref="OpenAIConfig.BaseUrl"/>.</param>
    /// <param name="defaultBaseUrl">Used when <see cref="OpenAIConfig.BaseUrl"/> is blank; null requires one.</param>
    /// <param name="sendAccountHeaders">Whether <c>OpenAI-Organization</c>/<c>OpenAI-Project</c> are sent.</param>
    public ProviderHttpClient(OpenAIConfig config, string path, string? defaultBaseUrl, bool sendAccountHeaders)
    {
        _placement = config.ApiKeyPlacement ?? CredentialPlacement.Bearer;
        _headers = ProviderRequestHeaders.Resolve(nameof(OpenAIConfig), nameof(OpenAIConfig.ApiKey), _placement.ReservedHeaderNames, config.Headers);
        _ownsHttp = config.HttpClient is null;
        Http = config.HttpClient ?? new HttpClient(ProviderConnect.CreateHandler(config.ConnectTimeout))
        {
            Timeout = System.Threading.Timeout.InfiniteTimeSpan,
        };

        var baseUrl = string.IsNullOrWhiteSpace(config.BaseUrl)
            ? defaultBaseUrl ?? throw new ArgumentException($"{nameof(OpenAIConfig)}.{nameof(OpenAIConfig.BaseUrl)} is required.", nameof(config))
            : config.BaseUrl;
        _endpoint = new Uri(new Uri(baseUrl.EnsureSuffix('/')), path);
        _timeout = config.Timeout;
        ProviderStreams.ThrowIfInvalid(config.StreamIdleTimeout, $"{nameof(OpenAIConfig)}.{nameof(OpenAIConfig.StreamIdleTimeout)}");
        _streamIdleTimeout = config.StreamIdleTimeout;
        _apiKey = string.IsNullOrWhiteSpace(config.ApiKey) ? null : config.ApiKey;
        // Read per request, as the OpenAI SDK path does — a key rotated in a secret store takes effect on the next call.
        _apiKeyResolver = config.ApiKeyResolver;
        if (sendAccountHeaders)
        {
            _organization = string.IsNullOrWhiteSpace(config.Organization) ? null : config.Organization;
            _project = string.IsNullOrWhiteSpace(config.Project) ? null : config.Project;
        }
    }

    public HttpClient Http { get; }

    /// <summary>
    /// A POST to the operation's endpoint carrying the credentials, the configured headers and then
    /// <paramref name="requestHeaders"/> — one request's own headers, which replace a configured one of the same name.
    /// A request header that names the credential is refused (<see cref="ProviderRequestHeaders.ResolveRequest"/>).
    /// </summary>
    public HttpRequestMessage CreatePost(HttpContent content, IDictionary<string, string>? requestHeaders = null)
    {
        var perRequest = ProviderRequestHeaders.ResolveRequest(
            nameof(OpenAIConfig), nameof(OpenAIConfig.ApiKey), _placement.ReservedHeaderNames, requestHeaders);
        var request = new HttpRequestMessage(HttpMethod.Post, _endpoint) { Content = content };
        var resolved = _apiKeyResolver?.Invoke();
        var apiKey = string.IsNullOrWhiteSpace(resolved) ? _apiKey : resolved;
        if (apiKey != null)
            request.Headers.TryAddWithoutValidation(_placement.Header, _placement.FormatValue(apiKey));
        if (_organization != null)
            request.Headers.Add("OpenAI-Organization", _organization);
        if (_project != null)
            request.Headers.Add("OpenAI-Project", _project);

        // A configured header replaces any default of the same name; the credential is never among them.
        Apply(request, _headers);
        Apply(request, perRequest);
        return request;
    }

    private static void Apply(HttpRequestMessage request, IReadOnlyDictionary<string, string>? headers)
    {
        if (headers is null)
            return;

        foreach (var (name, value) in headers)
        {
            request.Headers.Remove(name);
            request.Headers.TryAddWithoutValidation(name, value);
        }
    }

    /// <summary>
    /// A source that fires at <see cref="OpenAIConfig.Timeout"/> as well as on the caller's token, or null when the
    /// timeout is infinite. A cancellation the caller did not request is therefore the timeout.
    /// </summary>
    public CancellationTokenSource? CreateTimeoutSource(CancellationToken cancellationToken)
    {
        if (_timeout == System.Threading.Timeout.InfiniteTimeSpan)
            return null;
        var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        source.CancelAfter(_timeout);
        return source;
    }

    /// <summary><see cref="OpenAIConfig.StreamIdleTimeout"/>.</summary>
    public TimeSpan StreamIdleTimeout => _streamIdleTimeout;

    /// <summary>
    /// A source for one wait on a stream — the response to start, or the next line — that fires at
    /// <see cref="OpenAIConfig.StreamIdleTimeout"/> as well as on <paramref name="token"/>, or null when the budget is infinite.
    /// A fresh source per wait means an event that arrives never leaves a timer running into the next wait.
    /// </summary>
    public CancellationTokenSource? CreateStreamWaitSource(CancellationToken token)
    {
        if (_streamIdleTimeout == System.Threading.Timeout.InfiniteTimeSpan)
            return null;
        var source = CancellationTokenSource.CreateLinkedTokenSource(token);
        source.CancelAfter(_streamIdleTimeout);
        return source;
    }

    public void Dispose()
    {
        if (_ownsHttp)
            Http.Dispose();
    }
}

using System.Net.Http.Headers;
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
    private readonly string? _apiKey;
    private readonly string? _organization;
    private readonly string? _project;
    private readonly IReadOnlyDictionary<string, string>? _headers;

    /// <param name="config">The resolved configuration.</param>
    /// <param name="path">The operation's path relative to <see cref="OpenAIConfig.BaseUrl"/>.</param>
    /// <param name="defaultBaseUrl">Used when <see cref="OpenAIConfig.BaseUrl"/> is blank; null requires one.</param>
    /// <param name="sendAccountHeaders">Whether <c>OpenAI-Organization</c>/<c>OpenAI-Project</c> are sent.</param>
    public ProviderHttpClient(OpenAIConfig config, string path, string? defaultBaseUrl, bool sendAccountHeaders)
    {
        _headers = ProviderRequestHeaders.Resolve(nameof(OpenAIConfig), nameof(OpenAIConfig.ApiKey), ["Authorization"], config.Headers);
        _ownsHttp = config.HttpClient is null;
        Http = config.HttpClient ?? new HttpClient(new SocketsHttpHandler { ConnectTimeout = config.ConnectTimeout })
        {
            Timeout = System.Threading.Timeout.InfiniteTimeSpan,
        };

        var baseUrl = string.IsNullOrWhiteSpace(config.BaseUrl)
            ? defaultBaseUrl ?? throw new ArgumentException($"{nameof(OpenAIConfig)}.{nameof(OpenAIConfig.BaseUrl)} is required.", nameof(config))
            : config.BaseUrl;
        _endpoint = new Uri(new Uri(baseUrl.EnsureSuffix('/')), path);
        _timeout = config.Timeout;
        _apiKey = string.IsNullOrWhiteSpace(config.ApiKey) ? null : config.ApiKey;
        if (sendAccountHeaders)
        {
            _organization = string.IsNullOrWhiteSpace(config.Organization) ? null : config.Organization;
            _project = string.IsNullOrWhiteSpace(config.Project) ? null : config.Project;
        }
    }

    public HttpClient Http { get; }

    /// <summary>A POST to the operation's endpoint carrying the credentials and the configured headers.</summary>
    public HttpRequestMessage CreatePost(HttpContent content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _endpoint) { Content = content };
        if (_apiKey != null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        if (_organization != null)
            request.Headers.Add("OpenAI-Organization", _organization);
        if (_project != null)
            request.Headers.Add("OpenAI-Project", _project);

        // A configured header replaces any default of the same name; the credential is never among them.
        if (_headers is not null)
        {
            foreach (var (name, value) in _headers)
            {
                request.Headers.Remove(name);
                request.Headers.TryAddWithoutValidation(name, value);
            }
        }

        return request;
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

    public void Dispose()
    {
        if (_ownsHttp)
            Http.Dispose();
    }
}

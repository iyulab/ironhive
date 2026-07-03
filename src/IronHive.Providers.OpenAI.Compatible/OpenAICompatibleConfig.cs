using IronHive.Providers.OpenAI;

namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// Configuration for a generic OpenAI-compatible inference endpoint (Ollama, LM Studio, vLLM,
/// llama.cpp server, etc.) that exposes the standard <c>/v1</c> OpenAI API surface.
/// <para>
/// Unlike vendor-specific compatible providers (e.g. GPUStack's <c>/v1-openai/</c> path), this
/// targets the conventional <c>/v1</c> path and treats the API key as optional, matching LAN
/// services that accept unauthenticated requests.
/// </para>
/// </summary>
public class OpenAICompatibleConfig
{
    private const string DefaultBaseUrl = "http://localhost:11434"; // Ollama default; override for LM Studio (1234), vLLM (8000), etc.
    private const string DefaultPath = "/v1";

    /// <summary>
    /// API key for authentication. Optional — LAN services such as Ollama accept requests without a key.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Server base URL without the API path. Default: <c>http://localhost:11434</c> (Ollama).
    /// Set explicitly for other services (LM Studio <c>http://localhost:1234</c>, vLLM <c>http://localhost:8000</c>).
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// OpenAI-compatible API path appended to <see cref="BaseUrl"/>. Default: <c>/v1</c>.
    /// Appending is idempotent — if <see cref="BaseUrl"/> already ends with this path it is not duplicated.
    /// </summary>
    public string Path { get; set; } = DefaultPath;

    /// <summary>
    /// Optional resolver invoked per request to return the base URL dynamically.
    /// When null, <see cref="BaseUrl"/> is used.
    /// </summary>
    public Func<string>? BaseUrlResolver { get; set; }

    /// <summary>
    /// Optional resolver invoked per request to return the API key dynamically (key rotation).
    /// When null, the static <see cref="ApiKey"/> is used.
    /// </summary>
    public Func<string?>? ApiKeyResolver { get; set; }

    /// <summary>
    /// TCP connect timeout. (Default: 2s) On a LAN a healthy connection completes within tens of ms,
    /// so an unreachable host fails fast instead of stalling the fallback chain on the OS default (~21s).
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// True when a base URL is resolvable, i.e. the endpoint can be contacted. A key-optional LAN
    /// service is usable without a key (distinct from <see cref="IsConfigured"/>).
    /// </summary>
    public bool IsUsable => !string.IsNullOrWhiteSpace(ResolveBaseUrl());

    /// <summary>
    /// True when an API key is present. Distinct from <see cref="IsUsable"/>: a usable LAN endpoint
    /// may be unconfigured (no key) yet still serve requests.
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ResolveApiKey());

    /// <summary>Returns the effective base URL, preferring <see cref="BaseUrlResolver"/> when it yields a value.</summary>
    internal string ResolveBaseUrl()
    {
        if (BaseUrlResolver != null)
        {
            var resolved = BaseUrlResolver();
            if (!string.IsNullOrWhiteSpace(resolved))
                return resolved;
        }
        return string.IsNullOrEmpty(BaseUrl) ? DefaultBaseUrl : BaseUrl;
    }

    /// <summary>Returns the effective API key, preferring <see cref="ApiKeyResolver"/> when it yields a value.</summary>
    internal string ResolveApiKey()
    {
        if (ApiKeyResolver != null)
        {
            var resolved = ApiKeyResolver();
            if (!string.IsNullOrWhiteSpace(resolved))
                return resolved;
        }
        return ApiKey ?? string.Empty;
    }

    /// <summary>
    /// Converts this configuration to an equivalent <see cref="OpenAIConfig"/>, appending <see cref="Path"/>
    /// to the resolved base URL idempotently.
    /// </summary>
    public OpenAIConfig ToOpenAI()
    {
        var baseUrl = ResolveBaseUrl().TrimEnd('/');
        var path = (Path ?? string.Empty).Trim().Trim('/');
        var full = path.Length == 0 || baseUrl.EndsWith('/' + path, StringComparison.OrdinalIgnoreCase)
            ? baseUrl
            : baseUrl + '/' + path;

        return new OpenAIConfig
        {
            BaseUrl = full,
            ApiKey = ResolveApiKey(),
            HttpClient = new HttpClient(new SocketsHttpHandler
            {
                ConnectTimeout = ConnectTimeout
            }),
        };
    }
}

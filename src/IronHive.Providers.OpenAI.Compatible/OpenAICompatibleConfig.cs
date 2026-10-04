using IronHive.Abstractions.Http;
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
    /// Where and how the API key is sent — <see cref="OpenAIConfig.ApiKeyPlacement"/>, passed through to the
    /// configurations this one converts to. (Default: <see cref="CredentialPlacement.Bearer"/>.) For a gateway that
    /// expects <c>Basic</c>, a bare token, or the key in its own header (<c>api-key</c>); the key stays in the key slot,
    /// and <see cref="Headers"/> refuses <c>Authorization</c> and this placement's header.
    /// </summary>
    public CredentialPlacement ApiKeyPlacement { get; set; } = CredentialPlacement.Bearer;

    /// <summary>
    /// TCP connect timeout. (Default: 3s) On a LAN a healthy connection completes within tens of ms,
    /// so an unreachable host fails fast instead of stalling the fallback chain on the OS default (~21s).
    /// It stays above the ~2 s Windows takes to report a refused connection, so a server that is down is
    /// reported as refused rather than as a timeout.
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(3);

    /// <inheritdoc cref="OpenAIConfig.Timeout"/>
    public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    /// <inheritdoc cref="OpenAIConfig.StreamIdleTimeout"/>
    public TimeSpan StreamIdleTimeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    /// <summary>
    /// Extra request headers sent on every request — the uniform slot every IronHive provider config
    /// has. Passed through to the <see cref="OpenAIConfig"/> this configuration converts to, so the
    /// SDK path and the chat-completions path send the same set. <c>Authorization</c> is refused here
    /// and belongs to <see cref="ApiKey"/>.
    /// </summary>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// How many times a failed request is retried before it surfaces — <see cref="OpenAIConfig.MaxRetries"/>,
    /// passed through. Null keeps the SDK default (3); 0 sends one request.
    /// </summary>
    public int? MaxRetries { get; set; }

    /// <summary>
    /// Which output-length parameter to send. (Default: <c>max_completion_tokens</c>, the previous
    /// and only behaviour — existing configurations are unaffected.)
    /// </summary>
    /// <remarks>
    /// Set this to <see cref="Compatible.TokenLimitParameter.MaxTokens"/> for a server that predates
    /// OpenAI's rename. Such a server does not reject the newer name, it ignores it, so
    /// <c>MaxTokens</c> is dropped in silence and the only symptom is a response longer than asked
    /// for. There is no name that is accepted everywhere — current OpenAI models reject the old one —
    /// which is why this is a setting rather than something the package infers.
    /// </remarks>
    public TokenLimitParameter TokenLimitParameter { get; set; } = TokenLimitParameter.MaxCompletionTokens;

    /// <summary>
    /// Whether an image a tool returns is carried to the model in a user message that follows the tool results.
    /// (Default: <see langword="false"/> — the image is replaced by a note in the tool message, as before.)
    /// </summary>
    /// <remarks>
    /// A Chat Completions <c>tool</c> message holds text only, and OpenAI-compatible servers refuse an image part there
    /// (vLLM, llama.cpp). Turned on, the tool message keeps its text and names each image
    /// (<c>[image image/png — attached in the next message]</c>), and one <c>user</c> message after the round's tool
    /// messages carries the images as <c>image_url</c> parts, each introduced by the tool call it came from. For a
    /// vision model behind such a server this is the only way it sees a tool's image. Off by default because it adds
    /// a message the conversation did not contain.
    /// </remarks>
    public bool CarryImageToolResultsAsUserMessage { get; set; }

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
            Headers = Headers,
            ApiKeyPlacement = ApiKeyPlacement,
            // No HttpClient here: the client that receives this config creates and owns one with this connect
            // timeout and no client-level timeout. Handing it one of ours would make it the consumer's — never disposed.
            ConnectTimeout = ConnectTimeout,
            Timeout = Timeout,
            StreamIdleTimeout = StreamIdleTimeout,
            MaxRetries = MaxRetries,
        };
    }
}

using Google.Apis.Auth.OAuth2;
using Google.GenAI;
using Google.GenAI.Types;

namespace IronHive.Providers.GoogleAI;

internal static class GoogleAIClientFactory
{
    /// <summary>
    /// The arguments the vendor client is constructed from. The constructed <see cref="Client"/> exposes
    /// none of them, so the configuration-dependent mapping is built here and asserted, rather than
    /// inferred from a successful construction: a field routed to the wrong parameter compiles, and for a
    /// credential the resulting error names neither the field nor the factory.
    /// </summary>
    internal readonly record struct ClientArguments(
        bool VertexAI,
        string? ApiKey,
        ICredential? Credential,
        string? Project,
        string? Location,
        HttpOptions HttpOptions,
        ClientOptions? ClientOptions);

    /// <summary>
    /// Gemini authenticates with an API key; the project and location belong to Vertex and stay unset.
    /// </summary>
    internal static ClientArguments BuildArguments(GoogleAIConfig config) => new(
        VertexAI: false,
        ApiKey: config.ApiKey,
        Credential: null,
        Project: null,
        Location: null,
        HttpOptions: ResolveHttpOptions(config.HttpOptions, config.Timeout, nameof(GoogleAIConfig)),
        ClientOptions: ResolveClientOptions(config.HttpClientFactory));

    /// <summary>
    /// Vertex addresses a project and location and authenticates with a credential rather than a key —
    /// given none, the vendor falls back to Application Default Credentials.
    /// </summary>
    internal static ClientArguments BuildArguments(VertexAIConfig config) => new(
        VertexAI: true,
        ApiKey: null,
        Credential: config.Credential,
        Project: config.Project,
        Location: config.Location,
        HttpOptions: ResolveHttpOptions(config.HttpOptions, config.Timeout, nameof(VertexAIConfig)),
        ClientOptions: ResolveClientOptions(config.HttpClientFactory));

    private static ClientOptions? ResolveClientOptions(Func<HttpClient>? httpClientFactory)
        => httpClientFactory is null ? null : new ClientOptions { HttpClientFactory = httpClientFactory };

    internal static Client Create(GoogleAIConfig config) => Create(BuildArguments(config));

    internal static Client Create(VertexAIConfig config) => Create(BuildArguments(config));

    // The single place arguments reach the vendor constructor, so the mapping asserted above is the
    // mapping used. Nothing configuration-dependent happens here.
    private static Client Create(ClientArguments arguments) => new(
        vertexAI: arguments.VertexAI,
        apiKey: arguments.ApiKey,
        credential: arguments.Credential,
        project: arguments.Project,
        location: arguments.Location,
        httpOptions: arguments.HttpOptions,
        clientOptions: arguments.ClientOptions);

    /// <summary>
    /// Folds the configuration's timeout into the vendor <see cref="HttpOptions"/>, which is where the
    /// SDK reads it from. Two things are deliberate here. The vendor default when nothing is supplied
    /// is a bare <see cref="System.Net.Http.HttpClient"/>'s 100 seconds, which bounds a whole
    /// non-streaming call and the wait for a streaming call's first byte — so the adapter always
    /// supplies a value rather than letting that be inherited silently. And a configuration that sets
    /// the timeout twice, in different units, is rejected rather than resolved by precedence: a
    /// setting that loses silently is the same class of defect as one that never reaches the client.
    /// </summary>
    internal static HttpOptions ResolveHttpOptions(HttpOptions? options, TimeSpan? timeout, string configName)
    {
        if (timeout.HasValue && options?.Timeout != null)
        {
            throw new InvalidOperationException(
                $"{configName}.Timeout and {configName}.HttpOptions.Timeout are both set. " +
                $"Set only one — use {configName}.Timeout unless the vendor options are needed for something else.");
        }

        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero)
        {
            throw new InvalidOperationException($"{configName}.Timeout must be greater than zero.");
        }

        var effective = timeout ?? (options?.Timeout is null ? GoogleAIDefaults.Timeout : null);
        if (effective is null)
            return options!;

        var milliseconds = (int)Math.Min(effective.Value.TotalMilliseconds, int.MaxValue);
        return options is null
            ? new HttpOptions { Timeout = milliseconds }
            : options with { Timeout = milliseconds };
    }
}

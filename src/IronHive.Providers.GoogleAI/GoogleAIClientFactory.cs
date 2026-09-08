using Google.GenAI;
using Google.GenAI.Types;

namespace IronHive.Providers.GoogleAI;

internal static class GoogleAIClientFactory
{
    internal static Client Create(GoogleAIConfig config)
    {
        return new Client(
            vertexAI: false,
            apiKey: config.ApiKey,
            httpOptions: ResolveHttpOptions(config.HttpOptions, config.Timeout, nameof(GoogleAIConfig)),
            clientOptions: new ClientOptions
            {
                HttpClientFactory = ResolveHttpClientFactory(config.HttpClientFactory, config.ConnectTimeout)
            });
    }

    internal static Client Create(VertexAIConfig config)
    {
        return new Client(
            vertexAI: true,
            credential: config.Credential,
            project: config.Project,
            location: config.Location,
            httpOptions: ResolveHttpOptions(config.HttpOptions, config.Timeout, nameof(VertexAIConfig)),
            clientOptions: new ClientOptions
            {
                HttpClientFactory = ResolveHttpClientFactory(config.HttpClientFactory, config.ConnectTimeout)
            });
    }

    /// <summary>
    /// Resolves the <see cref="HttpClient"/> factory the vendor client uses. When the consumer supplies
    /// one, it is used as-is — connect timeout and request timeout become that factory's responsibility.
    /// Otherwise this builds a client whose connect budget is bounded by <paramref name="connectTimeout"/>
    /// and whose <see cref="HttpClient.Timeout"/> is disabled, so the vendor's own bare-<see cref="HttpClient"/>
    /// default (100 seconds, capping time-to-first-byte) is never inherited silently.
    /// </summary>
    internal static Func<HttpClient> ResolveHttpClientFactory(Func<HttpClient>? factory, TimeSpan connectTimeout)
    {
        if (factory != null)
            return factory;

        return () => new HttpClient(new SocketsHttpHandler { ConnectTimeout = connectTimeout })
        {
            Timeout = System.Threading.Timeout.InfiniteTimeSpan
        };
    }

    /// <summary>
    /// Folds the configuration's timeout into the vendor <see cref="HttpOptions"/>, which is where the
    /// SDK reads it from. <paramref name="timeout"/> at its default
    /// (<see cref="System.Threading.Timeout.InfiniteTimeSpan"/>) is treated as "not set" rather than a
    /// concrete value to send — the client built by <see cref="ResolveHttpClientFactory"/> already has
    /// an unbounded <see cref="HttpClient.Timeout"/>, so leaving it alone means "no request ceiling",
    /// not an accidental inheritance of the vendor's bare-<see cref="HttpClient"/> 100-second default.
    /// And a configuration that sets the timeout twice, in different units, is rejected rather than
    /// resolved by precedence: a setting that loses silently is the same class of defect as one that
    /// never reaches the client.
    /// </summary>
    internal static HttpOptions ResolveHttpOptions(HttpOptions? options, TimeSpan timeout, string configName)
    {
        var isSet = timeout != System.Threading.Timeout.InfiniteTimeSpan;

        if (isSet && options?.Timeout != null)
        {
            throw new InvalidOperationException(
                $"{configName}.Timeout and {configName}.HttpOptions.Timeout are both set. " +
                $"Set only one — use {configName}.Timeout unless the vendor options are needed for something else.");
        }

        if (isSet && timeout <= TimeSpan.Zero)
        {
            throw new InvalidOperationException($"{configName}.Timeout must be greater than zero.");
        }

        if (!isSet)
            return options ?? new HttpOptions();

        var milliseconds = (int)Math.Min(timeout.TotalMilliseconds, int.MaxValue);
        return options is null
            ? new HttpOptions { Timeout = milliseconds }
            : options with { Timeout = milliseconds };
    }
}

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
            clientOptions: config.HttpClientFactory != null ? new ClientOptions
            {
                HttpClientFactory = config.HttpClientFactory
            } : null);
    }

    internal static Client Create(VertexAIConfig config)
    {
        return new Client(
            vertexAI: true,
            credential: config.Credential,
            project: config.Project,
            location: config.Location,
            httpOptions: ResolveHttpOptions(config.HttpOptions, config.Timeout, nameof(VertexAIConfig)),
            clientOptions: config.HttpClientFactory != null ? new ClientOptions
            {
                HttpClientFactory = config.HttpClientFactory
            } : null);
    }

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

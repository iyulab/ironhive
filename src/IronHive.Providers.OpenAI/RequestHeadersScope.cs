namespace IronHive.Providers.OpenAI;

/// <summary>
/// The headers one request asked for (<c>MessageGenerationRequest.Headers</c>), carried from the generator to
/// <see cref="ExtraRequestHeadersPolicy"/>. The OpenAI SDK's typed calls take no per-call request options, so the value
/// travels with the async flow: the generator opens a scope before it calls the SDK, and the policy — running inside
/// that call, on the request the SDK assembled — applies it. A streaming call sends its request on the first read, which
/// happens inside the generator's own iteration, so the scope covers it there as well.
/// </summary>
internal static class RequestHeadersScope
{
    private static readonly AsyncLocal<IReadOnlyDictionary<string, string>?> s_current = new();

    /// <summary>The headers of the request being sent on this async flow, or null outside a scope.</summary>
    public static IReadOnlyDictionary<string, string>? Current => s_current.Value;

    /// <summary>Makes <paramref name="headers"/> current until the returned scope is disposed.</summary>
    public static Scope Begin(IReadOnlyDictionary<string, string>? headers)
    {
        var previous = s_current.Value;
        s_current.Value = headers;
        return new Scope(previous);
    }

    public readonly struct Scope(IReadOnlyDictionary<string, string>? previous) : IDisposable
    {
        public void Dispose() => s_current.Value = previous;
    }
}

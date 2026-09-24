namespace IronHive.Abstractions.Http;

/// <summary>
/// Writes a provider's credential header on every outgoing request from a resolver, so a key kept in a secret
/// store (and rotated or revoked there) takes effect on the next call without rebuilding the provider.
/// </summary>
/// <remarks>
/// It sits in the <see cref="HttpClient"/> the vendor SDK sends through, below the SDK's own credential handling,
/// and replaces whatever value the SDK put in <paramref name="headerName"/> — the SDK was constructed with the
/// resolver's first answer (or a placeholder), which must never be the one that reaches the wire after the key changes.
/// A resolver that returns nothing falls back to <paramref name="fallback"/>, the config's static key; when that is
/// empty too the request goes out without the header rather than with a stale key.
/// </remarks>
/// <param name="headerName">The provider's credential header (<c>Authorization</c>, <c>x-api-key</c>, <c>x-goog-api-key</c>).</param>
/// <param name="resolve">The config's key resolver, called once per request.</param>
/// <param name="fallback">The config's static key, used when the resolver returns null or blank.</param>
/// <param name="format">Turns the key into the header value (e.g. <c>Bearer {key}</c>); the key as-is when null.</param>
/// <param name="inner">The handler that sends the request.</param>
public sealed class ResolvedCredentialHandler(
    string headerName,
    Func<string?> resolve,
    string? fallback,
    Func<string, string>? format,
    HttpMessageHandler inner) : DelegatingHandler(inner)
{
    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var resolved = resolve();
        var key = string.IsNullOrWhiteSpace(resolved) ? fallback : resolved;

        request.Headers.Remove(headerName);
        if (!string.IsNullOrWhiteSpace(key))
            request.Headers.TryAddWithoutValidation(headerName, format is null ? key : format(key));

        return base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// The configuration error for a resolver that cannot reach the wire: the consumer supplied its own
    /// <see cref="HttpClient"/>, so there is no transport to put this handler in. Refused rather than ignored —
    /// a resolver that is silently skipped sends the static key forever.
    /// </summary>
    public static InvalidOperationException ConflictsWithCustomHttpClient(string configName, string httpClientMember) =>
        new($"{configName}.ApiKeyResolver cannot be combined with {configName}.{httpClientMember}: the resolved key is " +
            $"written by a handler in the HTTP client IronHive builds. Either drop {httpClientMember}, or put your own " +
            "handler that sets the credential header into the client you supply and leave ApiKeyResolver unset.");
}

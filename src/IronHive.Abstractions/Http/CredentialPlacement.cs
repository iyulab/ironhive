namespace IronHive.Abstractions.Http;

/// <summary>
/// Where a provider config's API key goes on the wire and how it is written there: a header name and an optional
/// scheme word in front of the key. The key itself stays in the config's key slot — this only says how that slot
/// is sent, so a gateway that expects something other than <c>Authorization: Bearer &lt;key&gt;</c> is configured
/// without moving the credential into <c>Headers</c>.
/// </summary>
/// <remarks>
/// The key is sent as given. A <c>Basic</c> gateway takes the already-encoded <c>user:password</c> as the key.
/// </remarks>
public sealed record CredentialPlacement
{
    /// <summary>The header every OpenAI-wire credential uses unless a placement says otherwise.</summary>
    public const string AuthorizationHeader = "Authorization";

    /// <summary>
    /// Creates a placement.
    /// </summary>
    /// <param name="header">The request header that carries the key.</param>
    /// <param name="scheme">The word written before the key (<c>Bearer</c>, <c>Basic</c>), or <see langword="null"/> to send the key alone.</param>
    /// <exception cref="ArgumentException">The header name is blank or not a valid token, or the scheme is blank or contains whitespace.</exception>
    public CredentialPlacement(string header, string? scheme)
    {
        if (string.IsNullOrWhiteSpace(header) || header.Any(c => char.IsWhiteSpace(c) || c == ':' || char.IsControl(c)))
            throw new ArgumentException($"'{header}' is not a header name a credential can be sent in.", nameof(header));
        if (scheme is not null && (scheme.Length == 0 || scheme.Any(c => char.IsWhiteSpace(c) || char.IsControl(c))))
            throw new ArgumentException(
                $"'{scheme}' is not a credential scheme: a scheme is one word (such as Bearer or Basic). Use null to send the key without one.",
                nameof(scheme));

        Header = header.Trim();
        Scheme = scheme;
    }

    /// <summary>The request header that carries the key.</summary>
    public string Header { get; }

    /// <summary>The word written before the key, or <see langword="null"/> when the key is sent alone.</summary>
    public string? Scheme { get; }

    /// <summary><c>Authorization: Bearer &lt;key&gt;</c> — the OpenAI wire's own form and the default.</summary>
    public static CredentialPlacement Bearer { get; } = new(AuthorizationHeader, "Bearer");

    /// <summary><c>Authorization: &lt;scheme&gt; &lt;key&gt;</c>, or <c>Authorization: &lt;key&gt;</c> when <paramref name="scheme"/> is <see langword="null"/>.</summary>
    public static CredentialPlacement Authorization(string? scheme) => new(AuthorizationHeader, scheme);

    /// <summary>The key alone in a header of the gateway's own, such as <c>api-key</c>.</summary>
    public static CredentialPlacement InHeader(string header) => new(header, null);

    /// <summary>Whether the key goes in <c>Authorization</c>.</summary>
    public bool UsesAuthorizationHeader => string.Equals(Header, AuthorizationHeader, StringComparison.OrdinalIgnoreCase);

    /// <summary>The header value for <paramref name="key"/>.</summary>
    public string FormatValue(string key) => Scheme is null ? key : $"{Scheme} {key}";

    /// <summary>
    /// The header names a configured <c>Headers</c> entry may not use: <c>Authorization</c> always (the wire's own
    /// credential header), and this placement's header.
    /// </summary>
    public IReadOnlyList<string> ReservedHeaderNames =>
        UsesAuthorizationHeader ? [AuthorizationHeader] : [AuthorizationHeader, Header];

    /// <inheritdoc/>
    public bool Equals(CredentialPlacement? other) =>
        other is not null
        && string.Equals(Header, other.Header, StringComparison.OrdinalIgnoreCase)
        && string.Equals(Scheme, other.Scheme, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(Header), Scheme);

    /// <inheritdoc/>
    public override string ToString() => Scheme is null ? $"{Header}: <key>" : $"{Header}: {Scheme} <key>";
}

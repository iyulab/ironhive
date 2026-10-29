namespace IronHive.Providers.Declarations;

/// <summary>
/// A provider as data: where a server is, which wire format it speaks, how its key is sent, and optionally which models
/// differ. A host stores these (JSON, YAML, a database row), lets a person add any number of them, and hands each to
/// <see cref="ProviderClientFactory"/>. Storage, UI and which declaration wins when sources collide stay with the host.
/// </summary>
/// <remarks>
/// <para>
/// Plain settable properties on purpose: a declaration is read from a file or a form, and every setting is checked
/// together by <see cref="ProviderClientFactory"/> when a client is created (an <see cref="ArgumentException"/> names the
/// declaration and the setting).
/// </para>
/// <para>
/// The key is a plain string here. A host that keeps keys in a secret store resolves the key into <see cref="ApiKey"/>
/// when it builds the declaration for a call, or builds the provider's own config (<c>OpenAICompatibleConfig</c> and the
/// like, which take a key resolver) when it needs rotation.
/// </para>
/// </remarks>
public sealed record ProviderDeclaration
{
    /// <summary>A name the host chooses for this provider (<c>"local-ollama"</c>, <c>"corp-gateway"</c>). Reported as the provider name of the clients it creates.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The wire format the server speaks.</summary>
    public WireFormat Wire { get; init; }

    /// <summary>
    /// The endpoint requests are sent under, as the server's documentation gives it - **including** the version segment
    /// where the wire has one: <c>https://gateway.example.com/v1</c> for Chat Completions and Responses,
    /// <c>https://api.anthropic.com</c> for Anthropic, <c>https://generativelanguage.googleapis.com</c> for Gemini.
    /// A trailing slash is ignored. An absolute <c>http</c> or <c>https</c> URL.
    /// </summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// The API key, or <see langword="null"/> for a server that takes none (a LAN Ollama). The declaration is the whole credential
    /// decision: no key here means no credential on the wire, whatever the vendor SDKs would otherwise read from the machine's
    /// environment (<c>ANTHROPIC_API_KEY</c>, <c>GOOGLE_API_KEY</c>, ...).
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// The request header that carries <see cref="ApiKey"/>, or <see langword="null"/> for the wire's own
    /// (<c>Authorization: Bearer</c> for the OpenAI wires and for Anthropic, whose vendor SDK writes it so; <c>x-goog-api-key</c> for Gemini).
    /// A gateway's own header works on every wire; on Gemini the key is then also sent in <c>x-goog-api-key</c>, which the vendor SDK always writes.
    /// <c>Authorization</c> in any form but Bearer, and <c>x-api-key</c>, cannot be sent on the Anthropic wire, and <c>Authorization</c> cannot on Gemini - the factory refuses them.
    /// </summary>
    public string? KeyHeader { get; init; }

    /// <summary>
    /// The word written before the key in <see cref="KeyHeader"/>: <see langword="null"/> means the default for the header
    /// (<c>Bearer</c> in <c>Authorization</c>, nothing in any other header), an empty string means the key alone,
    /// anything else (<c>Basic</c>, <c>Token</c>) is that word.
    /// </summary>
    public string? KeyScheme { get; init; }

    /// <summary>Extra headers sent on every request (a gateway's routing or subscription header). The credential header is refused here.</summary>
    public IDictionary<string, string>? Headers { get; init; }

    /// <summary>
    /// Per-server behaviour - timeouts, retries, and the layouts and spellings a particular server needs - for all models of this provider
    /// (<see langword="null"/> keeps every adapter default). A model's own <see cref="ModelDeclaration.Settings"/> override these per property.
    /// </summary>
    public ServerSettings? Settings { get; init; }

    /// <summary>Models of this provider that differ from the rest, by model id.</summary>
    public IDictionary<string, ModelDeclaration>? Models { get; init; }
}

/// <summary>What differs for one model of a <see cref="ProviderDeclaration"/>: a server that serves some models on another wire or under another endpoint.</summary>
public sealed record ModelDeclaration
{
    /// <summary>The wire format this model is reached over, or <see langword="null"/> for the provider's.</summary>
    public WireFormat? Wire { get; init; }

    /// <summary>The endpoint this model is reached under (same form as <see cref="ProviderDeclaration.BaseUrl"/>), or <see langword="null"/> for the provider's.</summary>
    public string? BaseUrl { get; init; }

    /// <summary>Settings that differ for this model; each one set here replaces the provider's (<see cref="ProviderDeclaration.Settings"/>), the rest fall through.</summary>
    public ServerSettings? Settings { get; init; }
}

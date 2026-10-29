using IronHive.Abstractions.Http;
using IronHive.Abstractions.Messages;
using IronHive.Extensions.AI;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;
using Microsoft.Extensions.AI;

namespace IronHive.Providers.Declarations;

/// <summary>
/// Turns a <see cref="ProviderDeclaration"/> into a client: the one place that knows which adapter a wire format
/// needs, how its base URL is written, and where each wire lets a key go.
/// </summary>
public static class ProviderClientFactory
{
    private const string AnthropicVendorKeyHeader = "x-api-key";
    private const string GoogleKeyHeader = "x-goog-api-key";

    /// <summary>
    /// Creates the <see cref="IChatClient"/> for <paramref name="modelId"/> of <paramref name="declaration"/>. The client
    /// reports <see cref="ProviderDeclaration.Id"/> as its provider name and owns the generator behind it.
    /// </summary>
    /// <exception cref="ArgumentException">A setting of the declaration is blank, malformed, or not something its wire can carry; the message names the setting.</exception>
    public static IChatClient Create(ProviderDeclaration declaration, string modelId)
    {
        var generator = CreateGenerator(declaration, modelId);
        return generator.AsChatClient(modelId, declaration.Id);
    }

    /// <summary>
    /// Creates the <see cref="IMessageGenerator"/> for <paramref name="modelId"/> of <paramref name="declaration"/> - for a
    /// host that drives IronHive's own message surface rather than <see cref="IChatClient"/>. The caller disposes it.
    /// </summary>
    /// <exception cref="ArgumentException">A setting of the declaration is blank, malformed, or not something its wire can carry; the message names the setting.</exception>
    public static IMessageGenerator CreateGenerator(ProviderDeclaration declaration, string modelId)
    {
        ArgumentNullException.ThrowIfNull(declaration);
        if (string.IsNullOrWhiteSpace(modelId))
            throw new ArgumentException("The model id is blank.", nameof(modelId));
        if (string.IsNullOrWhiteSpace(declaration.Id))
            throw new ArgumentException($"{nameof(ProviderDeclaration)}.{nameof(ProviderDeclaration.Id)} is blank.", nameof(declaration));

        var who = $"Provider '{declaration.Id}'";
        ModelDeclaration? model = null;
        declaration.Models?.TryGetValue(modelId, out model);

        var wire = model?.Wire ?? declaration.Wire;
        if (!Enum.IsDefined(wire))
            throw new ArgumentException($"{who}: the wire format {(int)wire} is not one of {string.Join(", ", Enum.GetNames<WireFormat>())}.", nameof(declaration));
        var baseUrl = NormalizeBaseUrl(model?.BaseUrl ?? declaration.BaseUrl, who, model?.BaseUrl is null ? nameof(ProviderDeclaration.BaseUrl) : $"Models['{modelId}'].BaseUrl");
        var settings = ResolveSettings(declaration, model, modelId, wire, who);
        var placement = ResolvePlacement(declaration, who);
        var key = string.IsNullOrWhiteSpace(declaration.ApiKey) ? null : declaration.ApiKey;
        var headers = declaration.Headers is { Count: > 0 } declared ? new Dictionary<string, string>(declared, StringComparer.OrdinalIgnoreCase) : null;

        return wire switch
        {
            WireFormat.OpenAIChatCompletions => CreateChatCompletions(baseUrl, key, placement, headers, settings),
            WireFormat.OpenAIResponses => CreateResponses(baseUrl, key, placement, headers, settings),
            WireFormat.Anthropic => CreateAnthropic(baseUrl, key, placement, headers, settings, who),
            WireFormat.GoogleAI => CreateGoogle(baseUrl, key, placement, headers, settings, who),
            _ => throw new ArgumentException($"{who}: the wire format {wire} has no adapter.", nameof(declaration)),
        };
    }

    private static OpenAICompatibleMessageGenerator CreateChatCompletions(
        string baseUrl, string? key, CredentialPlacement? placement, Dictionary<string, string>? headers, ResolvedSettings settings)
    {
        var config = new OpenAICompatibleConfig
        {
            BaseUrl = baseUrl,
            // The declared base URL is the whole endpoint, version segment included; the adapter's own "/v1" is not appended.
            Path = string.Empty,
            ApiKey = key,
            ApiKeyPlacement = placement ?? CredentialPlacement.Bearer,
            Headers = headers,
        };
        // Only what the declaration states is set: the adapter's own defaults stay the defaults.
        if (settings.ConnectTimeout is { } connect) config.ConnectTimeout = connect;
        if (settings.Timeout is { } timeout) config.Timeout = timeout;
        if (settings.StreamIdleTimeout is { } idle) config.StreamIdleTimeout = idle;
        if (settings.InstructionsPlacement is { } instructions) config.InstructionsPlacement = instructions;
        if (settings.TokenLimitParameter is { } tokenLimit) config.TokenLimitParameter = tokenLimit;
        if (settings.CarryImageToolResultsAsUserMessage is { } carry) config.CarryImageToolResultsAsUserMessage = carry;
        return new OpenAICompatibleMessageGenerator(config);
    }

    private static OpenAIMessageGenerator CreateResponses(
        string baseUrl, string? key, CredentialPlacement? placement, Dictionary<string, string>? headers, ResolvedSettings settings)
    {
        var config = new OpenAIConfig
        {
            BaseUrl = baseUrl,
            ApiKey = key ?? string.Empty,
            ApiKeyPlacement = placement ?? CredentialPlacement.Bearer,
            Headers = headers,
            MaxRetries = settings.MaxRetries,
        };
        if (settings.ConnectTimeout is { } connect) config.ConnectTimeout = connect;
        if (settings.Timeout is { } timeout) config.Timeout = timeout;
        if (settings.StreamIdleTimeout is { } idle) config.StreamIdleTimeout = idle;
        return new OpenAIMessageGenerator(config);
    }

    // Anthropic: the vendor SDK (13.x) owns the credential slot and writes the key as "Authorization: Bearer <key>" - that is
    // the wire's own placement, so it needs no more than ApiKey. The extra-headers slot refuses Authorization and x-api-key,
    // so those two cannot carry a key any other way: a gateway's own header does, anything else is refused.
    private static AnthropicMessageGenerator CreateAnthropic(string baseUrl, string? key, CredentialPlacement? placement, Dictionary<string, string>? headers, ResolvedSettings settings, string who)
    {
        // The declaration is the whole credential decision: a vendor SDK left with an empty credential slot fills it from the
        // machine (ANTHROPIC_API_KEY, the Anthropic profile) and would send a person's own key to the declared address. The
        // resolver that answers nothing keeps the SDK from looking and keeps the header off the wire.
        var config = new AnthropicConfig
        {
            BaseUrl = baseUrl,
            Headers = headers,
            ApiKeyResolver = NoVendorCredential,
            MaxRetries = settings.MaxRetries,
        };
        if (settings.ConnectTimeout is { } connect) config.ConnectTimeout = connect;
        if (settings.Timeout is { } timeout) config.Timeout = timeout;
        if (settings.StreamIdleTimeout is { } idle) config.StreamIdleTimeout = idle;
        if (key is null)
            return new AnthropicMessageGenerator(config);

        if (placement is null || (placement.UsesAuthorizationHeader && string.Equals(placement.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)))
        {
            config.ApiKey = key;
            config.ApiKeyResolver = null;
        }
        else if (placement.UsesAuthorizationHeader || SameHeader(placement.Header, AnthropicVendorKeyHeader))
            throw new ArgumentException(
                $"{who}: the key cannot be sent as '{placement}' on the Anthropic wire; the vendor SDK sends it as 'Authorization: Bearer'. " +
                "Leave KeyHeader unset for that, or name a header of the gateway's own.");
        else
            config.Headers = WithKeyHeader(headers, placement, key, who);

        return new AnthropicMessageGenerator(config);
    }

    // Gemini: the key is the SDK's ApiKey (x-goog-api-key), and the SDK will not build a client without one. A gateway's own
    // header therefore carries the key in addition to x-goog-api-key, not instead of it. Authorization is refused by the
    // extra-headers slot, so it is not a placement this wire can send.
    private static GoogleAIMessageGenerator CreateGoogle(string baseUrl, string? key, CredentialPlacement? placement, Dictionary<string, string>? headers, ResolvedSettings settings, string who)
    {
        var config = new GoogleAIConfig
        {
            HttpOptions = new Google.GenAI.Types.HttpOptions { BaseUrl = baseUrl },
            Headers = headers,
            ApiKey = key,
            // No declared key: the SDK would read GOOGLE_API_KEY / GEMINI_API_KEY and send the machine's key to the declared address.
            ApiKeyResolver = key is null ? NoVendorCredential : null,
        };
        if (settings.ConnectTimeout is { } connect) config.ConnectTimeout = connect;
        if (settings.Timeout is { } timeout) config.Timeout = timeout;
        if (settings.StreamIdleTimeout is { } idle) config.StreamIdleTimeout = idle;
        if (key is not null && placement is not null && !(SameHeader(placement.Header, GoogleKeyHeader) && placement.Scheme is null))
            config.Headers = WithKeyHeader(headers, placement, key, who);

        return new GoogleAIMessageGenerator(config);
    }

    private static string? NoVendorCredential() => null;

    // The model's value replaces the provider's per property; a setting the effective wire has no home for is refused by name,
    // like an unsendable key header, rather than dropped.
    private static ResolvedSettings ResolveSettings(ProviderDeclaration declaration, ModelDeclaration? model, string modelId, WireFormat wire, string who)
    {
        var provider = declaration.Settings;
        var forModel = model?.Settings;

        T? Pick<T>(Func<ServerSettings, T?> read, string name, out string path) where T : struct
        {
            var fromModel = forModel is null ? null : read(forModel);
            if (fromModel is not null)
            {
                path = $"Models['{modelId}'].Settings.{name}";
                return fromModel;
            }

            path = $"Settings.{name}";
            return provider is null ? null : read(provider);
        }

        var connect = Pick(s => s.ConnectTimeout, nameof(ServerSettings.ConnectTimeout), out var connectPath);
        var timeout = Pick(s => s.Timeout, nameof(ServerSettings.Timeout), out var timeoutPath);
        var idle = Pick(s => s.StreamIdleTimeout, nameof(ServerSettings.StreamIdleTimeout), out var idlePath);
        var retries = Pick(s => s.MaxRetries, nameof(ServerSettings.MaxRetries), out var retriesPath);
        var placement = Pick(s => s.InstructionsPlacement, nameof(ServerSettings.InstructionsPlacement), out var placementPath);
        var tokens = Pick(s => s.TokenLimitParameter, nameof(ServerSettings.TokenLimitParameter), out var tokensPath);
        var carry = Pick(s => s.CarryImageToolResultsAsUserMessage, nameof(ServerSettings.CarryImageToolResultsAsUserMessage), out var carryPath);

        void Positive(TimeSpan? value, string path)
        {
            if (value is { } span && span <= TimeSpan.Zero)
                throw new ArgumentException($"{who}: {path} must be greater than zero.");
        }

        Positive(connect, connectPath);
        Positive(timeout, timeoutPath);
        Positive(idle, idlePath);
        if (retries is < 0)
            throw new ArgumentException($"{who}: {retriesPath} must not be negative.");
        if (placement is { } p && !Enum.IsDefined(p))
            throw new ArgumentException($"{who}: {placementPath} {(int)p} is not one of {string.Join(", ", Enum.GetNames<InstructionsPlacement>())}.");
        if (tokens is { } t && !Enum.IsDefined(t))
            throw new ArgumentException($"{who}: {tokensPath} {(int)t} is not one of {string.Join(", ", Enum.GetNames<TokenLimitParameter>())}.");

        void OnlyOn(string path, bool set, bool supported, string where)
        {
            if (set && !supported)
                throw new ArgumentException($"{who}: {path} does not apply on the {wire} wire (it applies to {where}).");
        }

        var chatCompletions = wire == WireFormat.OpenAIChatCompletions;
        OnlyOn(retriesPath, retries is not null, wire is WireFormat.OpenAIResponses or WireFormat.Anthropic, "OpenAIResponses and Anthropic");
        OnlyOn(placementPath, placement is not null, chatCompletions, nameof(WireFormat.OpenAIChatCompletions));
        OnlyOn(tokensPath, tokens is not null, chatCompletions, nameof(WireFormat.OpenAIChatCompletions));
        OnlyOn(carryPath, carry is not null, chatCompletions, nameof(WireFormat.OpenAIChatCompletions));

        return new ResolvedSettings(connect, timeout, idle, retries, placement, tokens, carry);
    }

    private sealed record ResolvedSettings(
        TimeSpan? ConnectTimeout, TimeSpan? Timeout, TimeSpan? StreamIdleTimeout, int? MaxRetries,
        InstructionsPlacement? InstructionsPlacement, TokenLimitParameter? TokenLimitParameter, bool? CarryImageToolResultsAsUserMessage);

    private static Dictionary<string, string> WithKeyHeader(Dictionary<string, string>? headers, CredentialPlacement placement, string key, string who)
    {
        if (placement.UsesAuthorizationHeader)
            throw new ArgumentException(
                $"{who}: the key cannot be sent as '{placement}' on this wire; name a header of the gateway's own.");
        var result = headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!result.TryAdd(placement.Header, placement.FormatValue(key)))
            throw new ArgumentException(
                $"{who}: '{placement.Header}' is the key header and also a declared header; remove it from {nameof(ProviderDeclaration.Headers)}.");
        return result;
    }

    private static CredentialPlacement? ResolvePlacement(ProviderDeclaration declaration, string who)
    {
        if (string.IsNullOrWhiteSpace(declaration.KeyHeader) && declaration.KeyScheme is null)
            return null;

        var header = string.IsNullOrWhiteSpace(declaration.KeyHeader) ? CredentialPlacement.AuthorizationHeader : declaration.KeyHeader;
        var isAuthorization = string.Equals(header.Trim(), CredentialPlacement.AuthorizationHeader, StringComparison.OrdinalIgnoreCase);
        var scheme = declaration.KeyScheme switch
        {
            null => isAuthorization ? "Bearer" : null,
            "" => null,
            var word => word,
        };
        try
        {
            return new CredentialPlacement(header, scheme);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"{who}: {ex.Message}", nameof(declaration), ex);
        }
    }

    private static string NormalizeBaseUrl(string? value, string who, string setting)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{who}: {setting} is blank.");
        var trimmed = value.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException($"{who}: {setting} '{trimmed}' is not an absolute http or https URL.");
        if (!string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new ArgumentException($"{who}: {setting} '{trimmed}' has a query or fragment; the endpoint is a path.");
        return trimmed.TrimEnd('/');
    }

    private static bool SameHeader(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}

using System.Text.Json.Serialization;
using IronHive.Providers.OpenAI.Compatible;

namespace IronHive.Providers.Declarations;

/// <summary>
/// The per-server behaviour a host would otherwise set in code, next to the endpoint and the key: how patient to be, how often to
/// retry, and the spellings and layouts a particular server needs. Every property is optional; <see langword="null"/> means the
/// adapter's own default, so a declaration without <see cref="ProviderDeclaration.Settings"/> behaves exactly as before.
/// </summary>
/// <remarks>
/// <para>
/// Set on a <see cref="ProviderDeclaration"/> they apply to all its models; set on a <see cref="ModelDeclaration"/> they override the
/// provider's value for that model, property by property (a chat template and a reasoning model's pace are per model).
/// </para>
/// <para>
/// A setting the effective wire has no home for is refused with an <see cref="ArgumentException"/> naming the provider and the
/// setting - the same rule as an unsendable key header - instead of being dropped without a word. Where each applies:
/// the three timeouts, every wire; <see cref="MaxRetries"/>, Responses and Anthropic; the last three, Chat Completions only.
/// </para>
/// <para>Enums are serialised by name, like <see cref="WireFormat"/>; timeouts as <c>hh:mm:ss</c> text in JSON.</para>
/// </remarks>
public sealed record ServerSettings
{
    /// <summary>How long to wait for the connection to the server to open (adapter default: a few seconds). Greater than zero.</summary>
    public TimeSpan? ConnectTimeout { get; init; }

    /// <summary>The ceiling for one whole request, a stream included (adapter default: none). Greater than zero.</summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// How long a started stream may stay silent before it ends with a <see cref="TimeoutException"/> (adapter default: none).
    /// A local server that is slow to produce its first token needs a long one, a gateway that stalls a short one. Greater than zero.
    /// </summary>
    public TimeSpan? StreamIdleTimeout { get; init; }

    /// <summary>
    /// How many times a failed request is retried by the vendor SDK underneath; <c>0</c> for a caller that retries itself. Responses and Anthropic
    /// only: the Chat Completions adapter sends each request once (retry above it) and the Gemini adapter has no such setting.
    /// </summary>
    public int? MaxRetries { get; init; }

    /// <summary>
    /// Where a request's per-request instructions go - a property of the server's chat template and cache behaviour (Gemma 3 refuses a
    /// second system message; a sliding-window model keeps its prefix cache only with <see cref="OpenAI.Compatible.InstructionsPlacement.LeadingUserTurn"/>).
    /// Chat Completions only.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InstructionsPlacement? InstructionsPlacement { get; init; }

    /// <summary>Which spelling of the output limit the server honours (<c>max_completion_tokens</c>, <c>max_tokens</c> or both). Chat Completions only.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TokenLimitParameter? TokenLimitParameter { get; init; }

    /// <summary>
    /// Whether an image returned by a tool is carried to the model as a following user message - a Chat Completions <c>tool</c> message holds
    /// text only and vLLM or llama.cpp refuse an image part there. Chat Completions only.
    /// </summary>
    public bool? CarryImageToolResultsAsUserMessage { get; init; }
}

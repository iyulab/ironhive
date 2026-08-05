namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// Which output-length parameter to put on the wire for an OpenAI-compatible Chat Completions request.
/// </summary>
/// <remarks>
/// <para>
/// OpenAI renamed <c>max_tokens</c> to <c>max_completion_tokens</c>, and the two names now split the
/// ecosystem: current OpenAI models reject the old name outright, while many self-hosted and
/// gateway implementations only recognise it — and a server that does not recognise a field ignores
/// it without error, so the limit is dropped in silence and the caller sees only an unexpectedly
/// long response.
/// </para>
/// <para>
/// There is no name that is safe everywhere, and sending both is not a safe default either: an
/// endpoint that rejects <c>max_tokens</c> fails the whole request. Absorbing that split is this
/// package's job — it exists to talk to compatible endpoints, whose dialect the caller usually knows
/// — but the choice has to be the caller's, so this is a setting rather than a guess.
/// </para>
/// </remarks>
public enum TokenLimitParameter
{
    /// <summary>
    /// Send only <c>max_completion_tokens</c> (default). The current OpenAI spelling, and the only
    /// one accepted by newer OpenAI models.
    /// </summary>
    MaxCompletionTokens = 0,

    /// <summary>
    /// Send only <c>max_tokens</c>. For servers that predate the rename and silently ignore the new
    /// name — llama.cpp-derived servers and self-hosted gateways commonly do.
    /// </summary>
    MaxTokens,

    /// <summary>
    /// Send both, with the same value. Removes the guesswork on endpoints that accept either, but
    /// fails on endpoints that reject the deprecated name — do not use as a blanket default.
    /// </summary>
    Both
}

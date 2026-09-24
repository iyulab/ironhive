namespace IronHive.Abstractions.Messages;

/// <summary>
/// Asks the provider for the log probability of each output token.
/// </summary>
public sealed class LogProbabilityOptions
{
    /// <summary>The most alternatives a provider returns per position (OpenAI and Gemini both cap it at 20).</summary>
    public const int MaxTopAlternatives = 20;

    private int _topAlternatives;

    /// <summary>
    /// How many of the most likely alternatives to return at each position, besides the chosen token:
    /// 0 returns the chosen token's log probability only; at most <see cref="MaxTopAlternatives"/>.
    /// </summary>
    public int TopAlternatives
    {
        get => _topAlternatives;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxTopAlternatives);
            _topAlternatives = value;
        }
    }
}

/// <summary>
/// One output token with its log probability and, when requested, the most likely alternatives at its position.
/// </summary>
/// <param name="Token">The token as the provider reports it.</param>
/// <param name="LogProbability">Natural-log probability of the token.</param>
/// <param name="Alternatives">The most likely tokens at this position, most likely first (the chosen token may be among them).</param>
public sealed record TokenLogProbability(string Token, double LogProbability, IReadOnlyList<TokenAlternative> Alternatives);

/// <summary>A candidate token at a position, with its log probability.</summary>
/// <param name="Token">The token as the provider reports it.</param>
/// <param name="LogProbability">Natural-log probability of the token.</param>
public sealed record TokenAlternative(string Token, double LogProbability);

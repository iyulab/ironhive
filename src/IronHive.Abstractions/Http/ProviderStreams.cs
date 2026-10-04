using System.Globalization;
using System.Runtime.CompilerServices;

namespace IronHive.Abstractions.Http;

/// <summary>
/// The stream idle budget the providers apply to a streamed response: the longest the response may stay silent —
/// from the request until the first event, and between two events after that.
/// </summary>
/// <remarks>
/// A whole-request deadline cannot tell a slow stream from a dead one: set long enough to clear minutes of prompt
/// evaluation on slow hardware, it also caps how long a healthy answer may stream, and it notices a stall only at its
/// end. The gap between reads is what separates the two. Time the consumer spends handling an event is not counted —
/// the budget runs only while the next event is awaited.
/// </remarks>
public static class ProviderStreams
{
    /// <summary>
    /// Enumerates the stream <paramref name="open"/> returns, ending it with <see cref="TimeoutException"/> when the next
    /// event does not arrive within <paramref name="idleTimeout"/>. <see cref="Timeout.InfiniteTimeSpan"/> passes the
    /// stream through unchanged.
    /// </summary>
    /// <param name="open">Starts the stream with the token to observe; called once, on first enumeration.</param>
    /// <param name="idleTimeout">The budget; positive or <see cref="Timeout.InfiniteTimeSpan"/>.</param>
    /// <param name="cancellationToken">The caller's token. Its cancellation surfaces as cancellation, never as a timeout.</param>
    public static IAsyncEnumerable<T> WithIdleTimeout<T>(
        Func<CancellationToken, IAsyncEnumerable<T>> open,
        TimeSpan idleTimeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(open);
        ThrowIfInvalid(idleTimeout, nameof(idleTimeout));
        return idleTimeout == Timeout.InfiniteTimeSpan
            ? open(cancellationToken)
            : Iterate(open, idleTimeout, cancellationToken);
    }

    /// <summary>The exception a stream idle budget ends a stream with — distinct in wording from a request timeout.</summary>
    public static TimeoutException IdleTimeout(TimeSpan idleTimeout, Exception? innerException = null)
        => new(string.Create(CultureInfo.InvariantCulture,
            $"The response stream was silent for longer than the stream idle timeout ({idleTimeout.TotalSeconds:0.###} s)."),
            innerException);

    /// <summary>Rejects a budget that is neither positive nor <see cref="Timeout.InfiniteTimeSpan"/>.</summary>
    /// <param name="value">The configured budget.</param>
    /// <param name="paramName">The setting's name, as the consumer wrote it (for example <c>OpenAIConfig.StreamIdleTimeout</c>).</param>
    public static void ThrowIfInvalid(TimeSpan value, string paramName)
    {
        if (value != Timeout.InfiniteTimeSpan && value <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(paramName, value, "A stream idle timeout must be positive or Timeout.InfiniteTimeSpan.");
    }

    private static async IAsyncEnumerable<T> Iterate<T>(
        Func<CancellationToken, IAsyncEnumerable<T>> open,
        TimeSpan idleTimeout,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // An SDK stream takes its token once, when it starts, so one source is re-armed around each wait. The timer is
        // disarmed as soon as an event arrives; an expiry in the instant between the two still reads as idle, which it
        // nearly was. A hand-written reader that can take a token per read uses a fresh source per read instead.
        using var idle = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await using var enumerator = open(idle.Token).GetAsyncEnumerator(idle.Token);
        while (true)
        {
            idle.CancelAfter(idleTimeout);
            bool hasNext;
            try
            {
                hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
            }
            catch (Exception ex) when (idle.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                throw IdleTimeout(idleTimeout, ex);
            }

            idle.CancelAfter(Timeout.InfiniteTimeSpan);
            if (!hasNext)
                yield break;
            yield return enumerator.Current;
        }
    }
}

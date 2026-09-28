using System.Text;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// Keeps <c>StopSequences</c> on the client side. The Responses API has no <c>stop</c> parameter, so the generator
/// cuts the output at the first stop sequence itself — the buffered text at once, the stream by holding back the
/// last (longest stop − 1) characters of text until they can no longer start a match. As with a server-side stop,
/// the stop sequence is not part of the output and nothing after it is returned.
/// </summary>
internal sealed class StopSequenceFilter
{
    private readonly string[] _stops;
    private readonly int _hold;
    private readonly StringBuilder _held = new();

    private StopSequenceFilter(string[] stops)
    {
        _stops = stops;
        _hold = stops.Max(s => s.Length) - 1;
    }

    /// <summary>A filter for <paramref name="stops"/>, or null when there is no non-empty stop sequence.</summary>
    public static StopSequenceFilter? For(IEnumerable<string>? stops)
    {
        var usable = stops?.Where(s => !string.IsNullOrEmpty(s)).Distinct(StringComparer.Ordinal).ToArray();
        return usable is { Length: > 0 } ? new StopSequenceFilter(usable) : null;
    }

    /// <summary>True once a stop sequence has been seen; everything after it is dropped.</summary>
    public bool Stopped { get; private set; }

    /// <summary>The text before the first stop sequence in <paramref name="text"/>, or null when none occurs.</summary>
    public string? Cut(string text)
    {
        var at = FirstMatch(text);
        if (at < 0)
            return null;
        Stopped = true;
        return text[..at];
    }

    /// <summary>
    /// Adds a streamed text delta and returns the part that is safe to emit now: everything before a stop sequence
    /// (then <see cref="Stopped"/> is set and the rest is dropped), or everything except a tail that could still be
    /// the start of one.
    /// </summary>
    public string Push(string delta)
    {
        if (Stopped)
            return string.Empty;
        _held.Append(delta);
        var text = _held.ToString();
        var at = FirstMatch(text);
        if (at >= 0)
        {
            Stopped = true;
            _held.Clear();
            return text[..at];
        }
        var ready = Math.Max(0, text.Length - _hold);
        _held.Remove(0, ready);
        return text[..ready];
    }

    /// <summary>Releases the held tail when the text it belongs to has ended without a stop sequence.</summary>
    public string Flush()
    {
        var text = _held.ToString();
        _held.Clear();
        return text;
    }

    private int FirstMatch(string text)
    {
        var first = -1;
        foreach (var stop in _stops)
        {
            var at = text.IndexOf(stop, StringComparison.Ordinal);
            if (at >= 0 && (first < 0 || at < first))
                first = at;
        }
        return first;
    }
}

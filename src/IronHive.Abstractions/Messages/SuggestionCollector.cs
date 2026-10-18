using System.Text;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// The text protocol behind <see cref="SuggestionOptions"/>: the instruction that asks a model to end its answer with
/// <c>&lt;suggestion&gt;</c> blocks, and a collector that takes those blocks out of the answer text — whole or streamed in
/// pieces — and returns them as <see cref="Suggestion"/>s. Provider-neutral: any loop that sends text to a model and reads
/// text back uses the same one.
/// </summary>
/// <remarks>Not thread-safe; one collector per response.</remarks>
public sealed class SuggestionCollector
{
    private enum State { Passthrough, Buffering, InTag }

    private State state = State.Passthrough;
    private readonly StringBuilder buffer = new();
    private readonly List<Suggestion> bucket = [];

    private const string OTag = "<suggestion>";
    private const string CTag = "</suggestion>";

    /// <summary>
    /// <paramref name="system"/> with the suggestion instruction for <paramref name="options"/> appended, or
    /// <paramref name="system"/> unchanged when <paramref name="options"/> is null.
    /// </summary>
    public static string? Prompt(string? system, SuggestionOptions? options)
    {
        if (options == null) return system;
        var mode = options.Mode == SuggestionMode.Always
            ? "Always end"
            : "When it would help the user decide their next step, end";
        var count = options.MaxCount == 1
            ? "the following block"
            : $"up to {options.MaxCount} of the following blocks";

        var addon = $"""
            {mode} your response with {count}:
            <suggestion>
            Q: <one concise question>
            A: <option 1>
            A: <option 2>
            </suggestion>
            Each block: {options.MinItems}–{options.MaxItems} options.
            """;

        return string.IsNullOrEmpty(system) ? addon : $"{system}\n\n{addon}";
    }

    /// <summary>The suggestions collected so far, or null when there are none; the collector forgets them.</summary>
    public List<Suggestion>? Drain()
    {
        if (bucket.Count == 0) return null;
        var result = new List<Suggestion>(bucket);
        bucket.Clear();
        return result;
    }

    /// <summary>
    /// Feeds the next piece of answer text and returns the part of it that is not a suggestion block. A block may span
    /// pieces; a completed block is collected (<see cref="Drain"/>).
    /// </summary>
    public string Feed(string text)
    {
        var output = new StringBuilder();

        foreach (var c in text)
        {
            switch (state)
            {
                case State.Passthrough:
                    if (c == '<')
                    {
                        buffer.Clear();
                        buffer.Append(c);
                        state = State.Buffering;
                    }
                    else
                    {
                        output.Append(c);
                    }
                    break;

                case State.Buffering:
                    buffer.Append(c);
                    var la = buffer.ToString();
                    if (la == OTag)
                    {
                        buffer.Clear(); // reused for the block's content
                        state = State.InTag;
                    }
                    else if (!OTag.StartsWith(la, StringComparison.Ordinal))
                    {
                        output.Append(la);
                        buffer.Clear();
                        state = State.Passthrough;
                    }
                    break;

                case State.InTag:
                    buffer.Append(c);
                    if (buffer.Length >= CTag.Length &&
                        buffer.ToString(buffer.Length - CTag.Length, CTag.Length) == CTag)
                    {
                        var inner = buffer.ToString(0, buffer.Length - CTag.Length);
                        bucket.Add(ParseBlock(inner));
                        buffer.Clear();
                        state = State.Passthrough;
                    }
                    break;
            }
        }

        return output.ToString();
    }

    /// <summary>
    /// Ends the text: returns what <see cref="Feed"/> held back waiting to see whether it starts a block — a trailing
    /// <c>&lt;</c> or <c>&lt;sugg</c>, or a block the model never closed — as plain text, and starts over.
    /// </summary>
    public string Flush()
    {
        var held = state switch
        {
            State.Buffering => buffer.ToString(),
            State.InTag => OTag + buffer,
            _ => string.Empty,
        };
        buffer.Clear();
        state = State.Passthrough;
        return held;
    }

    private static Suggestion ParseBlock(string inner)
    {
        var result = new Suggestion();
        foreach (var line in inner.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line.StartsWith("Q: ", StringComparison.Ordinal))
                result.Question = line[3..].Trim();
            else if (line.StartsWith("A: ", StringComparison.Ordinal))
                result.Items.Add(line[3..].Trim());
        }
        return result;
    }
}

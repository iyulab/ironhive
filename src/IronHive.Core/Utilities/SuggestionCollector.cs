using System.Text;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Utilities;

/// <summary>
/// 텍스트 스트림에서 지정 태그를 감지·수집하는 수집기입니다.
/// 스트리밍에서는 Feed()로 태그를 먹어치우고, 완료 시 Drain()으로 결과를 회수합니다.
/// </summary>
internal sealed class SuggestionCollector
{
    private enum State { Passthrough, Buffering, InTag }

    private State state = State.Passthrough;
    private readonly StringBuilder buffer = new();
    private readonly List<Suggestion> bucket = [];

    private const string OTag = "<suggestion>";
    private const string CTag = "</suggestion>";

    /// <summary>
    /// 제안 옵션을 반영한 시스템 프롬프트를 빌드합니다.
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

    /// <summary>
    /// 지금까지 수집된 제안 목록을 회수하고, 내부 컬렉션을 비웁니다.
    /// </summary>
    public List<Suggestion>? Drain()
    {
        if (bucket.Count == 0) return null;
        var result = new List<Suggestion>(bucket);
        bucket.Clear();
        return result; 
    }

    /// <summary>
    /// 텍스트를 수집기에 공급합니다.
    /// 태그 구간은 소비(suppress)하고, 그 외 텍스트만 반환합니다.
    /// 완성된 태그는 내부 컬렉션에 누적됩니다.
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
                        buffer.Clear(); // InTag에서 tag 내용 누적용으로 재활용
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

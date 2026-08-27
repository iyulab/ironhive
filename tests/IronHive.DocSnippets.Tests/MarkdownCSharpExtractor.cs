using System.Text;

namespace IronHive.DocSnippets.Tests;

/// <summary>
/// Extracts the first ```csharp fenced code block following a given heading line, up to (but not
/// including) the next heading or thematic break. Kept deliberately narrow — this project checks
/// specific, known-important sections rather than every fence in the docs tree (see
/// DocSnippetComplianceTests for which sections and why).
/// </summary>
internal static class MarkdownCSharpExtractor
{
    public static string ExtractFirstFenceAfterHeading(string markdown, string headingLine)
    {
        var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var headingIndex = Array.FindIndex(lines, l => l.Trim() == headingLine.Trim());
        if (headingIndex < 0)
            throw new InvalidOperationException($"Heading '{headingLine}' not found in the document.");

        var i = headingIndex + 1;
        for (; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("##", StringComparison.Ordinal) || trimmed == "---")
                throw new InvalidOperationException(
                    $"No ```csharp fence found under heading '{headingLine}' before the next section.");
            if (trimmed.StartsWith("```csharp", StringComparison.Ordinal))
                break;
        }
        if (i >= lines.Length)
            throw new InvalidOperationException($"No ```csharp fence found under heading '{headingLine}'.");

        var content = new StringBuilder();
        for (i++; i < lines.Length && lines[i].TrimStart() != "```"; i++)
        {
            content.AppendLine(lines[i]);
        }
        return content.ToString();
    }
}

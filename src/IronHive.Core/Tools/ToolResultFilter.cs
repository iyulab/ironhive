using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Tools;

/// <summary>
/// Filters tool execution results to reduce token consumption.
/// Strategies: JSON→CSV conversion, whitespace normalization, large result truncation.
/// </summary>
public partial class ToolResultFilter : IToolResultFilter
{
    private readonly ToolResultFilterOptions _options;

    /// <summary>
    /// Creates a new tool result filter with the specified options.
    /// </summary>
    public ToolResultFilter(ToolResultFilterOptions? options = null)
    {
        _options = options ?? new ToolResultFilterOptions();
    }

    /// <inheritdoc />
    public ToolOutput Filter(string toolName, ToolOutput output)
    {
        if (output.Result is not { Length: > 0 })
        {
            return output;
        }

        var result = output.Result;

        // 1. JSON array → CSV conversion
        if (_options.EnableJsonToCsv)
        {
            result = TryConvertJsonArrayToCsv(result);
        }

        // 2. Whitespace normalization
        if (_options.EnableWhitespaceNormalization)
        {
            result = NormalizeWhitespace(result);
        }

        // 3. Truncation for oversized results
        if (result.Length > _options.MaxResultChars)
        {
            result = Truncate(result);
        }

        // Return original if unchanged
        if (string.Equals(result, output.Result, StringComparison.Ordinal))
        {
            return output;
        }

        return new ToolOutput(output.IsSuccess, result);
    }

    /// <summary>
    /// Converts a JSON array of flat objects to CSV format.
    /// Only converts when all elements share consistent keys and have no nested values.
    /// </summary>
    public static string TryConvertJsonArrayToCsv(string result, int minElements = 3)
    {
        // Quick check: must start with '['
        var trimmed = result.AsSpan().TrimStart();
        if (trimmed.Length == 0 || trimmed[0] != '[')
        {
            return result;
        }

        try
        {
            using var doc = JsonDocument.Parse(result);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return result;
            }

            var arrayLength = doc.RootElement.GetArrayLength();
            if (arrayLength < minElements)
            {
                return result;
            }

            // Extract keys from the first element
            var keys = new List<string>();
            var first = true;
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                {
                    return result; // Not all objects
                }

                if (first)
                {
                    foreach (var prop in element.EnumerateObject())
                    {
                        // Skip nested objects/arrays — not suitable for CSV
                        if (prop.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                        {
                            return result;
                        }
                        keys.Add(prop.Name);
                    }
                    first = false;
                }
            }

            if (keys.Count == 0)
            {
                return result;
            }

            // Build CSV
            var sb = new StringBuilder();
            sb.AppendJoin(',', keys.Select(EscapeCsvField));
            sb.AppendLine();

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                for (var i = 0; i < keys.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(',');
                    }

                    if (element.TryGetProperty(keys[i], out var val))
                    {
                        var text = val.ValueKind switch
                        {
                            JsonValueKind.String => EscapeCsvField(val.GetString() ?? ""),
                            JsonValueKind.Null => "",
                            _ => val.GetRawText()
                        };
                        sb.Append(text);
                    }
                }
                sb.AppendLine();
            }

            var csv = sb.ToString();

            // Only use CSV if it's actually shorter
            return csv.Length < result.Length ? csv : result;
        }
        catch (JsonException)
        {
            return result;
        }
    }

    public static string EscapeCsvField(string field)
    {
        if (field.AsSpan().IndexOfAny(',', '"', '\n') >= 0)
        {
            return string.Concat("\"", field.Replace("\"", "\"\"", StringComparison.Ordinal), "\"");
        }
        return field;
    }

    public static string NormalizeWhitespace(string result)
    {
        // Collapse 3+ consecutive newlines to 2
        var normalized = ExcessiveNewlinesRegex().Replace(result, "\n\n");

        // Trim trailing whitespace from each line
        normalized = TrailingWhitespaceRegex().Replace(normalized, "");

        return normalized.Trim();
    }

    internal string Truncate(string result)
    {
        var lines = result.Split('\n');
        var totalKeep = _options.KeepHeadLines + _options.KeepTailLines;

        if (lines.Length > totalKeep && totalKeep > 0)
        {
            var head = lines.AsSpan(0, _options.KeepHeadLines);
            var tail = lines.AsSpan(lines.Length - _options.KeepTailLines);
            var omitted = lines.Length - totalKeep;

            return string.Concat(
                string.Join('\n', head.ToArray()),
                string.Create(CultureInfo.InvariantCulture,
                    $"\n\n[... {omitted:N0} lines omitted ({result.Length:N0} chars total) ...]\n\n"),
                string.Join('\n', tail.ToArray()));
        }

        // Fallback: character-based truncation
        return string.Concat(
            result.AsSpan(0, _options.MaxResultChars),
            string.Create(CultureInfo.InvariantCulture,
                $"\n[... truncated ({result.Length:N0} chars total) ...]"));
    }

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExcessiveNewlinesRegex();

    [GeneratedRegex(@"[ \t]+$", RegexOptions.Multiline)]
    private static partial Regex TrailingWhitespaceRegex();
}

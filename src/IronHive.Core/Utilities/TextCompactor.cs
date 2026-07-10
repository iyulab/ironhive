using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace IronHive.Core.Utilities;

/// <summary>
/// 긴 텍스트(툴 출력, 로그 등)를 토큰/문자 절감을 위해 압축하는 범용 유틸리티입니다.
/// 전략: JSON→CSV 변환, 공백 정규화, 대용량 잘라내기.
/// 특정 도메인 타입에 묶이지 않은 순수 string 유틸리티이므로,
/// 필요한 위치(예: ToolOptions.OnAfterInvoke)에서 호출자가 직접 연결해 사용합니다.
/// </summary>
public static partial class TextCompactor
{
    /// <summary>
    /// 옵션에 활성화된 전략들을 순서대로 적용해 텍스트를 압축합니다.
    /// </summary>
    public static string Compact(string text, TextCompactorOptions? options = null)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        options ??= new TextCompactorOptions();
        var result = text;

        if (options.EnableJsonToCsv)
        {
            result = TryConvertJsonArrayToCsv(result, options.JsonToCsvMinElements);
        }

        if (options.EnableWhitespaceNormalization)
        {
            result = NormalizeWhitespace(result);
        }

        if (result.Length > options.MaxResultChars)
        {
            result = Truncate(result, options.MaxResultChars, options.KeepHeadLines, options.KeepTailLines);
        }

        return result;
    }

    /// <summary>
    /// 평탄한 객체로 이루어진 JSON 배열을 CSV 형식으로 변환합니다.
    /// 모든 요소가 동일한 키를 가지고 중첩 값이 없을 때만 변환합니다.
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

    /// <summary>
    /// 텍스트가 <paramref name="maxResultChars"/>를 초과하면 앞/뒤 일부 줄만 남기고 잘라냅니다.
    /// 줄바꿈이 없는 텍스트는 문자 단위로 잘라냅니다.
    /// </summary>
    public static string Truncate(string result, int maxResultChars, int keepHeadLines, int keepTailLines)
    {
        var lines = result.Split('\n');
        var totalKeep = keepHeadLines + keepTailLines;

        if (lines.Length > totalKeep && totalKeep > 0)
        {
            var head = lines.AsSpan(0, keepHeadLines);
            var tail = lines.AsSpan(lines.Length - keepTailLines);
            var omitted = lines.Length - totalKeep;

            return string.Concat(
                string.Join('\n', head.ToArray()),
                string.Create(CultureInfo.InvariantCulture,
                    $"\n\n[... {omitted:N0} lines omitted ({result.Length:N0} chars total) ...]\n\n"),
                string.Join('\n', tail.ToArray()));
        }

        // Fallback: character-based truncation
        return string.Concat(
            result.AsSpan(0, maxResultChars),
            string.Create(CultureInfo.InvariantCulture,
                $"\n[... truncated ({result.Length:N0} chars total) ...]"));
    }

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExcessiveNewlinesRegex();

    [GeneratedRegex(@"[ \t]+$", RegexOptions.Multiline)]
    private static partial Regex TrailingWhitespaceRegex();
}

/// <summary>
/// <see cref="TextCompactor"/>의 동작 설정입니다.
/// </summary>
public class TextCompactorOptions
{
    /// <summary>
    /// JSON 배열 출력을 CSV 형식으로 변환할지 여부입니다.
    /// 평탄한(nested 없는) 객체로 이루어진 JSON 배열은 CSV로 변환 시 약 40~50%의 문자 절감이 있습니다.
    /// 기본값: true.
    /// </summary>
    public bool EnableJsonToCsv { get; set; } = true;

    /// <summary>
    /// JSON→CSV 변환을 적용할 최소 배열 요소 수입니다. 이보다 적으면 변환하지 않습니다.
    /// 기본값: 3.
    /// </summary>
    public int JsonToCsvMinElements { get; set; } = 3;

    /// <summary>
    /// 과도한 공백을 정규화할지 여부입니다.
    /// 3개 이상 연속된 빈 줄을 2개로 줄이고, 각 줄의 trailing 공백을 제거합니다.
    /// 기본값: true.
    /// </summary>
    public bool EnableWhitespaceNormalization { get; set; } = true;

    /// <summary>
    /// 이 문자 수를 초과하면 잘라냅니다.
    /// 기본값: 50,000.
    /// </summary>
    public int MaxResultChars { get; set; } = 50_000;

    /// <summary>
    /// 잘라낼 때 앞에서 유지할 줄 수입니다.
    /// 기본값: 100.
    /// </summary>
    public int KeepHeadLines { get; set; } = 100;

    /// <summary>
    /// 잘라낼 때 뒤에서 유지할 줄 수입니다.
    /// 기본값: 30.
    /// </summary>
    public int KeepTailLines { get; set; } = 30;
}
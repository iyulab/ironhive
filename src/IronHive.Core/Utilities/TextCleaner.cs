using System.Text.RegularExpressions;

namespace IronHive.Core.Utilities;

public static class TextCleaner
{
    private static readonly Regex LineBreakNormalizationRegex = new(@"\r\n|\r", RegexOptions.Compiled);
    private static readonly Regex MultipleLineBreaksRegex = new(@"\n{3,}", RegexOptions.Compiled);
    private static readonly Regex MultipleSpacesRegex = new(@"\s{2,}", RegexOptions.Compiled);

    /// <summary>
    /// 입력된 텍스트를 정리하여 불필요한 공백과 과도한 줄 바꿈을 제거합니다.
    /// </summary>
    /// <param name="input">정리할 원본 텍스트</param>
    /// <returns>정리된 텍스트</returns>
    public static string Clean(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // 1. 줄바꿈 정규화: CRLF 또는 CR을 LF로 변환
        string cleanedText = LineBreakNormalizationRegex.Replace(input, "\n");

        // 2. 줄바꿈 정리: 3개 이상의 줄바꿈을 2개로 줄임
        cleanedText = MultipleLineBreaksRegex.Replace(cleanedText, "\n\n");

        // 3. 공백 정리: 각 줄의 앞뒤 공백을 제거하고 2개 이상의 공백을 1개로 줄임
        var lines = cleanedText.Split('\n');
        lines = lines.Select(line => MultipleSpacesRegex.Replace(line.Trim(), " ")).ToArray();
        cleanedText = string.Join("\n", lines);

        // 4. 줄바꿈 정리: 3개 이상의 줄바꿈을 2개로 줄임
        cleanedText = MultipleLineBreaksRegex.Replace(cleanedText, "\n\n");

        return cleanedText;
    }
}

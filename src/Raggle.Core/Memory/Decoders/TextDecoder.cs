using Raggle.Core.Memory.Decoders;
using Raggle.Core.Memory.Document;
using Raggle.Core.Utils;

namespace Raggle.Core.Memory.Decoders;

public class TextDecoder : IDocumentDecoder
{
    private readonly int _maxSplitLine;

    /// <inheritdoc />
    public IEnumerable<string> SupportContentTypes =>
    [
        "text/plain"
    ];

    public TextDecoder(int maxSplitLine = 10)
    {
        _maxSplitLine = maxSplitLine;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentSection>> DecodeAsync(
        Stream data, 
        CancellationToken cancellationToken = default)
    {
        var results = new List<DocumentSection>();

        using var reader = new StreamReader(data);
        if (_maxSplitLine <= 0)
        {
            var fullText = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            var cleanText = TextCleaner.Clean(fullText);
            results.Add(new DocumentSection
            {
                Identifier = "Entire Document",
                Text = cleanText,
            });
        }
        else
        {
            var lines = new List<string>();
            int currentLineNumber = 0;
            int startLineNumber = 1;

            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
            {
                // 라인 추가
                lines.Add(line);
                currentLineNumber++;

                if (lines.Count == _maxSplitLine)
                {
                    var sectionText = string.Join(Environment.NewLine, lines).TrimEnd();
                    var cleanText = TextCleaner.Clean(sectionText);
                    results.Add(new DocumentSection
                    {
                        Identifier = $"{startLineNumber} line ~ {currentLineNumber} line",
                        Text = cleanText,
                    });

                    // 다음 섹션을 위해 초기화
                    lines.Clear();
                    startLineNumber = currentLineNumber + 1;
                }
            }

            // 남아있는 라인들을 마지막 섹션으로 추가
            if (lines.Count > 0)
            {
                var sectionText = string.Join(Environment.NewLine, lines).TrimEnd();
                var cleanText = TextCleaner.Clean(sectionText);
                results.Add(new DocumentSection
                {
                    Identifier = $"{startLineNumber} line ~ {currentLineNumber} line",
                    Text = cleanText,
                });
            }
        }

        // 취소 요청 확인
        cancellationToken.ThrowIfCancellationRequested();

        return results;
    }
}

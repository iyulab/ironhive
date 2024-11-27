using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory.Document;
using Raggle.Core.Utils;
using System.Text;

namespace Raggle.Core.Memory.Decoders;

public class WordDecoder : IDocumentDecoder
{
    private readonly int _maxSplitParagraphs = 10;

    /// <inheritdoc />
    public async Task<object> DecodeAsync(
        Stream data,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var results = new List<DocumentSection>();

            using var word = WordprocessingDocument.Open(data, false)
                ?? throw new InvalidOperationException("Failed to open the Word document.");

            var paragraphs = word.MainDocumentPart?.Document.Body?.Descendants<Paragraph>()?.ToList()
                ?? throw new InvalidOperationException("The document body is missing.");

            if (_maxSplitParagraphs <= 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 모든 문단을 하나의 섹션으로 처리
                var fullText = string.Join(Environment.NewLine, paragraphs.Select(p => p.InnerText)).TrimEnd();
                var cleanText = TextCleaner.Clean(fullText);
                results.Add(new DocumentSection
                {
                    Identifier = "Entire Document",
                    Text = cleanText,
                });
            }
            else
            {
                var sb = new StringBuilder();
                int startParagraphNumber = 1;
                int currentParagraphNumber = 1;

                foreach (var p in paragraphs)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    sb.AppendLine(p.InnerText);

                    // 최대 문단 수에 도달하면 섹션으로 분할
                    if (currentParagraphNumber - startParagraphNumber + 1 == _maxSplitParagraphs)
                    {
                        var endParagraphNumber = currentParagraphNumber;
                        var sectiontext = sb.ToString().TrimEnd();
                        var cleanText = TextCleaner.Clean(sectiontext);

                        results.Add(new DocumentSection
                        {
                            Identifier = $"{startParagraphNumber} paragraph ~ {endParagraphNumber} paragraph",
                            Text = cleanText,
                        });

                        // 다음 섹션으로 초기화
                        sb.Clear();
                        startParagraphNumber = currentParagraphNumber + 1;
                    }

                    currentParagraphNumber++;
                }

                // 남은 문단이 있는 경우 마지막 섹션으로 처리
                if (sb.Length > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var endParagraphNumber = currentParagraphNumber - 1;
                    var sectionText = sb.ToString().TrimEnd();
                    var cleanText = TextCleaner.Clean(sectionText);

                    results.Add(new DocumentSection
                    {
                        Identifier = $"{startParagraphNumber} paragraph ~ {endParagraphNumber} paragraph",
                        Text = cleanText,
                    });
                }
            }

            // 최종적으로 한번 더 취소 요청 확인
            cancellationToken.ThrowIfCancellationRequested();

            return results;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public bool IsSupportContentType(string contentType)
    {
        return contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    }
}

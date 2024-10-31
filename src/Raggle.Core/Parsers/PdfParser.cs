using Raggle.Core.Document;
using Raggle.Core.Utils;
using System.Collections.Concurrent;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Raggle.Core.Parsers;

public class PDFParser : IDocumentParser
{
    /// <inheritdoc />
    public string[] SupportContentTypes => 
    [
        "application/pdf"
    ];

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentSection>> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<DocumentSection>();

        using var pdf = PdfDocument.Open(stream);
        var pages = pdf.GetPages();

        // 페이지 별로 병렬 처리
        await Parallel.ForEachAsync(pages, new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        }, async (page, ct) =>
        {
            // 각 슬라이드를 처리하기 전에 취소 요청을 확인
            ct.ThrowIfCancellationRequested();

            // 페이지의 텍스트 추출
            var sectionText = ContentOrderTextExtractor.GetText(page);
            var cleanText = TextCleaner.Clean(sectionText);
            results.Add(new DocumentSection
            {
                Identifier = $"Page {page.Number}",
                Text = cleanText,
            });

            await Task.CompletedTask;
        });

        return results;
    }
}

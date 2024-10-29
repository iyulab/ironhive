using Raggle.Core.Document;
using Raggle.Core.Utils;
using System.Collections.Concurrent;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Raggle.Core.Parsers;

public class PdfParser : IDocumentParser
{
    /// <inheritdoc />
    public string[] SupportTypes => 
    [
        "application/pdf"
    ];

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentSection>> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using var pdf = PdfDocument.Open(stream);
        var pages = pdf.GetPages();

        var results = new ConcurrentBag<DocumentSection>();
        await Parallel.ForEachAsync(pages, cancellationToken, (page, ct) =>
        {
            ct.ThrowIfCancellationRequested();

            // Get text from page
            var text = ContentOrderTextExtractor.GetText(page, true);
            text = TextCleaner.Clean(text);
            results.Add(new DocumentSection(page.Number, text));
            return ValueTask.CompletedTask;
        });

        return results.OrderBy(x => x.Number);
    }
}

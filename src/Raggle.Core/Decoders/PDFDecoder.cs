using Raggle.Abstractions.Memory;
using System.Collections.Concurrent;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Raggle.Core.Extractors;

public class PDFDecoder : IContentDecoder
{
    /// <inheritdoc />
    public string[] SupportTypes => 
    [
        "application/pdf"
    ];

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentSection>> DecodeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using var pdf = PdfDocument.Open(stream);
        var pages = pdf.GetPages();

        var results = new ConcurrentBag<DocumentSection>();
        await Parallel.ForEachAsync(pages, cancellationToken, (page, ct) =>
        {
            ct.ThrowIfCancellationRequested();

            // Get text from page
            var text = ContentOrderTextExtractor.GetText(page, true);
            var section = new DocumentSection(page.Number, text);

            results.Add(section);
            return ValueTask.CompletedTask;
        });

        return results.OrderBy(x => x.Number);
    }
}

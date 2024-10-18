using Raggle.Abstractions.Memory;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Raggle.Core.Extractors;

public class PDFDecoder : IContentDecoder
{
    /// <inheritdoc />
    public string[] SupportTypes => 
    [
        "application/pdf",
        "application/x-pdf",
        "application/acrobat",
        "applications/vnd.pdf",
        "text/pdf",
        "text/x-pdf"
    ];

    /// <inheritdoc />
    public Task<IDocumentContent[]> DecodeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using var pdfDocument = PdfDocument.Open(stream) ?? throw new Exception("Failed to open PDF document.");

        var results = new List<IDocumentContent>();
        foreach (var page in pdfDocument.GetPages())
        {
            var text = page.Text;
            var images = page.GetImages();
            results.Add(new TextDocumentContent { Text = text });
        }

        return Task.FromResult(results.ToArray());
    }

}

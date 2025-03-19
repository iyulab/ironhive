using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace IronHive.Core.Memory.Decoders;

public class PDFDecoder : IFileDecoder
{
    /// <inheritdoc />
    public bool IsSupportMimeType(string mimeType)
    {
        return mimeType == "application/pdf";
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> DecodeAsync(
        Stream data,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var contents = new List<string>();

            using var pdf = PdfDocument.Open(data);
            var pages = pdf.GetPages();
            foreach (var page in pages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var text = ContentOrderTextExtractor.GetText(page);
                text = TextCleaner.Clean(text);
                contents.Add(text);
            }

            cancellationToken.ThrowIfCancellationRequested();
            return contents;
        }, cancellationToken).ConfigureAwait(false);
    }
}

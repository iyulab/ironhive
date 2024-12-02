using Raggle.Abstractions.Memory;
using Raggle.Core.Memory.Document;
using Raggle.Core.Utils;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Raggle.Core.Memory.Decoders;

public class PDFDecoder : IDocumentDecoder
{
    /// <inheritdoc />
    public bool IsSupportMimeType(string mimeType)
    {
        return mimeType == "application/pdf";
    }

    /// <inheritdoc />
    public async Task<DocumentSource> DecodeAsync(
        DataPipeline pipeline,
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

            return new DocumentSource
            {
                Source = pipeline.Source,
                Section = new DocumentSegment
                {
                    Unit = "page",
                    From = 1,
                    To = contents.Count,
                },
                Content = contents,
            };
        }, cancellationToken);
    }
}

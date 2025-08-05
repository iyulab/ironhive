using IronHive.Abstractions.Files;
using IronHive.Core.Utilities;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace IronHive.Core.Files.Decoders;

/// <summary>
/// PDF 파일 디코더 클래스입니다.
/// </summary>
public class PDFDecoder : IFileDecoder
{
    /// <inheritdoc />
    public bool SupportsMimeType(string mimeType)
    {
        return mimeType switch
        {
            "application/pdf" => true,
            _ => false
        };
    }

    /// <inheritdoc />
    public Task<string> DecodeAsync(
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var content = new List<string>();

        using var pdf = PdfDocument.Open(data);
        var pages = pdf.GetPages();
        foreach (var page in pages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var text = ContentOrderTextExtractor.GetText(page);
            text = TextCleaner.Clean(text);
            content.Add(text);
        }

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(string.Join(Environment.NewLine, content));
    }
}

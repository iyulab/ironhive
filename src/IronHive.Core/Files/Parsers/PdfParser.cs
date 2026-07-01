using IronHive.Abstractions.Files;
using IronHive.Core.Utilities;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace IronHive.Core.Files.Parsers;

/// <summary>
/// PDF 파일을 파싱합니다. 페이지별로 <see cref="TextBlock"/>을 생성하고,
/// 페이지 내 PNG 이미지는 <see cref="ImageBlock"/>으로 추출합니다.
/// </summary>
public class PdfParser : IFileParser
{
    /// <inheritdoc />
    public bool CanParse(string fileName, string? mimeType = null)
        => fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
        || mimeType?.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) == true;

    /// <inheritdoc />
    public Task<IReadOnlyList<FileBlock>> ParseAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var blocks = new List<FileBlock>();
        try
        {
            using var pdf = PdfDocument.Open(data);
            foreach (var page in pdf.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var text = TextCleaner.Clean(ContentOrderTextExtractor.GetText(page));
                if (!string.IsNullOrWhiteSpace(text))
                    blocks.Add(new TextBlock { Label = $"{fileName} - Page {page.Number}", Text = text });

                foreach (var image in page.GetImages())
                {
                    try
                    {
                        if (image.TryGetPng(out var pngBytes))
                            blocks.Add(new ImageBlock { MimeType = "image/png", Data = pngBytes });
                    }
                    catch { }
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch { }

        return Task.FromResult<IReadOnlyList<FileBlock>>(blocks);
    }
}

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using IronHive.Abstractions.Files;
using IronHive.Core.Utilities;

namespace IronHive.Core.Files.Decoders;

/// <summary>
/// Word 파일 디코더 클래스입니다.
/// </summary>
public class WordDecoder : IFileDecoder<string>
{
    /// <inheritdoc />
    public bool SupportsMimeType(string mimeType)
    {
        return mimeType switch
        {
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => true,
            _ => false
        };
    }

    /// <inheritdoc />
    public Task<string> DecodeAsync(
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var content = new List<string>();

        using var word = WordprocessingDocument.Open(data, false)
            ?? throw new InvalidOperationException("Failed to open the Word document.");

        var paragraphs = word.MainDocumentPart?.Document?.Body?.Descendants<Paragraph>()?.ToList()
            ?? throw new InvalidOperationException("The document body is missing.");

        foreach (var p in paragraphs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var text = TextCleaner.Clean(p.InnerText);
            if (string.IsNullOrWhiteSpace(text))
                continue;
            content.Add(text);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(string.Join(Environment.NewLine, content));
    }
}

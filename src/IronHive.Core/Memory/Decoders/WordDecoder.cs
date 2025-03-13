using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace IronHive.Core.Memory.Decoders;

public class WordDecoder : IDocumentDecoder
{
    /// <inheritdoc />
    public bool IsSupportMimeType(string mimeType)
    {
        return mimeType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> DecodeAsync(
        Stream data,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var contents = new List<string>();

            using var word = WordprocessingDocument.Open(data, false)
                ?? throw new InvalidOperationException("Failed to open the Word document.");

            var paragraphs = word.MainDocumentPart?.Document.Body?.Descendants<Paragraph>()?.ToList()
                ?? throw new InvalidOperationException("The document body is missing.");

            foreach (var p in paragraphs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var text = TextCleaner.Clean(p.InnerText);
                if (string.IsNullOrWhiteSpace(text))
                    continue;
                contents.Add(text);
            }

            cancellationToken.ThrowIfCancellationRequested();
            return contents;
        }, cancellationToken).ConfigureAwait(false);
    }
}

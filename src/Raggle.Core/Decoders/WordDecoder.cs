using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Raggle.Abstractions.Memory;
using System.Collections.Concurrent;

namespace Raggle.Core.Extractors;

public class WordDecoder : IContentDecoder
{
    public string[] SupportTypes =>
    [
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
        "application/vnd.ms-word.document.macroEnabled.12",
        "application/vnd.ms-word.template.macroEnabled.12"
    ];

    public Task<IEnumerable<DocumentSection>> DecodeAsync(Stream data, CancellationToken cancellationToken = default)
    {
        using var word = WordprocessingDocument.Open(data, false);
        var paragraphs = word.MainDocumentPart?.Document.Body?.Descendants<Paragraph>()
            ?? throw new InvalidOperationException("The document body is missing.");

        var results = new ConcurrentBag<DocumentSection>();
        for (var i = 0; i < paragraphs.Count(); i++)
        {
            var paragraph = paragraphs.ElementAt(i);
            var text = paragraph.InnerText;
            if (!string.IsNullOrWhiteSpace(text))
            {
                results.Add(new DocumentSection(i + 1, text));
            }
        }

        return Task.FromResult(results.AsEnumerable());
    }
}

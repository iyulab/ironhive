using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Raggle.Core.Document;
using System.Collections.Concurrent;
using System.Text;

namespace Raggle.Core.Parsers;

public class MSWordParser : IDocumentParser
{
    public string[] SupportTypes =>
    [
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    ];

    public Task<IEnumerable<DocumentSection>> ParseAsync(Stream data, CancellationToken cancellationToken = default)
    {
        using var word = WordprocessingDocument.Open(data, false);
        var paragraphs = word.MainDocumentPart?.Document.Body?.Descendants<Paragraph>()
            ?? throw new InvalidOperationException("The document body is missing.");

        var results = new ConcurrentBag<DocumentSection>();
        var sb = new StringBuilder();
        foreach (Paragraph p in paragraphs)
        {
            sb.AppendLine(p.InnerText);
        }
        var text = sb.ToString();
        results.Add(new DocumentSection(1, text));

        return Task.FromResult<IEnumerable<DocumentSection>>(results);
    }

}

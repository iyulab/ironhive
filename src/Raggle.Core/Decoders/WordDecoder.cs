using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Raggle.Abstractions.Memory;

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

    public Task<IDocumentContent[]> DecodeAsync(Stream data, CancellationToken cancellationToken = default)
    {
        using var word = WordprocessingDocument.Open(data, false);

        var body = word.MainDocumentPart?.Document.Body
            ?? throw new InvalidOperationException("The document body is missing.");

        var results = new List<IDocumentContent>();
        int pageNumber = 1;
        var paragraphs = body.Descendants<Paragraph>();
        foreach (var element in body.Elements())
        {
            if (element is Paragraph paragraph)
            {
                results.Add(new TextDocumentContent
                {
                    Text = paragraph.InnerText
                });
            }
            else if (element is Table table)
            {
                var tableContent = new TableDocumentContent();
            }
            else if (element is SectionProperties section)
            {
                pageNumber++;
            }
        }
        return Task.FromResult(results.ToArray());
    }
}

using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using Raggle.Core.Document;
using Raggle.Core.Utils;
using System.Collections.Concurrent;

namespace Raggle.Core.Parsers;

public class PresentationParser : IDocumentParser
{
    public string[] SupportTypes =>
    [
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
    ];

    public Task<IEnumerable<DocumentSection>> ParseAsync(Stream data, CancellationToken cancellationToken = default)
    {
        using var presentation = PresentationDocument.Open(data, false)        
            ?? throw new Exception("Failed to open presentation document.");

        var slides = presentation.PresentationPart?.SlideParts.Select(p => p.Slide)
            ?? throw new Exception("Failed to get slide parts.");

        var sections = new ConcurrentBag<DocumentSection>();
        for (var i = 0; i < slides.Count(); i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var texts = slides.ElementAt(i).Descendants<Text>().Select(t => t.Text).ToList();
            var text = string.Join(Environment.NewLine, texts);
            text = TextCleaner.Clean(text);
            sections.Add(new DocumentSection(i + 1, text));
        }
        var result = sections.OrderBy(r => r.Number).AsEnumerable();
        return Task.FromResult(result);
    }
}

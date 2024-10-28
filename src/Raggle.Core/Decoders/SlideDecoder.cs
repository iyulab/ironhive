using DocumentFormat.OpenXml.Packaging;
using Raggle.Abstractions.Memory;
using System.Collections.Concurrent;

namespace Raggle.Core.Decoders;

public class SlideDecoder : IContentDecoder
{
    public string[] SupportTypes =>
    [
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
    ];

    public Task<IEnumerable<DocumentSection>> DecodeAsync(Stream data, CancellationToken cancellationToken = default)
    {
        using var presentation = PresentationDocument.Open(data, false)        
            ?? throw new Exception("Failed to open presentation document.");

        var slides = presentation.PresentationPart?.SlideParts.Select(p => p.Slide)
            ?? throw new Exception("Failed to get slide parts.");

        var results = new ConcurrentBag<DocumentSection>(); // Thread-safe collection
        for (var i = 0; i < slides.Count(); i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var texts = slides.ElementAt(i).Descendants<DocumentFormat.OpenXml.Drawing.Text>().Select(t => t.Text).ToList();
            var text = string.Join(Environment.NewLine, texts);

            results.Add(new DocumentSection(i + 1, text));
        }

        return Task.FromResult<IEnumerable<DocumentSection>>(results);
    }
}

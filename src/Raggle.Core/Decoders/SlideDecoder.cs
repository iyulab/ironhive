using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using Raggle.Abstractions.Memory;

namespace Raggle.Core.Decoders;

public class SlideDecoder : IContentDecoder
{
    public string[] SupportTypes =>
    [
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "application/vnd.openxmlformats-officedocument.presentationml.slideshow",
        "application/vnd.ms-powerpoint.presentation.macroenabled.12",
        "application/vnd.ms-powerpoint.slideshow.macroenabled.12",
        "application/vnd.openxmlformats-officedocument.presentationml.template",
        "application/vnd.ms-powerpoint.template.macroenabled.12",
    ];

    public async Task<IDocumentContent[]> DecodeAsync(Stream data, CancellationToken cancellationToken = default)
    {
        using var presentation = PresentationDocument.Open(data, false);
        if (presentation == null)
            throw new Exception("Failed to open presentation document.");

        var presentationPart = presentation.PresentationPart;
        if (presentationPart?.Presentation == null)
            throw new Exception("Invalid presentation document.");

        var slideParts = presentationPart.SlideParts.ToList();
        var results = new List<IDocumentContent>();

        int slideIndex = 1;
        foreach (var slidePart in slideParts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var text = slidePart.Slide.Descendants<Text>().Select(t => t.Text).ToList();
            var images = slidePart.ImageParts.ToList();
            results.Add(new TextDocumentContent { Text = string.Join(Environment.NewLine, text) });
            slideIndex++;
        }

        return results.ToArray();
    }
}

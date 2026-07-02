using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using IronHive.Abstractions.Files;
using IronHive.Core.Utilities;

namespace IronHive.Core.Files.Parsers;

/// <summary>
/// .pptx 파일을 파싱합니다. 슬라이드별로 <see cref="TextBlock"/>을 생성하고,
/// 슬라이드에 포함된 이미지는 <see cref="ImageBlock"/>으로 추출합니다.
/// </summary>
public class PowerPointParser : IFileParser
{
    /// <inheritdoc />
    public bool CanParse(string fileName)
        => fileName.EndsWith(".pptx", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<IReadOnlyList<FileBlock>> ParseAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var blocks = new List<FileBlock>();

        using var doc = PresentationDocument.Open(data, false);
        var presentationPart = doc.PresentationPart
            ?? throw new InvalidOperationException($"'{fileName}' has no presentation part.");
        var slideIdList = presentationPart.Presentation?.SlideIdList
            ?? throw new InvalidOperationException($"'{fileName}' has no slide list.");

        var slideIndex = 0;
        foreach (var slideId in slideIdList.Elements<SlideId>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            slideIndex++;

            if (slideId.RelationshipId?.Value is not { } rId) continue;
            if (presentationPart.GetPartById(rId) is not SlidePart slidePart) continue;

            var text = TextCleaner.Clean(slidePart.Slide?.InnerText ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(text))
                blocks.Add(new TextBlock { Label = $"{fileName} - Slide {slideIndex}", Text = text });

            blocks.AddRange(ExtractImages(slidePart));
        }

        return Task.FromResult<IReadOnlyList<FileBlock>>(blocks);
    }

    private static List<ImageBlock> ExtractImages(SlidePart slidePart)
    {
        var images = new List<ImageBlock>();
        foreach (var imagePart in slidePart.ImageParts)
        {
            try
            {
                using var ps = imagePart.GetStream();
                using var ms = new MemoryStream();
                ps.CopyTo(ms);
                images.Add(new ImageBlock { MimeType = imagePart.ContentType, Data = ms.ToArray() });
            }
            catch { }
        }
        return images;
    }
}

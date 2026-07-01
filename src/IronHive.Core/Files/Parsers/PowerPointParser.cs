using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using IronHive.Abstractions.Files;
using IronHive.Core.Utilities;

namespace IronHive.Core.Files.Parsers;

/// <summary>
/// .pptx 파일을 파싱합니다. 슬라이드별로 <see cref="TextBlock"/>을 생성합니다.
/// </summary>
public class PowerPointParser : IFileParser
{
    /// <inheritdoc />
    public bool CanParse(string fileName, string? mimeType = null)
        => fileName.EndsWith(".pptx", StringComparison.OrdinalIgnoreCase)
        || mimeType?.Equals("application/vnd.openxmlformats-officedocument.presentationml.presentation", StringComparison.OrdinalIgnoreCase) == true;

    /// <inheritdoc />
    public Task<IReadOnlyList<FileBlock>> ParseAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var blocks = new List<FileBlock>();
        try
        {
            using var doc = PresentationDocument.Open(data, false);
            var presentationPart = doc.PresentationPart;
            if (presentationPart?.Presentation?.SlideIdList is null)
                return Task.FromResult<IReadOnlyList<FileBlock>>(blocks);

            var slideIndex = 0;
            foreach (var slideId in presentationPart.Presentation.SlideIdList.Elements<SlideId>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                slideIndex++;

                if (slideId.RelationshipId?.Value is not { } rId) continue;
                if (presentationPart.GetPartById(rId) is not SlidePart slidePart) continue;

                var text = TextCleaner.Clean(slidePart.Slide?.InnerText ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(text))
                    blocks.Add(new TextBlock { Label = $"{fileName} - Slide {slideIndex}", Text = text });
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

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using IronHive.Abstractions.Files;
using IronHive.Core.Utilities;

namespace IronHive.Core.Files.Parsers;

/// <summary>
/// .docx 파일을 파싱합니다. 단락은 텍스트로, 표는 탭 구분 텍스트로, 인라인 이미지는
/// <see cref="ImageBlock"/>으로 추출하며 문서 내 요소 순서를 유지합니다.
/// </summary>
public class WordParser : IFileParser
{
    /// <inheritdoc />
    public bool CanParse(string fileName)
        => fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<IReadOnlyList<FileBlock>> ParseAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var blocks = new List<FileBlock>();

        using var doc = WordprocessingDocument.Open(data, false);
        var mainPart = doc.MainDocumentPart
            ?? throw new InvalidOperationException($"'{fileName}' has no main document part.");
        var body = mainPart.Document?.Body
            ?? throw new InvalidOperationException($"'{fileName}' has no document body.");

        var textBuffer = new System.Text.StringBuilder();
        foreach (var element in body.ChildElements)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (element is Paragraph para)
            {
                textBuffer.AppendLine(para.InnerText);

                // 단락 안에 이미지가 있으면, 여기까지 누적된 텍스트를 먼저 flush해서
                // 문서 내 텍스트/이미지 순서를 유지합니다.
                var images = ExtractInlineImages(para, mainPart);
                if (images.Count > 0)
                {
                    FlushText(blocks, textBuffer, fileName);
                    blocks.AddRange(images);
                }
            }
            else if (element is Table table)
            {
                FlushText(blocks, textBuffer, fileName);
                var tableText = ExtractTableText(table);
                if (!string.IsNullOrWhiteSpace(tableText))
                    blocks.Add(new TextBlock { Label = $"{fileName} - Table", Text = tableText });
            }
        }
        FlushText(blocks, textBuffer, fileName);

        return Task.FromResult<IReadOnlyList<FileBlock>>(blocks);
    }

    private static void FlushText(List<FileBlock> blocks, System.Text.StringBuilder sb, string fileName)
    {
        var text = TextCleaner.Clean(sb.ToString());
        if (!string.IsNullOrWhiteSpace(text))
            blocks.Add(new TextBlock { Label = fileName, Text = text });
        sb.Clear();
    }

    private static string ExtractTableText(Table table)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var row in table.Elements<TableRow>())
        {
            var cells = row.Elements<TableCell>().Select(c => c.InnerText.Trim());
            sb.AppendLine(string.Join('\t', cells));
        }
        return sb.ToString().Trim();
    }

    private static List<ImageBlock> ExtractInlineImages(Paragraph para, MainDocumentPart mainPart)
    {
        var images = new List<ImageBlock>();
        foreach (var blip in para.Descendants<DocumentFormat.OpenXml.Drawing.Blip>())
        {
            var embedId = blip.Embed?.Value;
            if (string.IsNullOrEmpty(embedId)) continue;
            if (mainPart.GetPartById(embedId) is not ImagePart imagePart) continue;

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

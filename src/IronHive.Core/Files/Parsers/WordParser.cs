using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using IronHive.Abstractions.Files;
using IronHive.Core.Utilities;

namespace IronHive.Core.Files.Parsers;

/// <summary>
/// .docx 파일을 파싱합니다. 단락은 텍스트로, 표는 탭 구분 텍스트로 추출하며
/// 문서 내 요소 순서를 유지합니다.
/// </summary>
public class WordParser : IFileParser
{
    /// <inheritdoc />
    public bool CanParse(string fileName, string? mimeType = null)
        => fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase)
        || mimeType?.Equals("application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.OrdinalIgnoreCase) == true;

    /// <inheritdoc />
    public Task<IReadOnlyList<FileBlock>> ParseAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var blocks = new List<FileBlock>();
        try
        {
            using var doc = WordprocessingDocument.Open(data, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body is null)
                return Task.FromResult<IReadOnlyList<FileBlock>>(blocks);

            var textBuffer = new System.Text.StringBuilder();
            foreach (var element in body.ChildElements)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (element is Paragraph para)
                {
                    textBuffer.AppendLine(para.InnerText);
                }
                else if (element is Table table)
                {
                    // 표 앞에 누적된 단락을 먼저 flush해서 순서를 보존합니다.
                    FlushText(blocks, textBuffer, fileName);
                    var tableText = ExtractTableText(table);
                    if (!string.IsNullOrWhiteSpace(tableText))
                        blocks.Add(new TextBlock { Label = $"{fileName} - Table", Text = tableText });
                }
            }
            FlushText(blocks, textBuffer, fileName);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch { }

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
}

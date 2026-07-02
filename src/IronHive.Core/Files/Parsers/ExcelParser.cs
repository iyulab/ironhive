using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using IronHive.Abstractions.Files;

namespace IronHive.Core.Files.Parsers;

/// <summary>
/// .xlsx 파일을 파싱합니다. 시트별로 <see cref="TextBlock"/>을 생성하며
/// 셀 값은 탭 구분 텍스트로 추출합니다.
/// </summary>
public class ExcelParser : IFileParser
{
    /// <inheritdoc />
    public bool CanParse(string fileName)
        => fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<IReadOnlyList<FileBlock>> ParseAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var blocks = new List<FileBlock>();

        using var doc = SpreadsheetDocument.Open(data, false);
        var workbookPart = doc.WorkbookPart
            ?? throw new InvalidOperationException($"'{fileName}' has no workbook part.");

        var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;
        // WorkbookPart.WorksheetParts는 순서가 보장되지 않으므로 Workbook.Sheets를 통해 순서대로 접근합니다.
        var sheets = workbookPart.Workbook?.Sheets?.Elements<Sheet>().ToList() ?? [];

        foreach (var sheet in sheets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (sheet.Id?.Value is not { } relationshipId) continue;
            if (workbookPart.GetPartById(relationshipId) is not WorksheetPart worksheetPart) continue;

            var sheetData = worksheetPart.Worksheet?.Elements<SheetData>().FirstOrDefault();
            if (sheetData is null) continue;

            var sb = new System.Text.StringBuilder();
            foreach (var row in sheetData.Elements<Row>())
            {
                var cells = row.Elements<Cell>().Select(c => GetCellValue(c, sharedStrings));
                sb.AppendLine(string.Join('\t', cells));
            }

            var text = sb.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(text))
                blocks.Add(new TextBlock { Label = $"{fileName} - {sheet.Name?.Value ?? "Sheet"}", Text = text });
        }

        return Task.FromResult<IReadOnlyList<FileBlock>>(blocks);
    }

    private static string GetCellValue(Cell cell, SharedStringTable? sharedStrings)
    {
        var value = cell.CellValue?.Text ?? string.Empty;
        // SharedString 타입은 값이 인덱스이므로 SharedStringTable에서 실제 문자열을 조회합니다.
        if (cell.DataType?.Value == CellValues.SharedString
            && sharedStrings is not null
            && int.TryParse(value, out var index))
        {
            return sharedStrings.ElementAt(index).InnerText;
        }
        return value;
    }
}

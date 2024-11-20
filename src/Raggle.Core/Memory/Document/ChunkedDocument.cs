namespace Raggle.Core.Memory.Document;

public class ChunkedDocument
{
    public int Index { get; set; }

    public string SourceFileName { get; set; } = string.Empty;

    public string SourceSection { get; set; } = string.Empty;

    public string RawText { get; set; } = string.Empty;

    public string? SummarizedText { get; set; } = string.Empty;

    public IEnumerable<QAPair>? ExtractedQAPairs { get; set; }
}

public class QAPair
{
    public int Index { get; set; }

    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;
}

namespace Raggle.Core.Document;

public class DocumentChunk
{
    public string SourceFileName { get; set; } = string.Empty;

    public int SectionNumber { get; set; }

    public int ChunkIndex { get; set; }

    public string RawText { get; set; } = string.Empty;

    public string? SummarizedText { get; set; } = string.Empty;

    public IEnumerable<string>? ExtractedQuestions { get; set; }
}

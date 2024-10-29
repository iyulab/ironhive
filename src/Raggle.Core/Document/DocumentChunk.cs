namespace Raggle.Core.Document;

public class DocumentChunk
{
    public int SectionNumber { get; set; }

    public int ChunkNumber { get; set; }

    public string Text { get; set; } = string.Empty;

    public IEnumerable<QAItem>? QAItems { get; set; }
}

public class QAItem
{
    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;
}

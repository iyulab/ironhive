namespace Raggle.Abstractions.Memory;

public class MemoryRecord
{
    public Guid DocumentID { get; set; } = Guid.NewGuid();

    public string? EmbeddingModel { get; set; }

    public string? ChatCompletionModel { get; set; }

    public string? FileName { get; set; }

    public string? FileType { get; set; }

    public long Size { get; set; }

    public int Partition { get; set; }

    public int Segment { get; set; }

    public string? Content { get; set; }

    public float[] Embedding { get; set; } = [];

    public string[] Tags { get; set; } = [];
}

public class DocumentRecord
{
    public Guid DocumentID { get; set; } = Guid.NewGuid();

    public string? EmbeddingModel { get; set; }

    public string? ChatCompletionModel { get; set; }

    public string? FileName { get; set; }

    public string? FileType { get; set; }

    public long Size { get; set; }

    public int Partitions { get; set; }

    public int Segments { get; set; }
}
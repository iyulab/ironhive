namespace Raggle.Abstractions.Memory;

public class DocumentSource
{
    public required string FileName { get; set; }

    public long? ContentSize { get; set; } = 0;

    public string? MimeType { get; set; }

    public DocumentSegment? Segments { get; set; }
}

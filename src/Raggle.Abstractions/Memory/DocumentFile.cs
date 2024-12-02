namespace Raggle.Abstractions.Memory;

public class DocumentFile
{
    public required DocumentSource Source { get; set; }

    public string? SegmentUnit { get; set; }

    public IEnumerable<DocumentSegment> Segments { get; set; }
}

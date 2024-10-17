namespace Raggle.Abstractions.Memory;

public class StructuredDocument
{
    public required string DocumentId { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long Size { get; set; }
    public ICollection<IDocumentContent>? Contents { get; set; }
}

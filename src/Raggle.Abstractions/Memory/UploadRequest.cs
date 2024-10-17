namespace Raggle.Abstractions.Memory;

public class UploadRequest
{
    public required string CollectionName { get; set; }

    public required string DocumentId { get; set; }

    public required string FilePath { get; set; }

    public required Stream Content { get; set; }

    public string[] Tags { get; set; } = [];
}

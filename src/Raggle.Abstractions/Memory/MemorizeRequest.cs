namespace Raggle.Abstractions.Memory;

public class MemorizeRequest
{
    public required string CollectionName { get; set; }

    public required string DocumentId { get; set; }

    public required string[] Steps { get; set; }

    public UploadFile? File { get; set; }

    public string[]? Tags { get; set; }
}

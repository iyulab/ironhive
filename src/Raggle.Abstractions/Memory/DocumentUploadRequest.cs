namespace Raggle.Abstractions.Memory;

public class DocumentUploadRequest
{
    public required string FileName { get; set; }

    public required Stream Content { get; set; }

    public string[]? Tags { get; set; }
}

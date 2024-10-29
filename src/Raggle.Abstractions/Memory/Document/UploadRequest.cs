namespace Raggle.Abstractions.Memory.Document;

public class UploadRequest
{
    public required string FileName { get; set; }

    public required Stream Content { get; set; }

    public string[]? Tags { get; set; }
}

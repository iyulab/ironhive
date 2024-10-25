namespace Raggle.Abstractions.Memory;

public class UploadRequest
{
    public required string FileName { get; set; }

    public required Stream Content { get; set; }
}

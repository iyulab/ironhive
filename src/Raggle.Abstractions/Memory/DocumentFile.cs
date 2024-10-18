namespace Raggle.Abstractions.Memory;

public class UploadFile
{
    public required string FileName { get; set; }

    public required string ContentType { get; set; }

    public required Stream Content { get; set; }
}

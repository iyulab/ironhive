namespace Raggle.Abstractions.Memory;

public class UploadRequest
{
    public string FileName { get; set; }

    public long Size { get; set; }

    public Stream Stream { get; set; }
}

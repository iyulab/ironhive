namespace Raggle.Abstractions.Memory;

public class FileReadRequest
{
    public required string FileStorageType { get; set; }
    public required string FileIdentifier { get; set; }
}

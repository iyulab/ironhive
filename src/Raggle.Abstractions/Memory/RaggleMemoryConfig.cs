namespace Raggle.Abstractions.Memory;

public class RaggleMemoryConfig
{
    public required object DocumentStorageServiceKey { get; set; }

    public required object VectorStorageServiceKey { get; set; }
}

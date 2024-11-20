namespace Raggle.Abstractions.Memory;

public class RaggleMemoryConfig
{
    public required object DocumentStorageServiceKey { get; set; }

    public required object VectorDBServiceKey { get; set; }

    public required object EmbeddingServiceKey { get; set; }

    public required string EmbeddingModel { get; set; }

    public object? ChatCompletionServiceKey { get; set; }

    public string? ChatCompletionModel { get; set; }
}

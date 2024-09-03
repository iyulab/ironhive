namespace Raggle.Services;

public class ServiceOptions
{
    public string OpenAIKey { get; set; } = string.Empty;
    public string ModelDirectory { get; set; } = string.Empty;
    public string MemoryDirectory { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = string.Empty;
}

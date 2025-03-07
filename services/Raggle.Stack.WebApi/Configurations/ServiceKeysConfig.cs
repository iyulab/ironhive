namespace Raggle.Stack.WebApi.Configurations;

public class ServiceKeysConfig
{
    // For AI services
    public string OpenAI { get; set; } = "openai";

    public string Anthrophic { get; set; } = "anthropic";

    public string Ollama { get; set; } = "ollama";

    // For pipeline handlers
    public string Decoding { get; set; } = "extract";

    public string Chunking { get; set; } = "chunk";

    public string Summarizing { get; set; } = "summary";

    public string Dialogue { get; set; } = "dialogue";

    public string Embeddings { get; set; } = "embeddings";

    // For tool services
    public string VectorSearch { get; set; } = "vector_search";
}

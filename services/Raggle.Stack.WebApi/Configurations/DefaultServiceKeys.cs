namespace Raggle.Stack.WebApi.Configurations;

public static class DefaultServiceKeys
{
    // For AI services
    public static string OpenAI { get; set; } = "openai";

    public static string Anthrophic { get; set; } = "anthropic";

    public static string Ollama { get; set; } = "ollama";

    // For pipeline handlers
    public static string Decoding { get; set; } = "extract";

    public static string Chunking { get; set; } = "chunk";

    public static string Summarizing { get; set; } = "summary";

    public static string Dialogue { get; set; } = "dialogue";

    public static string Embeddings { get; set; } = "embeddings";

    // For tool services
    public static string VectorSearch { get; set; } = "vector_search";
}

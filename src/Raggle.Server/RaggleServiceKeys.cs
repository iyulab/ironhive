namespace Raggle.Server;

public static class RaggleServiceKeys
{
    // For AI services
    public const string OpenAI = "openai";

    public const string Anthrophic = "anthrophic";

    public const string Ollama = "ollama";

    // For pipeline handlers
    public const string Decoding = "extract";

    public const string Chunking = "chunk";

    public const string Summarizing = "summary";

    public const string Dialogue = "dialogue";

    public const string Embeddings = "embeddings";

    // For tool services
    public const string VectorSearch = "vector_search";

}

using Raggle.Abstractions;
using Raggle.Connector.Ollama.Configurations;

namespace Raggle.Connector.Ollama;

public static class OllamaRaggleBuilderExtension
{
    public static void AddOllamaServices(
        this IRaggleBuilder builder,
        object key,
        OllamaConfig config)
    {
        builder.AddChatCompletionService(key, new OllamaChatCompletionService(config));
        builder.AddEmbeddingService(key, new OllamaEmbeddingService(config));
    }
}

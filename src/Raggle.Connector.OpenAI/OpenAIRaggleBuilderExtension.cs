using Raggle.Abstractions;
using Raggle.Connector.OpenAI.Configurations;

namespace Raggle.Connector.OpenAI;

public static class OpenAIRaggleBuilderExtension
{
    public static void AddOpenAIServices(
        this IRaggleBuilder builder,
        object key,
        OpenAIConfig config)
    {
        builder.AddChatCompletionService(key, new OpenAIChatCompletionService(config));
        builder.AddEmbeddingService(key, new OpenAIEmbeddingService(config));
    }
}

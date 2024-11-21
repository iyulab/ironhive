using Raggle.Driver.Anthropic;
using Raggle.Driver.Anthropic.Configurations;
using Raggle.Driver.Ollama;
using Raggle.Driver.Ollama.Configurations;
using Raggle.Driver.OpenAI;
using Raggle.Driver.OpenAI.Configurations;
using Raggle.Driver.LocalDisk;
using Raggle.Driver.LiteDB;
using Raggle.Abstractions.Memory;
using Raggle.Core;

namespace Raggle.Server.WebApi;

public static partial class ServiceCollectionExtention
{
    public static IServiceCollection AddRaggle(this IServiceCollection services)
    {
        services.AddRaggleServices();
        var memory_config = new RaggleMemoryConfig
        {
            DocumentStorageServiceKey = "local-disk",
            VectorStorageServiceKey = "lite-db",
        };
        var builder = new RaggleBuilder(services);
        var raggle = builder.Build(memory_config);
        return services.AddSingleton(raggle);
    }

    public static IServiceCollection AddRaggleServices(this IServiceCollection services)
    {
        var openai_config = new OpenAIConfig
        {
            ApiKey = "",
        };
        var anthropic_config = new AnthropicConfig
        {
            ApiKey = "",
        };
        var ollama_config = new OllamaConfig
        {
            EndPoint = "https://localhost:11434",
        };
        var localDisk_config = new LocalDiskConfig
        {
            DirectoryPath = @"C:\temp\documents",
        };
        var liteDb_config = new LiteDBConfig
        {
            DatabasePath = @"C:\temp\vector.db",
        };
        var memory_config = new RaggleMemoryConfig
        {
            DocumentStorageServiceKey = "local-disk",
            VectorStorageServiceKey = "lite-db",
        };

        return services.AddOpenAIServices("openai", openai_config)
                       .AddAnthropicServices("anthropic", anthropic_config)
                       .AddOllamaServices("ollama", ollama_config)
                       .AddLocalDiskServices("local-disk", localDisk_config)
                       .AddLiteDBServices("lite-db", liteDb_config);
    }
}

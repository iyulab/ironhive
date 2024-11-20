using Raggle.Connector.Anthropic;
using Raggle.Connector.Anthropic.Configurations;
using Raggle.Connector.Ollama;
using Raggle.Connector.Ollama.Configurations;
using Raggle.Connector.OpenAI;
using Raggle.Connector.OpenAI.Configurations;
using Raggle.Core;
using Raggle.DocumentStorage.LocalDisk;
using Raggle.VectorDB.LiteDB;

namespace Raggle.Server.WebApi;

public static class ServiceCollectionExtention
{
    public static IServiceCollection AddRaggle(this IServiceCollection services)
    {
        var builder = new RaggleBuilder(services);
        builder.AddRaggleServices();
        builder.AddRaggleMemoryHandlers();
        builder.AddRaggleToolKits();
        var raggle = builder.Build();
        services.AddSingleton(raggle);
        return services;
    }

    private static void AddRaggleServices(this RaggleBuilder builder)
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

        builder.AddOpenAIServices("openai", openai_config);
        builder.AddAnthropicServices("anthropic", anthropic_config);
        builder.AddOllamaServices("ollama", ollama_config);

        var local_disk_config = new LocalDiskStorageConfig
        {
            DirectoryPath = @"C:\temp\documents",
        };
        var docs = new LocalDiskDocumentStorage(local_disk_config);
        builder.AddDocumentStorage("disk-doc", docs);

        var vector_config = new LiteDBConfig
        {
            DatabasePath = @"C:\temp\vector.db",
        };
        var vectors = new LiteDBVectorStorage(vector_config);
        builder.AddVectorStorage("lite-db", vectors);
    }

    private static void AddRaggleMemoryHandlers(this RaggleBuilder builder)
    {
        //var orchestrator = new PipelineOrchestrator(docs);

        //orchestrator.TryAddHandler("decoing", new DocumentDecodingHandler(docs, 
        //    [
        //        new PPTDecoder(),
        //        new PDFDecoder(),
        //        new WordDecoder(),
        //        new TextDecoder(),
        //    ]));
        //orchestrator.TryAddHandler("chunking", new TextChunkingHandler(docs));
        //orchestrator.TryAddHandler("generate", new GenerateQAPairsHandler(docs,openai_chat, new GenerateQAPairsHandlerOptions
        //{
        //    Provider = "openai",
        //    Model = "gpt-4o-mini",
        //}));
        //orchestrator.TryAddHandler("embedding", new TextEmbeddingHandler(docs,vectors,openai_embedding, "text-embedding-ada-002"));
        //var memory = new RaggleMemory(docs, vectors, openai_embedding, "text-embedding-ada-002", orchestrator);
        //services.AddSingleton<IRaggleMemory>(memory);
    }

    private static void AddRaggleToolKits(this RaggleBuilder builder)
    {

    }
}

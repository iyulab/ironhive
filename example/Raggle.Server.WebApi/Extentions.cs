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
using Raggle.Server.WebApi.Models;
using Raggle.Abstractions;
using Raggle.Abstractions.Extensions;
using Raggle.Core.Memory.Decoders;
using Raggle.Core.Memory.Handlers;

namespace Raggle.Server.WebApi;

public static partial class ServiceCollectionExtention
{
    public static IServiceCollection AddRaggle(this IServiceCollection services, RaggleServiceConfig config)
    {
        services.AddRaggleServices(config);

        //services.AddDocumentDecoder<WordDecoder>("word");
        //services.AddDocumentDecoder<TextDecoder>("text");
        //services.AddDocumentDecoder<PPTDecoder>("ppt");
        //services.AddDocumentDecoder<PDFDecoder>("pdf");

        //services.AddPipelineHandler<DocumentDecodingHandler>("decoding");
        //services.AddPipelineHandler<TextChunkingHandler>("chunk");
        //services.AddPipelineHandler<GenerateSummarizedTextHandler>("summarize");
        //services.AddPipelineHandler<GenerateQAPairsHandler>("qa");
        //services.AddPipelineHandler<TextEmbeddingHandler>("embed");

        var memory_config = new RaggleMemoryConfig
        {
            DocumentStorageServiceKey = RaggleServiceKeys.LocalDisk,
            VectorStorageServiceKey = RaggleServiceKeys.LiteDB,
        };
        var builder = new RaggleBuilder(services);

        var raggle = builder.Build(memory_config);
        return services.AddSingleton(raggle);
    }

    public static IServiceCollection AddRaggleServices(this IServiceCollection services, RaggleServiceConfig config)
    {
        var openai_config = new OpenAIConfig
        {
            ApiKey = config.OpenAIKey,
        };
        var anthropic_config = new AnthropicConfig
        {
            ApiKey = config.AnthropicKey,
        };
        var ollama_config = new OllamaConfig
        {
            EndPoint = config.OllamaEndpoint,
        };
        var localDisk_config = new LocalDiskConfig
        {
            DirectoryPath = config.DocumentStoragePath,
        };
        var liteDb_config = new LiteDBConfig
        {
            DatabasePath = config.VectorStoragePath,
        };

        return services.AddOpenAIServices(RaggleServiceKeys.OpenAI, openai_config)
                       .AddAnthropicServices(RaggleServiceKeys.Anthropic, anthropic_config)
                       .AddOllamaServices(RaggleServiceKeys.Ollama, ollama_config)
                       .AddLocalDiskServices(RaggleServiceKeys.LocalDisk, localDisk_config)
                       .AddLiteDBServices(RaggleServiceKeys.LiteDB, liteDb_config);
    }
}

using Raggle.Driver.Anthropic;
using Raggle.Driver.Ollama;
using Raggle.Driver.OpenAI;
using Raggle.Server.WebApi.Configuration;
using Raggle.Abstractions.Extensions;
using Raggle.Core.Memory.Handlers;
using Raggle.Driver.LiteDB;
using Raggle.Driver.Qdrant;
using Raggle.Driver.LocalDisk;
using Raggle.Driver.AzureBlob;
using Raggle.Server.WebApi.Data;
using Microsoft.EntityFrameworkCore;
using Raggle.Core.Memory.Decoders;
using Raggle.Abstractions;

namespace Raggle.Server.WebApi;

public static partial class IServiceCollectionExtention
{
    public static IServiceCollection AddRaggleServices(this IServiceCollection services, RaggleConfig config)
    {
        services.SetRaggleDB(config)
                .AddAIServices(config)
                .SetVectorStorage(config)
                .SetDocumentStorage(config)
                .AddDocumentDecoders()
                .AddPipelineHandlers();
        services.AddSingleton<IRaggle, Core.Raggle>();
        return services;
    }

    public static IServiceCollection SetRaggleDB(this IServiceCollection services, RaggleConfig config)
    {
        services.AddDbContext<AppDbContext>((options) =>
        {
            options.UseSqlite(config.ConnectionString);
            options.AddInterceptors(new AppDbIntercepter());
        });
        return services;
    }

    public static IServiceCollection AddAIServices(this IServiceCollection services, RaggleConfig config)
    {
        return services.AddOpenAIServices(AIServiceKeys.OpenAI.ToString(), config.AIService.OpenAI)
                       .AddAnthropicServices(AIServiceKeys.Anthropic.ToString(), config.AIService.Anthropic)
                       .AddOllamaServices(AIServiceKeys.Ollama.ToString(), config.AIService.Ollama);
    }

    public static IServiceCollection SetVectorStorage(this IServiceCollection services, RaggleConfig config)
    {
        switch (config.VectorStorage.Type)
        {
            case VectorStorageTypes.LiteDB:
                services.SetLiteDBVectorStorage(config.VectorStorage.LiteDB);
                break;
            case VectorStorageTypes.Qdrant:
                services.SetQdrantVectorStorage(config.VectorStorage.Qdrant);
                break;
            default:
                throw new ArgumentException($"Invalid vector storage type: {config.VectorStorage.Type}");
        }

        return services;
    }

    public static IServiceCollection SetDocumentStorage(this IServiceCollection services, RaggleConfig config)
    {
        switch (config.DocumentStorage.Type)
        {
            case DocumentStorageTypes.LocalDisk:
                services.SetLocalDiskDocumentStorage(config.DocumentStorage.LocalDisk);
                break;
            case DocumentStorageTypes.AzureBlob:
                services.SetAzureBlobDocumentStorage(config.DocumentStorage.AzureBlob);
                break;
            default:
                throw new ArgumentException($"Invalid vector storage type: {config.VectorStorage.Type}");
        }

        return services;
    }

    public static IServiceCollection AddDocumentDecoders(this IServiceCollection services)
    {
        return services.AddDocumentDecoder<WordDecoder>()
                       .AddDocumentDecoder<TextDecoder>()
                       .AddDocumentDecoder<PPTDecoder>()
                       .AddDocumentDecoder<PDFDecoder>();
    }

    public static IServiceCollection AddPipelineHandlers(this IServiceCollection services)
    {
        return services.AddPipelineHandler<DecodingHandler>(HandlerServiceKeys.Decoding.ToString())
                       .AddPipelineHandler<ChunkingHandler>(HandlerServiceKeys.Chunking.ToString())
                       .AddPipelineHandler<SummarizationHandler>(HandlerServiceKeys.Summarization.ToString())
                       .AddPipelineHandler<GenerateQAHandler>(HandlerServiceKeys.GenerateQA.ToString())
                       .AddPipelineHandler<EmbeddingsHandler>(HandlerServiceKeys.Embeddings.ToString());
    }
}

public static partial class IServiceProviderExtension
{
    public static bool EnsureRaggleDB(this IServiceProvider provider)
    {
        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return context.Database.EnsureCreated();
        }
    }
}

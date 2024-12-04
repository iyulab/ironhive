using Raggle.Driver.Anthropic;
using Raggle.Driver.Ollama;
using Raggle.Driver.OpenAI;
using Raggle.Abstractions.Extensions;
using Raggle.Core.Memory.Handlers;
using Raggle.Driver.LiteDB;
using Raggle.Driver.Qdrant;
using Raggle.Driver.LocalDisk;
using Raggle.Driver.AzureBlob;
using Microsoft.EntityFrameworkCore;
using Raggle.Core.Memory.Decoders;
using Raggle.Abstractions;
using Raggle.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Raggle.Server.Configurations;
using Raggle.Server.Data;
using Raggle.Server.Services;

namespace Raggle.Server;

public static partial class IServiceCollectionExtentions
{
    public static IServiceCollection AddRaggleServices(
        this IServiceCollection services, 
        RaggleConfig config)
    {
        services.SetDatabaseService(config.Database)
                .SetStorageServices(config.Storages)
                .AddKeyedServices(config.Services)
                .AddDefaultDocumentDecoders()
                .AddDefaultPipelineHandlers(config.Services)

                .SetJsonDocumentManager(); // ?? 삭제 또는 변경 TODO

        services.AddSingleton<IRaggle, Core.Raggle>();
        return services;
    }

    public static IServiceCollection SetDatabaseService(
        this IServiceCollection services,
        RaggleDatabaseConfig config)
    {
        services.AddDbContext<RaggleDbContext>(options =>
        {
            if (config.Type == DatabaseTypes.Sqlite)
            {
                options.UseSqlite(config.Sqlite.ConnectionString);
                options.AddInterceptors(new SqliteDbInterceptor());
            }
            else if (config.Type == DatabaseTypes.SqlServer)
            {
                options.UseSqlServer(config.SqlServer.ConnectionString);
            }
            else if (config.Type == DatabaseTypes.Cosmos)
            {
                options.UseCosmos(config.Cosmos.ConnectionString, config.Cosmos.DatabaseName);
            }
            else if (config.Type == DatabaseTypes.MySql)
            {
                var version = ServerVersion.AutoDetect(config.MySql.ConnectionString);
                options.UseMySql(config.MySql.ConnectionString, version);
            }
            else if (config.Type == DatabaseTypes.Oracle)
            {
                options.UseOracle(config.Oracle.ConnectionString);
            }
            else if (config.Type == DatabaseTypes.PostgreSQL)
            {
                options.UseNpgsql(config.PostgreSQL.ConnectionString);
            }
            else if (config.Type == DatabaseTypes.MongoDB)
            {
                options.UseMongoDB(config.MongoDB.ConnectionString, config.MongoDB.DatabaseName);
            }
            else
            {
                throw new ArgumentException($"Invalid database type: {config.Type}");
            }
        });

        services.AddScoped<MemoryService>();
        services.AddScoped<AssistantService>();
        return services;
    }

    public static IServiceCollection SetStorageServices(
        this IServiceCollection services, 
        RaggleStorageConfig config)
    {
        switch (config.Vectors.Type)
        {
            case VectorStorageTypes.LiteDB:
                services.SetLiteDBVectorStorage(config.Vectors.LiteDB);
                break;
            case VectorStorageTypes.Qdrant:
                services.SetQdrantVectorStorage(config.Vectors.Qdrant);
                break;
            default:
                throw new ArgumentException($"Invalid vector storage type: {config.Vectors.Type}");
        }

        switch (config.Documents.Type)
        {
            case DocumentStorageTypes.LocalDisk:
                services.SetLocalDiskDocumentStorage(config.Documents.LocalDisk);
                break;
            case DocumentStorageTypes.AzureBlob:
                services.SetAzureBlobDocumentStorage(config.Documents.AzureBlob);
                break;
            default:
                throw new ArgumentException($"Invalid vector storage type: {config.Documents.Type}");
        }

        return services;
    }

    public static IServiceCollection AddKeyedServices(
        this IServiceCollection services,
        RaggleKeyedServiceConfig config)
    {
        if (config.AIProviders.Ollama.Validate())
        {
            services.AddOllamaServices(
                serviceKey: config.AIProviders.Ollama.ServiceKey, 
                config: config.AIProviders.Ollama.Value);
        }
        if (config.AIProviders.OpenAI.Validate())
        {
            services.AddOpenAIServices(
                serviceKey: config.AIProviders.OpenAI.ServiceKey,
                config: config.AIProviders.OpenAI.Value);
        }
        if (config.AIProviders.Anthropic.Validate())
        {
            services.AddAnthropicServices(
                serviceKey: config.AIProviders.Anthropic.ServiceKey,
                config: config.AIProviders.Anthropic.Value);
        }

        if (config.ToolKits.VectorSearch.Validate())
        {
            // TODO: Add toolkit
        }

        return services;
    }

    public static IServiceCollection AddDefaultDocumentDecoders(
        this IServiceCollection services)
    {
        return services.AddDocumentDecoder<WordDecoder>()
                       .AddDocumentDecoder<TextDecoder>()
                       .AddDocumentDecoder<PPTDecoder>()
                       .AddDocumentDecoder<PDFDecoder>();
    }

    public static IServiceCollection AddDefaultPipelineHandlers(
        this IServiceCollection services,
        RaggleKeyedServiceConfig config)
    {
        return services.AddPipelineHandler<DecodingHandler>(DefaultServiceKeys.Decoding)
                       .AddPipelineHandler<ChunkingHandler>(DefaultServiceKeys.Chunking)
                       .AddPipelineHandler<SummarizationHandler>(DefaultServiceKeys.Summarizing)
                       .AddPipelineHandler<GenerateDialogueHandler>(DefaultServiceKeys.GenDialogue)
                       .AddPipelineHandler<EmbeddingsHandler>(DefaultServiceKeys.Embeddings);
    }
}

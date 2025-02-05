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
using Raggle.Server.Data;
using Raggle.Server.Services;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory;
using Raggle.Server.Configurations.Models;
using Microsoft.AspNetCore.Http;
using Raggle.Server.Tools;

namespace Raggle.Server.Extensions;

public static partial class IServiceCollectionExtentions
{
    public static IServiceCollection AddRaggleServices(
        this IServiceCollection services,
        RaggleConfig config)
    {
        services.SetDatabaseService(config.Database)
                .SetStorageServices(config.Storages)
                .AddAIServices(config.Services)
                .AddDefaultToolServices()
                .AddDefaultDocumentDecoders()
                .AddDefaultPipelineHandlers();

        services.AddSingleton<IRaggle, Core.Raggle>();
        services.AddSingleton<IRaggleMemory, RaggleMemory>();

        
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

        services.AddScoped<MemoryService>((provider) =>
        {
            var dbContext = provider.GetRequiredService<RaggleDbContext>();
            var memory = provider.GetRequiredService<IRaggleMemory>();
            var http = provider.GetRequiredService<IHttpContextAccessor>();
            var serviceId = http.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "serviceId")?.Value
                ?? string.Empty;
            return new MemoryService(dbContext, memory, serviceId);
        });
        services.AddScoped<AssistantService>((provider) => 
        {
            var dbContext = provider.GetRequiredService<RaggleDbContext>();
            var raggle = provider.GetRequiredService<IRaggle>();
            var http = provider.GetRequiredService<IHttpContextAccessor>();
            var serviceId = http.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "serviceId")?.Value
                ?? string.Empty;
            return new AssistantService(dbContext, raggle, serviceId);
        });
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

    public static IServiceCollection AddAIServices(
        this IServiceCollection services,
        RaggleAIConfig config)
    {
        services.AddOpenAIServices(RaggleServiceKeys.OpenAI, config.OpenAI);
        services.AddAnthropicServices(RaggleServiceKeys.Anthrophic, config.Anthropic);
        services.AddOllamaServices(RaggleServiceKeys.Ollama, config.Ollama);
        return services;
    }

    public static IServiceCollection AddDefaultToolServices(
        this IServiceCollection services)
    {
        services.AddToolService<VectorSearchTool>(RaggleServiceKeys.VectorSearch);
        services.AddToolService<PythonTool>("python-interpreter");
        return services;
    }

    public static IServiceCollection AddDefaultPipelineHandlers(
        this IServiceCollection services)
    {
        return services.AddPipelineHandler<DecodingHandler>(RaggleServiceKeys.Decoding)
                       .AddPipelineHandler<ChunkingHandler>(RaggleServiceKeys.Chunking)
                       .AddPipelineHandler<SummaryHandler>(RaggleServiceKeys.Summarizing)
                       .AddPipelineHandler<DialogueHandler>(RaggleServiceKeys.Dialogue)
                       .AddPipelineHandler<EmbeddingsHandler>(RaggleServiceKeys.Embeddings);
    }

    public static IServiceCollection AddDefaultDocumentDecoders(
        this IServiceCollection services)
    {
        return services.AddDocumentDecoder<WordDecoder>()
                       .AddDocumentDecoder<TextDecoder>()
                       .AddDocumentDecoder<PPTDecoder>()
                       .AddDocumentDecoder<PDFDecoder>();
    }
}

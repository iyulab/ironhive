using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace IronHive.Stack.EFCore.Extensions;

public static class ServiceCollectionExtentions
{
    //public static IServiceCollection AddDefaultServices(
    //    this IServiceCollection services,
    //    ServiceConfig config)
    //{
    //    services.SetDatabaseService(config.Database)
    //            .SetStorageServices(config.Storages)
    //            .AddAIServices(config.Services)
    //            .AddDefaultToolServices()
    //            .AddDefaultDocumentDecoders()
    //            .AddDefaultPipelineHandlers();

    //    services.AddSingleton<IMemoryService, Core.Memory.MemoryService>();
    //    return services;
    //}

    //public static IServiceCollection SetDatabaseService(
    //    this IServiceCollection services,
    //    DatabaseConfig config)
    //{
    //    services.AddDbContext<RaggleDbContext>(options =>
    //    {
    //        if (config.Type == DatabaseTypes.Sqlite)
    //        {
    //            options.UseSqlite(config.Sqlite.ConnectionString);
    //            options.AddInterceptors(new SqliteDbInterceptor());
    //        }
    //        else if (config.Type == DatabaseTypes.SqlServer)
    //        {
    //            options.UseSqlServer(config.SqlServer.ConnectionString);
    //        }
    //        else if (config.Type == DatabaseTypes.Cosmos)
    //        {
    //            options.UseCosmos(config.Cosmos.ConnectionString, config.Cosmos.DatabaseName);
    //        }
    //        else if (config.Type == DatabaseTypes.MySql)
    //        {
    //            var version = ServerVersion.AutoDetect(config.MySql.ConnectionString);
    //            options.UseMySql(config.MySql.ConnectionString, version);
    //        }
    //        else if (config.Type == DatabaseTypes.Oracle)
    //        {
    //            options.UseOracle(config.Oracle.ConnectionString);
    //        }
    //        else if (config.Type == DatabaseTypes.PostgreSQL)
    //        {
    //            options.UseNpgsql(config.PostgreSQL.ConnectionString);
    //        }
    //        else if (config.Type == DatabaseTypes.MongoDB)
    //        {
    //            options.UseMongoDB(config.MongoDB.ConnectionString, config.MongoDB.DatabaseName);
    //        }
    //        else
    //        {
    //            throw new ArgumentException($"Invalid database type: {config.Type}");
    //        }
    //    });

    //    services.AddScoped((provider) =>
    //    {
    //        var dbContext = provider.GetRequiredService<RaggleDbContext>();
    //        var memory = provider.GetRequiredService<IMemoryService>();
    //        var http = provider.GetRequiredService<IHttpContextAccessor>();
    //        var serviceId = http.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "serviceId")?.Value
    //            ?? string.Empty;
    //        return new Services.MemoryService(dbContext, memory, serviceId);
    //    });
    //    services.AddScoped<AssistantService>((provider) => 
    //    {
    //        var dbContext = provider.GetRequiredService<RaggleDbContext>();
    //        var http = provider.GetRequiredService<IHttpContextAccessor>();
    //        var serviceId = http.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "serviceId")?.Value
    //            ?? string.Empty;
    //        return new AssistantService(dbContext, serviceId);
    //    });
    //    return services;
    //}

    //public static IServiceCollection SetStorageServices(
    //    this IServiceCollection services,
    //    StorageConfig config)
    //{
    //    switch (config.Vectors.Type)
    //    {
    //        case VectorStorageTypes.LiteDB:
    //            services.SetLiteDBVectorStorage(config.Vectors.LiteDB);
    //            break;
    //        case VectorStorageTypes.Qdrant:
    //            services.SetQdrantVectorStorage(config.Vectors.Qdrant);
    //            break;
    //        default:
    //            throw new ArgumentException($"Invalid vector storage type: {config.Vectors.Type}");
    //    }

    //    switch (config.Documents.Type)
    //    {
    //        case DocumentStorageTypes.LocalDisk:
    //            services.SetLocalDiskDocumentStorage(config.Documents.LocalDisk);
    //            break;
    //        case DocumentStorageTypes.AzureBlob:
    //            services.SetAzureBlobDocumentStorage(config.Documents.AzureBlob);
    //            break;
    //        default:
    //            throw new ArgumentException($"Invalid vector storage type: {config.Documents.Type}");
    //    }

    //    return services;
    //}

    //public static IServiceCollection AddAIServices(
    //    this IServiceCollection services,
    //    AIServiceConfig config)
    //{
    //    services.AddOpenAIServices(DefaultServiceKeys.OpenAI, config.OpenAI);
    //    services.AddAnthropicServices(DefaultServiceKeys.Anthrophic, config.Anthropic);
    //    services.AddOllamaServices(DefaultServiceKeys.Ollama, config.Ollama);
    //    return services;
    //}

    //public static IServiceCollection AddDefaultToolServices(
    //    this IServiceCollection services)
    //{
    //    services.AddToolService<VectorSearchTool>(DefaultServiceKeys.VectorSearch);
    //    services.AddToolService<PythonTool>("python-interpreter");
    //    return services;
    //}

    //public static IServiceCollection AddDefaultPipelineHandlers(
    //    this IServiceCollection services)
    //{
    //    return services.AddPipelineHandler<DecodingHandler>(DefaultServiceKeys.Decoding)
    //                   .AddPipelineHandler<ChunkingHandler>(DefaultServiceKeys.Chunking)
    //                   .AddPipelineHandler<SummaryHandler>(DefaultServiceKeys.Summarizing)
    //                   .AddPipelineHandler<DialogueHandler>(DefaultServiceKeys.Dialogue)
    //                   .AddPipelineHandler<EmbeddingsHandler>(DefaultServiceKeys.Embeddings);
    //}

    //public static IServiceCollection AddDefaultDocumentDecoders(
    //    this IServiceCollection services)
    //{
    //    return services.AddDocumentDecoder<WordDecoder>()
    //                   .AddDocumentDecoder<TextDecoder>()
    //                   .AddDocumentDecoder<PPTDecoder>()
    //                   .AddDocumentDecoder<PDFDecoder>();
    //}
}

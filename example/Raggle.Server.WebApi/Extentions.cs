using Raggle.Driver.Anthropic;
using Raggle.Driver.Ollama;
using Raggle.Driver.OpenAI;
using Raggle.Core;
using Raggle.Server.WebApi.Configuration;
using Raggle.Abstractions.Extensions;
using Raggle.Core.Memory.Handlers;
using Raggle.Driver.LiteDB;
using Raggle.Driver.Qdrant;
using Raggle.Driver.LocalDisk;
using Raggle.Driver.AzureBlob;
using Raggle.Core.Memory.Decoders;
using Raggle.Server.WebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Raggle.Server.WebApi;

public static partial class ServiceCollectionExtention
{
    public static IServiceCollection AddRaggle(this IServiceCollection services, RaggleConfig config)
    {
        services.AddRaggleServices(config);
        var builder = new RaggleBuilder(services);

        var raggle = builder.Build();
        services.AddSingleton(raggle);
        return services;
    }

    public static IServiceCollection AddRaggleBuilder(this IServiceCollection services, RaggleConfig config)
    {
        services.AddRaggleServices(config);
        var builder = new RaggleBuilder(services);

        services.AddSingleton(builder);
        return services;
    }

    public static IServiceCollection AddRaggleServices(this IServiceCollection services, RaggleConfig config)
    {
        return services.SetRaggleDB(config)
                       .AddAIServices(config)
                       .SetVectorStorage(config)
                       .SetDocumentStorage(config)
                       .AddDocumentDecoders()
                       .AddPipelineHandlers();
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
        return services.AddPipelineHandler<DecodingHandler>("decoding")
                       .AddPipelineHandler<ChunkingHandler>("chunk")
                       .AddPipelineHandler<SummarizationHandler>("summarize")
                       .AddPipelineHandler<GenerateQAHandler>("qa")
                       .AddPipelineHandler<EmbeddingsHandler>("embed");
    }
}

public static partial class ConfigurantionBuilderExtension
{
    public static IConfigurationBuilder AddRaggleConfigFile(this IConfigurationBuilder configBuilder, string? filePath = null)
    {
        //filePath ??= Path.Combine(Directory.GetCurrentDirectory(), "ragglesettings.json");

        //if (!File.Exists(filePath))
        //{
        //    CreateNew();
        //}
        //else
        //{
        //    var jsonConfig = File.ReadAllText(filePath);
        //    var config = JsonSerializer.Deserialize<JsonDocument?>(jsonConfig, RaggleConfig.DefaultJsonOptions);
        //    config?.RootElement.TryGetProperty("Raggle", out var content);
        //    var raggleConfig = content.Deserialize<RaggleConfig>();
        //    if (raggleConfig != null)
        //    {
        //        CreateNew();
        //    }
        //}

        //configBuilder.AddJsonFile(filePath, optional: false, reloadOnChange: true);

        //void CreateNew()
        //{
        //    var config = new { Raggle = new RaggleConfig() };
        //    var jsonConfig = JsonSerializer.Serialize(config, RaggleConfig.DefaultJsonOptions);
        //    File.WriteAllText(filePath, jsonConfig);
        //}

        return configBuilder;
    }
}

public static partial class ServiceProviderExtension
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

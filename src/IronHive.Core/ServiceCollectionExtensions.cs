using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions;
using IronHive.Core.Files;
using IronHive.Core.Memory;
using IronHive.Core.Services;
using IronHive.Core.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core;

public static class ServiceCollectionExtensions
{
    public static HiveServiceBuilder AddHiveServiceCore(this IServiceCollection services)
    {
        // 코어 서비스 등록
        services.AddSingleton<IHiveMind, HiveMind>();
        services.AddSingleton<IChatCompletionService, ChatCompletionService>();
        services.AddSingleton<IEmbeddingService, EmbeddingService>();
        services.AddSingleton<IToolManager, ToolManager>();

        services.AddSingleton<IMemoryService, MemoryService>();
        services.AddSingleton<IPipelineOrchestrator, PipelineOrchestrator>();
        services.AddSingleton<IFileStorageFactory, FileStorageFactory>();
        services.AddSingleton<IFileManager, FileManager>();

        return new HiveServiceBuilder(services);
    }
}

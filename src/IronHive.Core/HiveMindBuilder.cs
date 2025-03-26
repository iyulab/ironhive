using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions;
using IronHive.Core.Services;
using IronHive.Core.Memory;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions.Files;
using IronHive.Core.Files;
using IronHive.Core.Tools;

namespace IronHive.Core;

public class HiveMindBuilder : HiveBuilderBase<HiveMindBuilder>
{
    public HiveMindBuilder() : base(new ServiceCollection())
    {
    }

    public IHiveMind Build()
    {
        // 코어 서비스 등록
        _services.AddSingleton<IChatCompletionService, ChatCompletionService>();
        _services.AddSingleton<IEmbeddingService, EmbeddingService>();
        _services.AddSingleton<IToolManager, ToolManager>();

        _services.AddSingleton<IMemoryService, MemoryService>();
        _services.AddSingleton<IPipelineOrchestrator, PipelineOrchestrator>();
        _services.AddSingleton<IFileStorageFactory, FileStorageFactory>();
        _services.AddSingleton<IFileManager, FileManager>();

        var provider = _services.BuildServiceProvider();
        return new HiveMind(provider);
    }
}
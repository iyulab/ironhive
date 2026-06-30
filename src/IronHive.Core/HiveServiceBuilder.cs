using IronHive.Abstractions;
using IronHive.Abstractions.Audio;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Images;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions.Vector;
using IronHive.Abstractions.Videos;
using IronHive.Abstractions.Workflow;
using IronHive.Core.Agent;
using IronHive.Core.Files;
using IronHive.Core.Memory;
using IronHive.Core.Services;
using IronHive.Core.Tools;
using IronHive.Core.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core;

public class HiveServiceBuilder : IHiveServiceBuilder
{
    private readonly Dictionary<string, IModelCatalog> _catalogs = new();
    private readonly Dictionary<string, IMessageGenerator> _messageGenerators = new();
    private readonly Dictionary<string, IEmbeddingGenerator> _embeddingGenerators = new();
    private readonly Dictionary<string, IImageGenerator> _imageGenerators = new();
    private readonly Dictionary<string, IVideoGenerator> _videoGenerators = new();
    private readonly Dictionary<string, IAudioProcessor> _audioProcessors = new();
    private readonly Dictionary<string, IFileStorage> _fileStorages = new();
    private readonly Dictionary<string, IVectorStorage> _vectorStorages = new();
    private readonly Dictionary<string, IQueueStorage> _queueStorages = new();
    private readonly List<ITool> _tools = [];
    private readonly List<Action<IServiceCollection>> _stepRegistrations = [];
    private readonly List<Action<IToolCollection, IServiceProvider?>> _toolInitializers = [];

    public IHiveServiceBuilder AddModelCatalog(string name, IModelCatalog catalog)
    { _catalogs[name] = catalog; return this; }

    public IHiveServiceBuilder AddMessageGenerator(string name, IMessageGenerator generator)
    { _messageGenerators[name] = generator; return this; }

    public IHiveServiceBuilder AddEmbeddingGenerator(string name, IEmbeddingGenerator generator)
    { _embeddingGenerators[name] = generator; return this; }

    public IHiveServiceBuilder AddImageGenerator(string name, IImageGenerator generator)
    { _imageGenerators[name] = generator; return this; }

    public IHiveServiceBuilder AddVideoGenerator(string name, IVideoGenerator generator)
    { _videoGenerators[name] = generator; return this; }

    public IHiveServiceBuilder AddAudioProcessor(string name, IAudioProcessor processor)
    { _audioProcessors[name] = processor; return this; }

    public IHiveServiceBuilder AddFileStorage(string name, IFileStorage storage)
    { _fileStorages[name] = storage; return this; }

    public IHiveServiceBuilder AddVectorStorage(string name, IVectorStorage storage)
    { _vectorStorages[name] = storage; return this; }

    public IHiveServiceBuilder AddQueueStorage(string name, IQueueStorage storage)
    { _queueStorages[name] = storage; return this; }

    public IHiveServiceBuilder AddTool(ITool tool)
    { _tools.Add(tool); return this; }

    public IHiveServiceBuilder AddWorkflowStep<T>(string name, T? instance = null)
        where T : class, IWorkflowStep
    {
        if (instance is not null)
            _stepRegistrations.Add(sc => sc.AddKeyedSingleton<IWorkflowStep>(name, instance));
        else
            _stepRegistrations.Add(sc => sc.AddKeyedTransient<IWorkflowStep, T>(name));
        return this;
    }

    public IHiveServiceBuilder AddToolInitializer(Action<IToolCollection, IServiceProvider?> initializer)
    { _toolInitializers.Add(initializer); return this; }

    public IHiveService Build(IServiceProvider? sp = null)
    {
        // 내부 ServiceCollection — 워크플로우 스텝 및 메모리 파이프라인용
        var internalSc = new ServiceCollection();

        // 딕셔너리를 내부 DI에 등록 (파이프라인 등에서 주입받을 수 있도록)
        internalSc.AddSingleton<IReadOnlyDictionary<string, IModelCatalog>>(_catalogs);
        internalSc.AddSingleton<IReadOnlyDictionary<string, IMessageGenerator>>(_messageGenerators);
        internalSc.AddSingleton<IReadOnlyDictionary<string, IEmbeddingGenerator>>(_embeddingGenerators);
        internalSc.AddSingleton<IReadOnlyDictionary<string, IImageGenerator>>(_imageGenerators);
        internalSc.AddSingleton<IReadOnlyDictionary<string, IVideoGenerator>>(_videoGenerators);
        internalSc.AddSingleton<IReadOnlyDictionary<string, IAudioProcessor>>(_audioProcessors);
        internalSc.AddSingleton<IReadOnlyDictionary<string, IFileStorage>>(_fileStorages);
        internalSc.AddSingleton<IReadOnlyDictionary<string, IVectorStorage>>(_vectorStorages);
        internalSc.AddSingleton<IReadOnlyDictionary<string, IQueueStorage>>(_queueStorages);

        // 워크플로우 스텝 등록
        foreach (var reg in _stepRegistrations)
            reg(internalSc);

        var internalSp = internalSc.BuildServiceProvider();

        // 외부 SP와 내부 SP를 합성: 외부 우선, 내부 폴백
        IServiceProvider effectiveSp = sp != null
            ? new CompositeServiceProvider(sp, internalSp)
            : internalSp;

        // 툴 컬렉션 생성 및 초기화
        var toolCollection = new ToolCollection(_tools, null, effectiveSp);
        foreach (var init in _toolInitializers)
            init(toolCollection, sp);

        // 서비스 생성
        var catalogService = new ModelCatalogService(_catalogs);
        var embeddingService = new EmbeddingService(_embeddingGenerators);
        var messageService = new MessageService(_messageGenerators, toolCollection, sp);
        var imageService = new ImageService(_imageGenerators);
        var videoService = new VideoService(_videoGenerators);
        var audioService = new AudioService(_audioProcessors);
        var fileService = new FileStorageService(_fileStorages);
        var memoryService = new MemoryService(_vectorStorages, _queueStorages, embeddingService);
        var workflowFactory = new WorkflowFactory(effectiveSp);
        var agentService = new AgentService(messageService);

        return new HiveService(
            catalog: catalogService,
            messages: messageService,
            embeddings: embeddingService,
            images: imageService,
            videos: videoService,
            audio: audioService,
            files: fileService,
            memory: memoryService,
            workflows: workflowFactory,
            agents: agentService,
            internalSp: internalSp,
            vectorStorages: _vectorStorages,
            queueStorages: _queueStorages);
    }
}

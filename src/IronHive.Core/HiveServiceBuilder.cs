using IronHive.Abstractions;
using IronHive.Abstractions.Audio;
using IronHive.Abstractions.Models;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Images;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Vector;
using IronHive.Abstractions.Videos;
using IronHive.Core.Agent;
using IronHive.Core.Files;
using IronHive.Core.Memory;
using IronHive.Core.Services;

namespace IronHive.Core;

public class HiveServiceBuilder : IHiveServiceBuilder
{
    private readonly Dictionary<string, IModelFinder> _modelFinders = new();
    private readonly Dictionary<string, IMessageGenerator> _messageGenerators = new();
    private readonly Dictionary<string, IEmbeddingGenerator> _embeddingGenerators = new();
    private readonly Dictionary<string, IImageGenerator> _imageGenerators = new();
    private readonly Dictionary<string, IVideoGenerator> _videoGenerators = new();
    private readonly Dictionary<string, IAudioProcessor> _audioProcessors = new();
    private readonly Dictionary<string, IFileStorage> _fileStorages = new();
    private readonly Dictionary<string, IVectorStorage> _vectorStorages = new();
    private readonly Dictionary<string, IQueueStorage> _queueStorages = new();

    public IHiveServiceBuilder AddModelFinder(string name, IModelFinder finder)
    { _modelFinders[name] = finder; return this; }

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

    public IHiveService Build(IServiceProvider? sp = null)
    {
        var modelService = new ModelService(_modelFinders);
        var embeddingService = new EmbeddingService(_embeddingGenerators);
        var messageService = new MessageService(_messageGenerators);
        var imageService = new ImageService(_imageGenerators);
        var videoService = new VideoService(_videoGenerators);
        var audioService = new AudioService(_audioProcessors);
        var fileService = new FileStorageService(_fileStorages);
        var memoryService = new MemoryService(_vectorStorages, _queueStorages, embeddingService);
        var agentService = new AgentService(messageService);

        return new HiveService(
            models: modelService,
            messages: messageService,
            embeddings: embeddingService,
            images: imageService,
            videos: videoService,
            audio: audioService,
            files: fileService,
            memory: memoryService,
            agents: agentService,
            vectorStorages: _vectorStorages,
            queueStorages: _queueStorages);
    }
}

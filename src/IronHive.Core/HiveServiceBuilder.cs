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
using IronHive.Core.Files;
using IronHive.Core.Services;

namespace IronHive.Core;

public class HiveServiceBuilder : IHiveServiceBuilder
{
    private readonly Dictionary<string, IModelFinder> _models = new();
    private readonly Dictionary<string, IMessageGenerator> _messages = new();
    private readonly Dictionary<string, IEmbeddingGenerator> _embeddings = new();
    private readonly Dictionary<string, IImageGenerator> _images = new();
    private readonly Dictionary<string, IVideoGenerator> _videos = new();
    private readonly Dictionary<string, IAudioProcessor> _audios = new();
    private readonly Dictionary<string, IFileStorage> _files = new();
    private readonly Dictionary<string, IVectorStorage> _vectors = new();
    private readonly Dictionary<string, IQueueStorage> _queues = new();

    public IHiveServiceBuilder AddModelFinder(string name, IModelFinder finder)
    { 
        _models[name] = finder; 
        return this; 
    }

    public IHiveServiceBuilder AddMessageGenerator(string name, IMessageGenerator generator)
    { 
        _messages[name] = generator; 
        return this; 
    }

    public IHiveServiceBuilder AddEmbeddingGenerator(string name, IEmbeddingGenerator generator)
    { 
        _embeddings[name] = generator; 
        return this; 
    }

    public IHiveServiceBuilder AddImageGenerator(string name, IImageGenerator generator)
    { 
        _images[name] = generator; 
        return this; 
    }

    public IHiveServiceBuilder AddVideoGenerator(string name, IVideoGenerator generator)
    { 
        _videos[name] = generator; 
        return this; 
    }

    public IHiveServiceBuilder AddAudioProcessor(string name, IAudioProcessor processor)
    { 
        _audios[name] = processor; 
        return this; 
    }

    public IHiveServiceBuilder AddFileStorage(string name, IFileStorage storage)
    { 
        _files[name] = storage; 
        return this; 
    }

    public IHiveServiceBuilder AddVectorStorage(string name, IVectorStorage storage)
    { 
        _vectors[name] = storage; 
        return this; 
    }

    public IHiveServiceBuilder AddQueueStorage(string name, IQueueStorage storage)
    { 
        _queues[name] = storage; 
        return this; 
    }

    public IHiveService Build()
    {
        var modelService = new ModelService(_models);
        var messageService = new MessageService(_messages);
        var embeddingService = new EmbeddingService(_embeddings);
        var imageService = new ImageService(_images);
        var videoService = new VideoService(_videos);
        var audioService = new AudioService(_audios);
        var fileService = new FileStorageService(_files);

        return new HiveService(
            models: modelService,
            messages: messageService,
            embeddings: embeddingService,
            images: imageService,
            videos: videoService,
            audio: audioService,
            files: fileService,
            vectors: _vectors,
            queues: _queues);
    }
}

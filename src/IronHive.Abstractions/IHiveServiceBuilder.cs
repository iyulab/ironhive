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

namespace IronHive.Abstractions;

public interface IHiveServiceBuilder
{
    IHiveServiceBuilder AddModelCatalog(string name, IModelCatalog catalog);
    IHiveServiceBuilder AddMessageGenerator(string name, IMessageGenerator generator);
    IHiveServiceBuilder AddEmbeddingGenerator(string name, IEmbeddingGenerator generator);
    IHiveServiceBuilder AddImageGenerator(string name, IImageGenerator generator);
    IHiveServiceBuilder AddVideoGenerator(string name, IVideoGenerator generator);
    IHiveServiceBuilder AddAudioProcessor(string name, IAudioProcessor processor);
    IHiveServiceBuilder AddFileStorage(string name, IFileStorage storage);
    IHiveServiceBuilder AddVectorStorage(string name, IVectorStorage storage);
    IHiveServiceBuilder AddQueueStorage(string name, IQueueStorage storage);
    IHiveServiceBuilder AddTool(ITool tool);
    IHiveServiceBuilder AddWorkflowStep<T>(string name, T? instance = null) where T : class, IWorkflowStep;
    IHiveServiceBuilder AddToolInitializer(Action<IToolCollection, IServiceProvider?> initializer);
    IHiveService Build(IServiceProvider? sp = null);
}

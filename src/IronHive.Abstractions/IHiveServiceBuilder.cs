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
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Abstractions;

/// <summary>
/// Hive 서비스를 등록하고 HiveMind 인스턴스를 생성하기 위한 빌더 인터페이스입니다.
/// </summary>
public interface IHiveServiceBuilder
{
    /// <summary>
    /// Hive 서비스 등록에 사용되는 서비스 컬렉션을 가져옵니다.
    /// </summary>
    IServiceCollection Services { get; }


    /// <summary>
    /// 새로운 모델 제공자를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddModelCatalog(string providerName, IModelCatalog catalog);

    /// <summary>
    /// 새로운 임베딩 생성기를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddEmbeddingGenerator(string providerName, IEmbeddingGenerator generator);

    /// <summary>
    /// 새로운 메시지 생성기를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddMessageGenerator(string providerName, IMessageGenerator generator);

    /// <summary>
    /// 새로운 이미지 생성기를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddImageGenerator(string providerName, IImageGenerator generator);

    /// <summary>
    /// 새로운 비디오 생성기를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddVideoGenerator(string providerName, IVideoGenerator generator);

    /// <summary>
    /// 새로운 오디오 프로세서를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddAudioProcessor(string providerName, IAudioProcessor processor);

    /// <summary>
    /// 파일 저장소를 추가합니다.
    /// </summary>
    IHiveServiceBuilder AddFileStorage(string storageName, IFileStorage storage);

    /// <summary>
    /// 큐 저장소를 설정합니다.
    /// </summary>
    IHiveServiceBuilder AddQueueStorage(string storageName, IQueueStorage storage);

    /// <summary>
    /// 벡터 저장소를 설정합니다.
    /// </summary>
    IHiveServiceBuilder AddVectorStorage(string storageName, IVectorStorage storage);

    /// <summary>
    /// 툴을 추가합니다.
    /// </summary>
    IHiveServiceBuilder AddTool(ITool tool);

    /// <summary>
    /// 작업들을 DI 컨테이너에 등록합니다.
    /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords — 'step' is domain-specific and not a VB.NET concern
    IHiveServiceBuilder AddWorkflowStep<T>(string stepName, T? step = null)
        where T : class, IWorkflowStep;
#pragma warning restore CA1716

    /// <summary>
    /// HiveService 인스턴스를 생성합니다.
    /// </summary>
    IHiveService Build();
}

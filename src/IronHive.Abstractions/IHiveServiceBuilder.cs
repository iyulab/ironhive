using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Message;
using IronHive.Abstractions.Storages;

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
    IHiveServiceBuilder AddModelCatalogProvider(IModelCatalogProvider provider);

    /// <summary>
    /// 새로운 메시지 생성기를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddMessageGenerator(IMessageGenerator provider);

    /// <summary>
    /// 새로운 임베딩 생성기를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddEmbeddingGenerator(IEmbeddingGenerator provider);

    /// <summary>
    /// 툴을 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddTool(ITool tool);

    /// <summary>
    /// 벡터 스토리지를 싱글턴으로 등록합니다.
    /// 하나의 벡터 스토리지만 등록할 수 있으며, 이후 등록은 기존 것을 대체합니다.
    /// </summary>
    IHiveServiceBuilder WithVectorStorage(IVectorStorage storage);

    /// <summary>
    /// 메모리 큐 스토리지를 싱글턴으로 등록합니다.
    /// 하나의 큐 스토리지만 등록할 수 있으며, 이후 등록은 기존 것을 대체합니다.
    /// </summary>
    IHiveServiceBuilder WithMemoryQueueStorage(IQueueStorage<MemoryPipelineRequest> storage);

    /// <summary>
    /// 메모리 파이프라인 핸들러를 등록합니다.
    /// </summary>
    /// <param name="serviceKey">메모리 서비스에서 파이프라인 단계로 사용될 고유 키입니다.</param>
    /// <param name="lifetime">서비스 수명 주기입니다.</param>
    /// <param name="implementationFactory">
    /// 파이프라인 핸들러 인스턴스를 생성하는 팩토리 메서드입니다.
    /// 첫 번째 매개변수는 서비스 프로바이더, 두 번째는 서비스 키입니다.
    /// null이면 기본 구현이 사용됩니다.
    /// </param>
    IHiveServiceBuilder AddMemoryPipelineHandler<TImplementation>(
        string serviceKey,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, object?, TImplementation>? implementationFactory = null)
        where TImplementation : class, IMemoryPipelineHandler;

    /// <summary>
    /// 파일 저장소 구현을 싱글턴으로 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddFileStorage(IFileStorage storage);

    /// <summary>
    /// 하나 이상의 파일 디코더를 싱글턴으로 등록합니다.
    /// 디코더의 순서가 중요하며, 첫 번째로 일치하는 디코더가 사용됩니다.
    /// </summary>
    IHiveServiceBuilder AddFileDecoder(IFileDecoder decoder);

    /// <summary>
    /// HiveService 인스턴스를 생성합니다.
    /// </summary>
    IHiveService Build();
}

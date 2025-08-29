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
    /// Hive 서비스의 파일 서비스를 구성하기 위한 빌더를 가져옵니다.
    /// </summary>
    IFileServiceBuilder File { get; }

    /// <summary>
    /// Hive 서비스의 메모리를 구성하기 위한 빌더를 가져옵니다.
    /// </summary>
    IMemoryServiceBuilder Memory { get; }

    /// <summary>
    /// 새로운 모델 제공자를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddModelCatalogProvider(IModelCatalogProvider provider);

    /// <summary>
    /// 새로운 임베딩 생성기를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddEmbeddingGenerator(IEmbeddingGenerator provider);

    /// <summary>
    /// 새로운 메시지 생성기를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddMessageGenerator(IMessageGenerator provider);

    /// <summary>
    /// 새로운 툴을 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddMessageTool(ITool tool);

    /// <summary>
    /// HiveService 인스턴스를 생성합니다.
    /// </summary>
    IHiveService Build();
}

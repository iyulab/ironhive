using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Message;

namespace IronHive.Abstractions;

/// <summary>
/// HiveService 인터페이스
/// </summary>
public interface IHiveService
{
    /// <summary>
    /// 서비스 제공자
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    /// 모델 제공자를 등록합니다. 기존 제공자가 있으면 대체됩니다.
    /// </summary>
    IHiveService SetModelCatalogProvider(IModelCatalogProvider provider);

    /// <summary>
    /// 메시지 생성기를 등록합니다. 기존 생성기가 있으면 대체됩니다.
    /// </summary>
    IHiveService SetMessageGenerator(IMessageGenerator generator);

    /// <summary>
    /// 임베딩 생성기를 등록합니다. 기존 생성기가 있으면 대체됩니다.
    /// </summary>
    IHiveService SetEmbeddingGenerator(IEmbeddingGenerator generator);

    /// <summary>
    /// 지정된 이름에 해당하는 모델 제공자를 제거합니다.
    /// </summary>
    IHiveService RemoveModelCatalogProvider(string name);

    /// <summary>
    /// 지정된 이름에 해당하는 메시지 생성기를 제거합니다.
    /// </summary>
    IHiveService RemoveMessageGenerator(string name);

    /// <summary>
    /// 지정된 이름에 해당하는 임베딩 생성기를 제거합니다.
    /// </summary>
    IHiveService RemoveEmbeddingGenerator(string name);
}

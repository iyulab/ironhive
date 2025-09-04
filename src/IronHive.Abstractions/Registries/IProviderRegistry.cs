using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions.Registries;

/// <summary>
/// <see cref="IProviderRegistry"/>에 저장될 수 있는 항목을 나타내는 마커 인터페이스입니다.  
/// - 모든 Provider 항목은 이 인터페이스를 구현해야 하며,  
/// - 리소스 해제를 위해 <see cref="IDisposable"/>을 상속받습니다.  
/// </summary>
public interface IProviderItem : IDisposable
{ }

/// <summary>
/// 문자열 키(<see cref="string"/>)와 <see cref="IProviderItem"/>을 매핑하는 서비스 레지스트리입니다.  
/// - 주로 AI 모델 제공자(Provider)와 관련된 다양한 서비스들을 통합 관리합니다.
/// </summary>
public interface IProviderRegistry : IKeyedServiceRegistry<string, IProviderItem>
{
    /// <summary>
    /// Provider가 제공하는 다양한 모델들의 정보를 조회하는 서비스를 등록합니다.
    /// </summary>
    /// <param name="providerName">등록할 Provider의 고유 이름</param>
    /// <param name="catalog">해당 Provider가 제공하는 모델 카탈로그</param>
    void SetModelCatalog(string providerName, IModelCatalog catalog);

    /// <summary>
    /// 특정 Provider의 LLM(Message Generator) 서비스를 등록합니다.
    /// </summary>
    /// <param name="providerName">등록할 Provider의 고유 이름</param>
    /// <param name="generator">LLM 메시지 생성 서비스</param>
    void SetMessageGenerator(string providerName, IMessageGenerator generator);

    /// <summary>
    /// 특정 Provider에 임베딩 생성기(<see cref="IEmbeddingGenerator"/>)를 등록합니다.
    /// </summary>
    /// <param name="providerName">등록할 Provider의 고유 이름</param>
    /// <param name="generator">임베딩 생성 서비스</param>
    void SetEmbeddingGenerator(string providerName, IEmbeddingGenerator generator);
}

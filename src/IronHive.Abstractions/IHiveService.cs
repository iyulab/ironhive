using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions;

/// <summary>
/// HiveService 인터페이스  
/// 애플리케이션 전반에서 사용하는 주요 서비스와 관리 리소스를 정의합니다.  
/// </summary>
public interface IHiveService
{
    /// <summary>
    /// 서비스 공급자  
    /// 애플리케이션에서 필요한 서비스 인스턴스를 제공하는 DI(Dependency Injection) 컨테이너입니다.
    /// </summary>
    IServiceProvider Services { get; }

    #region 관리 리소스 영역

    /// <summary>
    /// AI 서비스 공급자 레지스트리  
    /// 다양한 AI 모델 및 서비스 공급자를 등록하고 관리합니다.
    /// </summary>
    IProviderRegistry Providers { get; }

    /// <summary>
    /// 스토리지 서비스 공급자 레지스트리  
    /// 파일, 데이터 저장소 등의 스토리지 서비스들을 등록하고 관리합니다.
    /// </summary>
    IStorageRegistry Storages { get; }

    /// <summary>
    /// 도구(툴) 서비스 컬렉션  
    /// 애플리케이션에서 활용 가능한 각종 도구들을 제공합니다.
    /// </summary>
    IToolCollection Tools { get; }

    #endregion

    #region 핵심 서비스 영역

    /// <summary>
    /// 에이전트 서비스  
    /// 에이전트의 생성, 실행, 관리 등의 기능을 담당합니다.
    /// </summary>
    IAgentService Agent { get; }

    /// <summary>
    /// 메모리 서비스  
    /// 애플리케이션의 단기/장기 메모리를 관리하고 조회할 수 있는 기능을 제공합니다.
    /// </summary>
    IMemoryService Memory { get; }

    #endregion
}

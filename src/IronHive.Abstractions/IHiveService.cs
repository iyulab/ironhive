using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions.Workflow;

namespace IronHive.Abstractions;

/// <summary>
/// HiveService 인터페이스  
/// 애플리케이션 전반에서 사용하는 주요 서비스와 관리 리소스를 정의합니다.  
/// </summary>
public interface IHiveService
{
    #region 관리 리소스 영역

    /// <summary>
    /// 서비스 공급자  
    /// 애플리케이션에서 필요한 서비스 인스턴스를 제공하는 DI(Dependency Injection) 컨테이너입니다.
    /// </summary>
    IServiceProvider Services { get; }

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
    /// 메모리 서비스를 제공합니다.
    /// </summary>
    IMemoryService Memory { get; }

    /// <summary>
    /// 에이전트 서비스를 Yaml 문자열로부터 생성합니다.
    /// </summary>
    IAgent CreateAgentFrom(AgentCard card);

    /// <summary>
    /// 에이전트 서비스를 Yaml 문자열로부터 생성합니다.
    /// </summary>
    IAgent CreateAgentFromYaml(string yaml);

    /// <summary>
    /// 메모리 작업 서비스를 생성합니다.
    /// </summary>
    IAgent CreateMemoryWorker(string queueName, WorkflowDefinition definition);

    #endregion
}

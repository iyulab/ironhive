using IronHive.Abstractions.Memory;

namespace IronHive.Abstractions;

/// <summary>
/// HiveMind 인터페이스
/// </summary>
public interface IHiveMind
{
    /// <summary>
    /// HiveMind의 서비스 제공자
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    /// 아직 모름
    /// </summary>
    IHiveSession CreateHiveSession(IHiveAgent master, IDictionary<string, IHiveAgent>? agents);

    /// <summary>
    /// 메모리 서비스 생성
    /// </summary>
    /// <param name="embedProvider">임베딩 모델 제공자 서비스키</param>
    /// <param name="embedModel">임베딩 모델 식별자</param>
    /// <returns></returns>
    IHiveMemory CreateHiveMemory(string embedProvider, string embedModel);

    /// <summary>
    /// 메모리 파이프라인 백그라운드 워커 생성
    /// </summary>
    /// <param name="maxExecutionSlots">최대 동시 실행 슬롯 수</param>
    /// <param name="pollingInterval">작업 폴링 대기 간격</param>
    IPipelineWorker CreatePipelineWorker(int maxExecutionSlots, TimeSpan pollingInterval);
}

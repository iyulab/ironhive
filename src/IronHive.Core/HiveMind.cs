using IronHive.Abstractions;
using IronHive.Abstractions.Memory;

namespace IronHive.Core;

public class HiveMind : IHiveMind
{
    public IServiceProvider Services { get; }

    public HiveMind(IServiceProvider services)
    {
        Services = services;
    }

    // 세션 생성 (싱글 or 멀티 에이전트)
    // 채팅 세션 Or 작업 세션
    public IHiveSession CreateSession()
    {
        // TODO: Implement session creation
        throw new NotImplementedException();
    }

    // 메모리 서비스 반환
    public IHiveMemory CreateMemory(string embedProvider, string embedModel)
    {
        // TODO: Implement memory collection creation
        throw new NotImplementedException();
    }

    // 메모리 파이프라인 백그라운드 워커 생성
    public IPipelineWorker CreatePipelineWorker()
    {
        // TODO: Implement memory worker creation
        throw new NotImplementedException();
    }
}

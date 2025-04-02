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

    // 채팅 세션 생성 (싱글 or 멀티 에이전트)
    public T CreateChatSession<T>()
        where T : IHiveSession
    {
        // TODO: Implement session creation
        throw new NotImplementedException();
    }

    // 작업 세션 생성 (싱글 or 멀티 에이전트)
    public T CreateJobSession<T>()
        where T : IHiveSession
    {
        // TODO: Implement session creation
        throw new NotImplementedException();
    }

    // 파이프라인 백그라운드 워커 생성
    public IPipelineWorker CreatePipelineWorker(
        int maxConcurrent = 1)
    {
        // TODO: Implement memory worker creation
        throw new NotImplementedException();
    }

    // 벡터 DB 컬렉션 리스트
    public IEnumerable<string> ListMemoryCollection()
    {
        throw new NotImplementedException();
    }

    // 메모리 서비스 반환, 컬렉션이 없으면 새로 생성
    public IMemoryService GetMemoryCollection(
        string collectionName,
        string embedProvider,
        string embedModel)
    {
        // TODO: Implement memory collection creation
        throw new NotImplementedException();
    }

    // 벡터 DB 컬렉션 삭제
    public Task DeleteMemoryCollection(string collectionName)
    {
        throw new NotImplementedException();
    }
}

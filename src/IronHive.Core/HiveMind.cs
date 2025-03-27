using IronHive.Abstractions;

namespace IronHive.Core;

public class HiveMind : IHiveMind
{
    public IServiceProvider Services { get; }

    public HiveMind(IServiceProvider services)
    {
        Services = services;
    }

    public T CreateSession<T>() where T : IHiveSession
    {
        // 1. Single Turn (단일 Q & A) || Multi Turn (History 기반) 분류
        // 2. 싱글 에이전트 || 그룹 에이전트 분류
        // 3. 채팅 세션 || 작업 세션 분류

        // TODO: Implement session creation
        throw new NotImplementedException();
    }
}

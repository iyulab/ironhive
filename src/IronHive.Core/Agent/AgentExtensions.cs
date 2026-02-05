using IronHive.Abstractions.Agent;

namespace IronHive.Core.Agent;

/// <summary>
/// 에이전트 확장 메서드
/// </summary>
public static class AgentExtensions
{
    /// <summary>
    /// 에이전트에 미들웨어 체인을 적용합니다.
    /// </summary>
    public static IAgent WithMiddleware(this IAgent agent, params IAgentMiddleware[] middlewares)
    {
        if (middlewares.Length == 0)
            return agent;

        return new MiddlewareAgent(agent, middlewares);
    }
}

namespace IronHive.Abstractions.Agent;

public interface IAgentServiceBuilder
{
    /// <summary>
    /// 서비스를 빌드합니다.
    /// </summary>
    IAgentService Build();
}

using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Agent;

public interface IAgentServiceBuilder
{
    /// <summary>
    /// 새로운 툴을 등록합니다.
    /// </summary>
    IAgentServiceBuilder AddTool(ITool tool);

    /// <summary>
    /// 서비스를 빌드합니다.
    /// </summary>
    IAgentService Build();
}

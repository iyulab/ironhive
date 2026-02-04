using IronHive.Abstractions.Agent;

namespace IronHive.Core.Agent;

/// <summary>
/// YAML/TOML 파싱 시 루트 래퍼 클래스입니다.
/// </summary>
internal class AgentConfigRoot
{
    public AgentConfig Agent { get; set; } = new();
}

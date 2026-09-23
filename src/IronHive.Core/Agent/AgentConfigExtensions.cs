using IronHive.Abstractions.Agent;

namespace IronHive.Core.Agent;

/// <summary>
/// AgentConfig DTO에 대한 비즈니스 로직 확장 메서드입니다.
/// </summary>
public static class AgentConfigExtensions
{
#pragma warning disable CA2208
    public static void Validate(this AgentConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Model))
            throw new ArgumentException("Agent model is required.", nameof(AgentConfig.Model));
    }
#pragma warning restore CA2208
}

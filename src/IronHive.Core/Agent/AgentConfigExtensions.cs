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

        if (config.Tools is { Count: > 0 } || config.ToolOptions is { Count: > 0 })
        {
            throw new NotSupportedException(
                $"{nameof(AgentConfig)}.{nameof(AgentConfig.Tools)}/{nameof(AgentConfig.ToolOptions)} " +
                "is parsed but never resolved when building an agent from this config: " +
                $"{nameof(IronHive.Core.Agent.AgentService)} has no name-to-ITool registry, so a " +
                "declared tool would silently never execute. Set IAgent.Tools directly on the " +
                "constructed agent instead (e.g. myToolCollection.FilterBy(names)), or use a " +
                "framework that resolves tool names for you (e.g. Ironbees's AgentConfig.Tools + " +
                "IronhiveOptions.Tools).");
        }
    }
#pragma warning restore CA2208
}

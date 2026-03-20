using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Agent;

/// <summary>
/// AgentConfig DTO에 대한 비즈니스 로직 확장 메서드입니다.
/// </summary>
public static class AgentConfigExtensions
{
    /// <summary>
    /// 설정 유효성을 검증합니다.
    /// </summary>
#pragma warning disable CA2208 // paramName is intentionally the property name being validated
    public static void Validate(this AgentConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Name))
            throw new ArgumentException("Agent name is required.", nameof(AgentConfig.Name));
        if (string.IsNullOrWhiteSpace(config.Provider))
            throw new ArgumentException("Agent provider is required.", nameof(AgentConfig.Provider));
        if (string.IsNullOrWhiteSpace(config.Model))
            throw new ArgumentException("Agent model is required.", nameof(AgentConfig.Model));
    }
#pragma warning restore CA2208

    /// <summary>
    /// ToolItem 목록으로 변환합니다.
    /// </summary>
    public static IEnumerable<ToolItem>? ToToolItems(this AgentConfig config)
    {
        if (config.Tools == null || config.Tools.Count == 0)
            return null;

        return config.Tools.Select(toolName => new ToolItem
        {
            Name = toolName,
            Options = config.ToolOptions?.GetValueOrDefault(toolName)
        });
    }

    /// <summary>
    /// MessageGenerationParameters로 변환합니다.
    /// </summary>
    public static MessageGenerationParameters? ToParameters(this AgentConfig config)
    {
        if (config.Parameters == null)
            return null;

        return new MessageGenerationParameters
        {
            MaxTokens = config.Parameters.MaxTokens,
            Temperature = config.Parameters.Temperature,
            TopP = config.Parameters.TopP,
            TopK = config.Parameters.TopK,
            StopSequences = config.Parameters.StopSequences
        };
    }
}

using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Agent;

/// <summary>
/// Agent 설정을 위한 DTO 클래스입니다.
/// YAML, TOML, JSON에서 역직렬화할 때 사용됩니다.
/// </summary>
internal class AgentConfig
{
    /// <summary>
    /// 에이전트의 이름입니다.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 에이전트에 대한 설명입니다.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 기본 LLM 제공자입니다. (예: "openai", "anthropic")
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// 기본 모델 이름입니다. (예: "gpt-4o-mini", "claude-3-sonnet")
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 에이전트 동작을 안내하는 지침(시스템 프롬프트)입니다.
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// 에이전트가 사용할 도구 이름 목록입니다.
    /// </summary>
    public List<string>? Tools { get; set; }

    /// <summary>
    /// 도구별 옵션 설정입니다.
    /// </summary>
    public Dictionary<string, object?>? ToolOptions { get; set; }

    /// <summary>
    /// 텍스트 생성 매개변수입니다.
    /// </summary>
    public AgentParametersConfig? Parameters { get; set; }

    /// <summary>
    /// 설정 유효성을 검증합니다.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Agent name is required.", nameof(Name));
        if (string.IsNullOrWhiteSpace(Provider))
            throw new ArgumentException("Agent provider is required.", nameof(Provider));
        if (string.IsNullOrWhiteSpace(Model))
            throw new ArgumentException("Agent model is required.", nameof(Model));
    }

    /// <summary>
    /// ToolItem 목록으로 변환합니다.
    /// </summary>
    public IEnumerable<ToolItem>? ToToolItems()
    {
        if (Tools == null || Tools.Count == 0)
            return null;

        return Tools.Select(toolName => new ToolItem
        {
            Name = toolName,
            Options = ToolOptions?.GetValueOrDefault(toolName)
        });
    }

    /// <summary>
    /// MessageGenerationParameters로 변환합니다.
    /// </summary>
    public MessageGenerationParameters? ToParameters()
    {
        if (Parameters == null)
            return null;

        return new MessageGenerationParameters
        {
            MaxTokens = Parameters.MaxTokens,
            Temperature = Parameters.Temperature,
            TopP = Parameters.TopP,
            TopK = Parameters.TopK,
            StopSequences = Parameters.StopSequences
        };
    }
}

/// <summary>
/// 생성 매개변수 설정 DTO입니다.
/// </summary>
internal class AgentParametersConfig
{
    public int? MaxTokens { get; set; }
    public float? Temperature { get; set; }
    public float? TopP { get; set; }
    public int? TopK { get; set; }
    public List<string>? StopSequences { get; set; }
}

/// <summary>
/// YAML/TOML 파싱 시 루트 래퍼 클래스입니다.
/// </summary>
internal class AgentConfigRoot
{
    public AgentConfig Agent { get; set; } = new();
}

namespace IronHive.Abstractions.Agent;

/// <summary>
/// Agent 설정을 위한 DTO 클래스입니다.
/// YAML, TOML, JSON에서 역직렬화하거나 프로그래밍 방식으로 구성할 때 사용됩니다.
/// </summary>
public class AgentConfig
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
}

/// <summary>
/// 생성 매개변수 설정 DTO입니다.
/// </summary>
public class AgentParametersConfig
{
    public int? MaxTokens { get; set; }
    public float? Temperature { get; set; }
    public float? TopP { get; set; }
    public int? TopK { get; set; }
    public List<string>? StopSequences { get; set; }
}

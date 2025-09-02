using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Agent;

/// <summary>
/// Agent 서비스를 나타냅니다.
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Agent가 사용하는 도구 모음입니다.
    /// </summary>
    IToolCollection Tools { get; }

    /// <summary>
    /// 에이전트 서비스를 Yaml 문자열로부터 생성합니다.
    /// 
    /// <code>
    /// agent:
    ///     name: "SupportBot"
    ///     description: "고객 문의를 처리하는 챗 에이전트"
    ///     defaultProvider: "openai"
    ///     defaultModel: "gpt-4o-mini"
    ///     instructions: |
    ///         당신은 고객 상담원입니다.친절하고 간결하게 답하세요.
    ///     tools: ["web-search", "calculator"]
    ///     toolOptions:
    ///         web-search:
    ///             region: "KR"
    ///             safe: true
    ///         calculator: {}
    ///     parameters:
    ///         maxTokens: 512
    ///         temperature: 0.3
    ///         topP: 0.9
    ///         stopSequences: ["\nUser:"]
    /// </code>
    /// </summary>
    IAgent CreateAgentFromYaml(string yaml);

    /// <summary>
    /// 에이전트 서비스를 TOML 문자열로부터 생성합니다.
    /// 
    /// <code>
    /// [agent]
    /// name = "SupportBot"
    /// description = "고객 문의를 처리하는 챗 에이전트"
    /// defaultProvider = "openai"
    /// defaultModel = "gpt-4o-mini"
    /// instructions = """
    /// 당신은 고객 상담원입니다. 친절하고 간결하게 답하세요.
    /// """
    /// tools = ["web-search", "calculator"]
    /// 
    /// [agent.toolOptions.web - search]
    /// region = "KR"
    /// safe = true
    /// 
    /// [agent.toolOptions.calculator]
    /// 
    /// [agent.parameters]
    /// maxTokens = 512
    /// temperature = 0.3
    /// topP = 0.9
    /// stopSequences = ["\nUser:"]
    /// </code>
    /// </summary>
    IAgent CreateAgentFromToml(string toml);

    /// <summary>
    /// 에이전트 서비스를 JSON 문자열로부터 생성합니다.
    /// </summary>
    IAgent CreateAgentFromJson(string json);
}

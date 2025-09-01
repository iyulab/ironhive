using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions;

/// <summary>
/// HiveService 인터페이스
/// </summary>
public interface IHiveService
{
    /// <summary>
    /// 서비스 제공자
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    /// 파일 서비스
    /// </summary>
    IFileService Files { get; }

    /// <summary>
    /// 메모리 서비스
    /// </summary>
    IMemoryService Memory { get; }

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
    IHiveAgent CreateAgentFromYaml(string yaml);

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
    IHiveAgent CreateAgentFromToml(string toml);

    /// <summary>
    /// 에이전트 서비스를 JSON 문자열로부터 생성합니다.
    /// </summary>
    IHiveAgent CreateAgentFromJson(string json);

    /// <summary>
    /// 모델 제공자를 등록합니다. 기존 제공자가 있으면 대체됩니다.
    /// </summary>
    IHiveService SetModelCatalogProvider(IModelCatalogProvider provider);

    /// <summary>
    /// 메시지 생성기를 등록합니다. 기존 생성기가 있으면 대체됩니다.
    /// </summary>
    IHiveService SetMessageGenerator(IMessageGenerator generator);

    /// <summary>
    /// 임베딩 생성기를 등록합니다. 기존 생성기가 있으면 대체됩니다.
    /// </summary>
    IHiveService SetEmbeddingGenerator(IEmbeddingGenerator generator);

    /// <summary>
    /// 지정된 이름에 해당하는 모델 제공자를 제거합니다.
    /// </summary>
    IHiveService RemoveModelCatalogProvider(string name);

    /// <summary>
    /// 지정된 이름에 해당하는 메시지 생성기를 제거합니다.
    /// </summary>
    IHiveService RemoveMessageGenerator(string name);

    /// <summary>
    /// 지정된 이름에 해당하는 임베딩 생성기를 제거합니다.
    /// </summary>
    IHiveService RemoveEmbeddingGenerator(string name);
}

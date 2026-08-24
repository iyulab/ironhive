using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Agent;

/// <summary>
/// <see cref="IAgent"/> invoke 호출 단위로 적용되는 per-request 옵션입니다.
/// 에이전트 고정 속성(Provider/Model/Instructions) 위에 overlay되며,
/// null 필드는 에이전트 기본값을 유지합니다.
/// </summary>
public class AgentInvokeOptions
{
    /// <summary>
    /// 이전 응답의 ResponseId. 프로바이더 측 저장된 컨텍스트를 재사용해 비용을 절감합니다.
    /// </summary>
    public string? PreviousId { get; set; }

    /// <summary>
    /// 모델의 사고 노력 수준을 정의합니다.
    /// </summary>
    public MessageThinkingEffort? ThinkingEffort { get; set; }

    /// <summary>
    /// 생성할 최대 토큰 수입니다. 설정 시 에이전트의 <see cref="IAgent.MaxTokens"/> 기본값을 override합니다.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Sampling temperature. Higher values make output more random.
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Nucleus sampling threshold.
    /// </summary>
    public float? TopP { get; set; }

    /// <summary>
    /// Top-K sampling cutoff. Ignored by providers that do not expose it.
    /// </summary>
    public int? TopK { get; set; }

    /// <summary>
    /// Sequences that stop generation when produced.
    /// </summary>
    public ICollection<string>? StopSequences { get; set; }


    /// <summary>
    /// 이 호출에서 사용할 도구 집합입니다. 설정 시 <see cref="IAgent.Tools"/>를 이 호출에 한해
    /// override합니다(다른 필드와 동일한 overlay 규약 — 다른 호출/에이전트 상태에는 영향 없음).
    /// <see cref="IAgent"/> 인스턴스는 이름으로 캐시·공유되는 경우가 있어(예: Ironbees
    /// <c>AgentRegistry</c>) 세션·요청마다 달라지는 도구 집합(예: 워크스페이스 스코프 도구)을
    /// 표현하려면 공유 인스턴스의 <see cref="IAgent.Tools"/>를 직접 변경하는 대신 이 옵션을
    /// 사용해야 동시 요청 간 경합이 발생하지 않습니다. null이면 <see cref="IAgent.Tools"/>가
    /// 그대로 사용됩니다.
    /// </summary>
    public IToolCollection? Tools { get; set; }

    /// <summary>
    /// 도구 실행 동작 설정입니다. null이면 기본값이 사용됩니다.
    /// </summary>
    public ToolOptions? ToolOptions { get; set; }

    /// <summary>
    /// 구조화 출력 설정입니다. null이면 기본 텍스트 출력입니다.
    /// </summary>
    public OutputFormat? OutputFormat { get; set; }

    /// <summary>
    /// 제안 기능 옵션입니다. null이면 비활성화됩니다.
    /// </summary>
    public SuggestionOptions? Suggestions { get; set; }

    /// <summary>
    /// 툴 사용 최대 턴 수입니다. null이면 <see cref="MessageRequest"/> 기본값(50)이 사용됩니다.
    /// </summary>
    public int? MaxTurns { get; set; }

    /// <summary>
    /// 파이프라인(MessageContext)에 초기값으로 전달되는 공유 데이터입니다.
    /// </summary>
    public MessageContextItems? Items { get; set; }
}

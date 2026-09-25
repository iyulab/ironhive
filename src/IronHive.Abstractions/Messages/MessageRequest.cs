using System.Text.Json.Nodes;
using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 메시지 생성 서비스에 전달되는 요청입니다.
/// </summary>
public class MessageRequest
{
    /// <summary>
    /// 이전 응답의 ResponseId. 프로바이더 측 저장된 컨텍스트를 재사용해 비용을 절감합니다.
    /// </summary>
    public string? PreviousId { get; set; }

    /// <summary>
    /// 메시지 생성에 사용될 AI 서비스 제공자의 식별자입니다.
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// 사용할 특정 모델의 식별자입니다.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// 모델의 사고 노력 수준을 정의합니다.
    /// null 이면 추론에 대해 아무것도 보내지 않아 provider·모델 기본 동작을 따릅니다 — 기본으로 추론하는 모델
    /// (예: OpenAI 호환 서버의 Qwen·DeepSeek 계열)은 추론합니다. 추론을 끄려면 <see cref="MessageThinkingEffort.None"/>
    /// 을 명시합니다.
    /// </summary>
    public MessageThinkingEffort? ThinkingEffort { get; set; }

    /// <summary>
    /// 응답에 추론 내용을 얼마나 실을지입니다. null 이면 provider 기본 동작을 따릅니다.
    /// </summary>
    public MessageThinkingOutput? ThinkingOutput { get; set; }

    /// <summary>
    /// 생성할 최대 토큰 수입니다.
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
    /// Provider-specific request fields, deep-merged into the JSON body the provider sends (an object merges into an
    /// object; any other value replaces the one there, including a field this library sets). For server extensions no
    /// typed member models, e.g. a llama.cpp or vLLM sampling option. Honoured by the OpenAI-compatible
    /// (Chat Completions) provider; a provider that cannot extend its request body throws
    /// <see cref="NotSupportedException"/> when this has entries rather than dropping them.
    /// </summary>
    public JsonObject? ExtraBody { get; set; }

    /// <summary>
    /// Asks for the log probability of each output token (and, optionally, the most likely alternatives). The response
    /// carries them in <c>LogProbabilities</c>. <see langword="null"/> requests none. A provider that cannot return them
    /// throws <see cref="NotSupportedException"/> rather than answering without them.
    /// </summary>
    public LogProbabilityOptions? LogProbabilities { get; set; }

    /// <summary>
    /// Sequences that stop generation when produced.
    /// </summary>
    public ICollection<string>? StopSequences { get; set; }


    /// <summary>
    /// 대화의 컨텍스트와 동작을 정의하는 시스템 프롬프트입니다.
    /// </summary>
    public string? System { get; set; }

    /// <summary>
    /// 모델에 전달될 대화 메시지 컬렉션입니다.
    /// </summary>
    public ICollection<Message> Messages { get; set; } = [];

    /// <summary>
    /// 모델이 사용 가능한 도구 컬렉션입니다. null이면 도구를 사용하지 않습니다.
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
    /// 툴 사용에서 무한 루프에 빠질 수 있는 상황을 방지하기 위한 최대 턴 수입니다. 기본값은 50입니다.
    /// </summary>
    public int MaxTurns { get; set; } = 50;

    /// <summary>
    /// 파이프라인(MessageContext)에 초기값으로 전달되는 공유 데이터입니다.
    /// </summary>
    public MessageContextItems Items { get; set; } = new();
}

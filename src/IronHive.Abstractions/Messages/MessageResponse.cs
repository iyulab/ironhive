using System.Text.Json.Nodes;

namespace IronHive.Abstractions.Messages;

public class MessageResponse
{
    /// <summary>
    /// 프로바이더 발급 ID ({provider}_ prefix 포함). tool_call 루프의 마지막 값.
    /// 다음 요청의 PreviousId로 사용하면 비용 절감 가능. Google은 미지원으로 null.
    /// </summary>
    public string? ResponseId { get; set; }

    public MessageDoneReason? DoneReason { get; set; }

    public Message? Message { get; set; }

    /// <summary>
    /// 모델이 생성한 제안 목록입니다. MessageRequest.Suggestions가 설정된 경우에만 포함됩니다.
    /// </summary>
    public List<Suggestion>? Suggestions { get; set; }

    public MessageTokenUsage? TokenUsage { get; set; }

    /// <summary>
    /// Top-level fields of the provider's response body that no typed member maps — e.g. llama.cpp's
    /// <c>timings</c> (<c>prompt_ms</c>, <c>predicted_ms</c>). Per-choice fields are not included. From the last
    /// generation call of the request. <see langword="null"/> when the response had none, or the provider does not
    /// collect them (the OpenAI-compatible provider does).
    /// </summary>
    public JsonObject? ExtraBody { get; set; }

    /// <summary>
    /// The log probability of each output text token, in order, when the request asked for them
    /// (<c>LogProbabilities</c>); <see langword="null"/> otherwise. From the last generation call of the request.
    /// </summary>
    public IReadOnlyList<TokenLogProbability>? LogProbabilities { get; set; }

    public string? Model { get; set; }

    public TimeSpan? Duration { get; set; }

    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// 파이프라인(MessageContext)이 처리 과정에서 누적한 공유 데이터입니다.
    /// </summary>
    public MessageContextItems Items { get; set; } = new();
}

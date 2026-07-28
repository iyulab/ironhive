using System.Diagnostics;
using IronHive.Abstractions.Messages.Content;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// MessageService의 한 호출(GenerateMessageAsync/GenerateStreamingMessageAsync) 동안
/// 미들웨어 파이프라인과 턴(turn) 루프가 공유하는 실행 컨텍스트입니다.
/// </summary>
public class MessageContext
{
    /// <summary>
    /// 현재 턴에서 제너레이터에 전달되는 요청입니다. 턴을 거치며 Messages 등이 누적됩니다.
    /// </summary>
    public MessageGenerationRequest Request { get; set; }

    /// <summary>
    /// 이번 호출에서 허용되는 최대 턴 수입니다(최소 1로 보정됨).
    /// </summary>
    public int MaxTurns { get; }

    /// <summary>
    /// 현재 턴 인덱스입니다(0부터 시작).
    /// </summary>
    public int CurrentTurn { get; internal set; }

    /// <summary>
    /// 가장 최근 턴에서 제너레이터가 발급한 원본(prefix 없는) 응답 ID입니다.
    /// </summary>
    public string? TrackedId { get; internal set; }

    /// <summary>
    /// 가장 최근 턴의 종료 사유입니다. 루프가 끝나면 최종 DoneReason이 됩니다.
    /// </summary>
    public MessageDoneReason? TurnReason { get; internal set; }

    /// <summary>
    /// 가장 최근 턴의 토큰 사용량입니다(누적이 아닌 마지막 턴 값).
    /// </summary>
    public MessageTokenUsage? TokenUsage { get; internal set; }

    /// <summary>
    /// 턴을 거치며 계속 누적되는 assistant 메시지입니다. 아직 아무 컨텐츠도 생성되지 않았다면 null입니다.
    /// </summary>
    public Message? CurrentMessage { get; internal set; }

    /// <summary>
    /// 컨텍스트 생성 이후 경과 시간입니다.
    /// </summary>
    public TimeSpan Elapsed => _timer.Elapsed;

    /// <summary>
    /// 파이프라인 단계(미들웨어) 간 공유되는 데이터입니다. MessageRequest.Items로 시작해
    /// MessageResponse.Items/StreamingMessageDoneResponse.Items로 흘러나갑니다.
    /// </summary>
    public MessageContextItems Items { get; }

    private readonly Stopwatch _timer = Stopwatch.StartNew();

    // 생성과 턴 상태(CurrentTurn/TrackedId/TurnReason 등) 갱신을 MessageService로 한정합니다.
    // 미들웨어가 임의로 컨텍스트를 만들거나 턴 계정(bookkeeping) 값을 조작해서
    // 루프 제어(ShouldContinue 판단 등)를 우회하지 못하게 막기 위함입니다.
    internal MessageContext(MessageRequest request, Action<MessageGenerationRequest>? configure = null)
    {
        Request = new MessageGenerationRequest
        {
            PreviousId = request.PreviousId,
            Model = request.Model,
            ThinkingEffort = request.ThinkingEffort,
            Messages = new List<Message>(request.Messages),
            System = request.System,
            Tools = request.Tools,
            OutputFormat = request.OutputFormat,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            TopP = request.TopP,
            TopK = request.TopK,
            StopSequences = request.StopSequences,
        };
        configure?.Invoke(Request);

        MaxTurns = Math.Max(1, request.MaxTurns);
        Items = new MessageContextItems(request.Items);

        // 이전 호출에서 승인 대기 중이던 assistant 메시지가 있다면 이어받습니다(재개 시나리오).
        if (Request.Messages.LastOrDefault() is { Role: MessageRole.Assistant } last)
        {
            CurrentMessage = last;
        }
    }

    /// <summary>
    /// 이번 턴의 pipeline 호출 전에, CurrentMessage를 Request.Messages에 반영합니다.
    /// CurrentMessage가 없거나 이미 Request.Messages의 마지막 항목이면 아무 것도 하지 않습니다.
    /// </summary>
    internal void BeginTurn()
    {
        if (CurrentMessage != null && !ReferenceEquals(Request.Messages.LastOrDefault(), CurrentMessage))
        {
            Request.Messages.Add(CurrentMessage);
        }
    }

    /// <summary>
    /// 다음 턴을 계속 진행해야 하는지 판단합니다.
    /// </summary>
    public bool ShouldContinue() =>
        TurnReason != MessageDoneReason.EndTurn &&
        TurnReason != MessageDoneReason.StopSequence &&
        !(CurrentMessage?.Content.OfType<ToolMessageContent>().Any(t => !t.IsApproved) ?? false);
}

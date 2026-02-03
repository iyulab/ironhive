using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions.Agent.Orchestration;

/// <summary>
/// 에이전트 오케스트레이션의 실행 결과를 나타냅니다.
/// </summary>
public sealed class OrchestrationResult
{
    /// <summary>
    /// 오케스트레이션이 성공적으로 완료되었는지 여부
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// 최종 출력 메시지
    /// </summary>
    public Message? FinalOutput { get; init; }

    /// <summary>
    /// 각 에이전트의 개별 응답
    /// </summary>
    public IReadOnlyList<AgentStepResult> Steps { get; init; } = [];

    /// <summary>
    /// 총 실행 시간
    /// </summary>
    public TimeSpan TotalDuration { get; init; }

    /// <summary>
    /// 총 사용된 토큰 수 (가용한 경우)
    /// </summary>
    public TokenUsageSummary? TokenUsage { get; init; }

    /// <summary>
    /// 오류 메시지 (실패 시)
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// 성공 결과 생성
    /// </summary>
    public static OrchestrationResult Success(
        Message finalOutput,
        IReadOnlyList<AgentStepResult> steps,
        TimeSpan duration,
        TokenUsageSummary? tokenUsage = null) => new()
    {
        IsSuccess = true,
        FinalOutput = finalOutput,
        Steps = steps,
        TotalDuration = duration,
        TokenUsage = tokenUsage
    };

    /// <summary>
    /// 실패 결과 생성
    /// </summary>
    public static OrchestrationResult Failure(
        string error,
        IReadOnlyList<AgentStepResult>? steps = null,
        TimeSpan? duration = null) => new()
    {
        IsSuccess = false,
        Error = error,
        Steps = steps ?? [],
        TotalDuration = duration ?? TimeSpan.Zero
    };
}

/// <summary>
/// 개별 에이전트 실행 단계의 결과
/// </summary>
public sealed class AgentStepResult
{
    /// <summary>
    /// 실행된 에이전트 이름
    /// </summary>
    public required string AgentName { get; init; }

    /// <summary>
    /// 에이전트에 전달된 입력 메시지
    /// </summary>
    public IReadOnlyList<Message> Input { get; init; } = [];

    /// <summary>
    /// 에이전트의 응답
    /// </summary>
    public MessageResponse? Response { get; init; }

    /// <summary>
    /// 이 단계의 실행 시간
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// 단계가 성공했는지 여부
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// 오류 메시지 (실패 시)
    /// </summary>
    public string? Error { get; init; }
}

/// <summary>
/// 토큰 사용량 요약
/// </summary>
public sealed class TokenUsageSummary
{
    /// <summary>
    /// 총 입력 토큰 수
    /// </summary>
    public int TotalInputTokens { get; init; }

    /// <summary>
    /// 총 출력 토큰 수
    /// </summary>
    public int TotalOutputTokens { get; init; }

    /// <summary>
    /// 총 토큰 수
    /// </summary>
    public int TotalTokens => TotalInputTokens + TotalOutputTokens;
}

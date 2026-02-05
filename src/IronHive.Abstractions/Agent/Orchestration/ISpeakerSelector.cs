using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions.Agent.Orchestration;

/// <summary>
/// GroupChat에서 다음 발언자를 선택하는 전략
/// </summary>
public interface ISpeakerSelector
{
    /// <summary>
    /// 다음 발언자를 선택합니다.
    /// null 반환 시 대화가 종료됩니다.
    /// </summary>
    /// <param name="steps">지금까지 실행된 에이전트 단계 결과</param>
    /// <param name="conversationHistory">초기 메시지를 포함한 전체 대화 히스토리</param>
    /// <param name="agents">선택 가능한 에이전트 목록</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<string?> SelectNextSpeakerAsync(
        IReadOnlyList<AgentStepResult> steps,
        IReadOnlyList<Message> conversationHistory,
        IReadOnlyList<IAgent> agents,
        CancellationToken cancellationToken = default);
}

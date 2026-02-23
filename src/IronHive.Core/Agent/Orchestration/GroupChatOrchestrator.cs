using System.Diagnostics;
using System.Runtime.CompilerServices;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 관리자 기반 반복 토론/정제 패턴을 지원하는 GroupChat 오케스트레이터입니다.
/// </summary>
public class GroupChatOrchestrator : OrchestratorBase
{
    private readonly Dictionary<string, IAgent> _agentMap = [];

    private new GroupChatOrchestratorOptions Options => (GroupChatOrchestratorOptions)base.Options;

    internal GroupChatOrchestrator(
        GroupChatOrchestratorOptions options,
        Dictionary<string, IAgent> agentMap)
        : base(options)
    {
        _agentMap = agentMap;
        AddAgents(agentMap.Values);
    }

    /// <inheritdoc />
    public override async Task<OrchestrationResult> ExecuteAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default)
    {
        if (Agents.Count == 0)
        {
            return OrchestrationResult.Failure("No agents registered in the orchestrator.");
        }

        var stopwatch = Stopwatch.StartNew();
        var steps = new List<AgentStepResult>();
        var conversationHistory = messages.ToList();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(Options.Timeout);

        try
        {
            // 체크포인트에서 재개
            var checkpoint = await LoadCheckpointAsync(cts.Token).ConfigureAwait(false);
            var completedSteps = checkpoint?.CompletedSteps.ToList() ?? [];
            steps.AddRange(completedSteps);

            // 대화 히스토리 복원
            if (checkpoint?.CurrentMessages.Count > 0)
            {
                conversationHistory = checkpoint.CurrentMessages.ToList();
            }

            // 완료된 라운드 수로 재개 지점 결정
            var startRound = completedSteps.Count;

            for (var round = startRound; round < Options.MaxRounds; round++)
            {
                // 다음 발언자 선택 (초기 메시지 포함 대화 히스토리 전달)
                var nextSpeaker = await Options.SpeakerSelector.SelectNextSpeakerAsync(
                    steps, conversationHistory, Agents, cts.Token).ConfigureAwait(false);

                if (nextSpeaker == null)
                    break; // 발언자 없음 → 종료

                if (!_agentMap.TryGetValue(nextSpeaker, out var agent))
                {
                    return OrchestrationResult.Failure(
                        $"Selected speaker '{nextSpeaker}' not found.",
                        steps, stopwatch.Elapsed);
                }

                // 승인 체크
                if (!await CheckApprovalAsync(agent, steps.LastOrDefault(), cts.Token).ConfigureAwait(false))
                {
                    await SaveCheckpointAsync(steps, conversationHistory, cts.Token).ConfigureAwait(false);
                    stopwatch.Stop();
                    return OrchestrationResult.Failure(
                        $"Approval denied for agent '{nextSpeaker}'",
                        steps,
                        stopwatch.Elapsed);
                }

                // 에이전트 실행
                var step = await ExecuteAgentAsync(agent, conversationHistory, cts.Token)
                    .ConfigureAwait(false);
                steps.Add(step);

                if (!step.IsSuccess)
                {
                    if (Options.StopOnAgentFailure)
                    {
                        await SaveCheckpointAsync(steps, conversationHistory, cts.Token).ConfigureAwait(false);
                        return OrchestrationResult.Failure(
                            step.Error ?? $"Agent '{nextSpeaker}' failed.",
                            steps, stopwatch.Elapsed);
                    }
                    continue;
                }

                // 응답을 대화 히스토리에 추가
                var responseMessage = ExtractMessage(step.Response);
                if (responseMessage != null)
                {
                    conversationHistory.Add(responseMessage);
                }

                // 라운드 완료 후 체크포인트 저장
                await SaveCheckpointAsync(steps, conversationHistory, cts.Token).ConfigureAwait(false);

                // 종료 조건 체크
                var shouldTerminate = await Options.TerminationCondition.ShouldTerminateAsync(
                    steps, cts.Token).ConfigureAwait(false);

                if (shouldTerminate)
                    break;
            }

            stopwatch.Stop();

            var lastStep = steps.LastOrDefault();
            var finalMessage = lastStep?.Response?.Message as Message
                ?? new AssistantMessage
                {
                    Content = [new TextMessageContent { Value = "GroupChat completed." }]
                };

            // 성공 시 체크포인트 삭제
            await DeleteCheckpointAsync(cancellationToken).ConfigureAwait(false);

            return OrchestrationResult.Success(
                finalMessage,
                steps,
                stopwatch.Elapsed,
                AggregateTokenUsage(steps));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return OrchestrationResult.Failure(
                $"GroupChat timed out after {Options.Timeout.TotalSeconds}s.",
                steps, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return OrchestrationResult.Failure(
                $"GroupChat failed: {ex.Message}",
                steps, stopwatch.Elapsed);
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        IEnumerable<Message> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // ExecuteAsync에 위임하여 체크포인트/승인 로직 일관성 유지
        var result = await ExecuteAsync(messages, cancellationToken).ConfigureAwait(false);

        yield return new OrchestrationStreamEvent { EventType = OrchestrationEventType.Started };

        foreach (var step in result.Steps)
        {
            yield return new OrchestrationStreamEvent
            {
                EventType = OrchestrationEventType.SpeakerSelected,
                AgentName = step.AgentName
            };

            yield return new OrchestrationStreamEvent
            {
                EventType = OrchestrationEventType.AgentStarted,
                AgentName = step.AgentName
            };

            yield return new OrchestrationStreamEvent
            {
                EventType = step.IsSuccess
                    ? OrchestrationEventType.AgentCompleted
                    : OrchestrationEventType.AgentFailed,
                AgentName = step.AgentName,
                CompletedResponse = step.Response,
                Error = step.Error
            };
        }

        yield return new OrchestrationStreamEvent
        {
            EventType = result.IsSuccess
                ? OrchestrationEventType.Completed
                : OrchestrationEventType.Failed,
            Result = result,
            Error = result.Error
        };
    }
}

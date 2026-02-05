using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
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
            for (var round = 0; round < Options.MaxRounds; round++)
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

                // 에이전트 실행
                var step = await ExecuteAgentAsync(agent, conversationHistory, cts.Token)
                    .ConfigureAwait(false);
                steps.Add(step);

                if (!step.IsSuccess)
                {
                    if (Options.StopOnAgentFailure)
                    {
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
        var channel = Channel.CreateUnbounded<OrchestrationStreamEvent>();

        var executeTask = Task.Run(async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            var steps = new List<AgentStepResult>();
            var conversationHistory = messages.ToList();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(Options.Timeout);

            try
            {
                await channel.Writer.WriteAsync(new OrchestrationStreamEvent
                {
                    EventType = OrchestrationEventType.Started
                }, cts.Token).ConfigureAwait(false);

                for (var round = 0; round < Options.MaxRounds; round++)
                {
                    var nextSpeaker = await Options.SpeakerSelector.SelectNextSpeakerAsync(
                        steps, conversationHistory, Agents, cts.Token).ConfigureAwait(false);

                    if (nextSpeaker == null)
                        break;

                    if (!_agentMap.TryGetValue(nextSpeaker, out var agent))
                    {
                        await channel.Writer.WriteAsync(new OrchestrationStreamEvent
                        {
                            EventType = OrchestrationEventType.Failed,
                            Error = $"Selected speaker '{nextSpeaker}' not found."
                        }, cts.Token).ConfigureAwait(false);
                        return;
                    }

                    await channel.Writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.SpeakerSelected,
                        AgentName = nextSpeaker
                    }, cts.Token).ConfigureAwait(false);

                    await channel.Writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.AgentStarted,
                        AgentName = nextSpeaker
                    }, cts.Token).ConfigureAwait(false);

                    var step = await ExecuteAgentAsync(agent, conversationHistory, cts.Token)
                        .ConfigureAwait(false);
                    steps.Add(step);

                    if (!step.IsSuccess)
                    {
                        await channel.Writer.WriteAsync(new OrchestrationStreamEvent
                        {
                            EventType = OrchestrationEventType.AgentFailed,
                            AgentName = nextSpeaker,
                            Error = step.Error
                        }, cts.Token).ConfigureAwait(false);

                        if (Options.StopOnAgentFailure)
                        {
                            await channel.Writer.WriteAsync(new OrchestrationStreamEvent
                            {
                                EventType = OrchestrationEventType.Failed,
                                Error = step.Error,
                                Result = OrchestrationResult.Failure(step.Error ?? "Agent failed.", steps, stopwatch.Elapsed)
                            }, cts.Token).ConfigureAwait(false);
                            return;
                        }
                        continue;
                    }

                    await channel.Writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.AgentCompleted,
                        AgentName = nextSpeaker,
                        CompletedResponse = step.Response
                    }, cts.Token).ConfigureAwait(false);

                    var responseMessage = ExtractMessage(step.Response);
                    if (responseMessage != null)
                    {
                        conversationHistory.Add(responseMessage);
                    }

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

                await channel.Writer.WriteAsync(new OrchestrationStreamEvent
                {
                    EventType = OrchestrationEventType.Completed,
                    Result = OrchestrationResult.Success(finalMessage, steps, stopwatch.Elapsed, AggregateTokenUsage(steps))
                }, cts.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                await channel.Writer.WriteAsync(new OrchestrationStreamEvent
                {
                    EventType = OrchestrationEventType.Failed,
                    Error = ex.Message,
                    Result = OrchestrationResult.Failure(ex.Message, steps, stopwatch.Elapsed)
                }, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, cancellationToken);

        await foreach (var evt in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return evt;
        }

        await executeTask.ConfigureAwait(false);
    }
}

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 에이전트를 순차적으로 실행하는 오케스트레이터입니다.
/// 각 에이전트의 출력이 다음 에이전트의 입력으로 전달됩니다.
/// </summary>
public class SequentialOrchestrator : OrchestratorBase
{
    private new SequentialOrchestratorOptions Options => (SequentialOrchestratorOptions)base.Options;

    public SequentialOrchestrator(SequentialOrchestratorOptions? options = null)
        : base(options ?? new SequentialOrchestratorOptions())
    {
    }

    /// <inheritdoc />
    public override async Task<OrchestrationResult> ExecuteAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default)
    {
        if (!Agents.Any())
        {
            return OrchestrationResult.Failure("No agents registered in the orchestrator.");
        }

        var stopwatch = Stopwatch.StartNew();
        var steps = new List<AgentStepResult>();
        var currentMessages = messages.ToList();
        var accumulatedMessages = new List<Message>(currentMessages);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(Options.Timeout);

        try
        {
            foreach (var agent in Agents)
            {
                var input = Options.AccumulateHistory ? accumulatedMessages : currentMessages;
                var stepResult = await ExecuteAgentAsync(agent, input, cts.Token).ConfigureAwait(false);
                steps.Add(stepResult);

                if (!stepResult.IsSuccess)
                {
                    if (Options.StopOnAgentFailure)
                    {
                        stopwatch.Stop();
                        return OrchestrationResult.Failure(
                            stepResult.Error ?? $"Agent '{agent.Name}' failed",
                            steps,
                            stopwatch.Elapsed);
                    }
                    continue;
                }

                // 다음 에이전트를 위한 입력 준비
                var outputMessage = ExtractMessage(stepResult.Response);
                if (outputMessage != null && Options.PassOutputAsInput)
                {
                    if (Options.AccumulateHistory)
                    {
                        accumulatedMessages.Add(outputMessage);
                    }
                    else
                    {
                        currentMessages = [outputMessage];
                    }
                }
            }

            stopwatch.Stop();

            // 마지막 성공 응답을 최종 출력으로 사용
            var lastSuccessStep = steps.LastOrDefault(s => s.IsSuccess);
            var finalOutput = ExtractMessage(lastSuccessStep?.Response);

            if (finalOutput == null)
            {
                return OrchestrationResult.Failure(
                    "No successful agent output",
                    steps,
                    stopwatch.Elapsed);
            }

            return OrchestrationResult.Success(
                finalOutput,
                steps,
                stopwatch.Elapsed,
                AggregateTokenUsage(steps));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return OrchestrationResult.Failure(
                $"Orchestration timed out after {Options.Timeout.TotalSeconds}s",
                steps,
                stopwatch.Elapsed);
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        IEnumerable<Message> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!Agents.Any())
        {
            yield return new OrchestrationStreamEvent
            {
                EventType = OrchestrationEventType.Failed,
                Error = "No agents registered in the orchestrator."
            };
            yield break;
        }

        // Channel을 사용하여 try-catch 내부의 yield 제한을 우회
        var channel = Channel.CreateUnbounded<OrchestrationStreamEvent>();

        var producerTask = ProduceStreamingEventsAsync(channel.Writer, messages, cancellationToken);

        await foreach (var streamEvent in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return streamEvent;
        }

        // producer가 완료된 후 예외 전파
        await producerTask.ConfigureAwait(false);
    }

    private async Task ProduceStreamingEventsAsync(
        ChannelWriter<OrchestrationStreamEvent> writer,
        IEnumerable<Message> messages,
        CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var steps = new List<AgentStepResult>();
            var currentMessages = messages.ToList();
            var accumulatedMessages = new List<Message>(currentMessages);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(Options.Timeout);

            await writer.WriteAsync(
                new OrchestrationStreamEvent { EventType = OrchestrationEventType.Started },
                cancellationToken).ConfigureAwait(false);

            var timedOut = false;

            foreach (var agent in Agents)
            {
                await writer.WriteAsync(new OrchestrationStreamEvent
                {
                    EventType = OrchestrationEventType.AgentStarted,
                    AgentName = agent.Name
                }, cancellationToken).ConfigureAwait(false);

                var input = Options.AccumulateHistory ? accumulatedMessages : currentMessages;
                var agentStopwatch = Stopwatch.StartNew();

                var textBuilder = new StringBuilder();
                StreamingMessageDoneResponse? doneResponse = null;

                try
                {
                    using var agentCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
                    agentCts.CancelAfter(Options.AgentTimeout);

                    await foreach (var chunk in agent.InvokeStreamingAsync(input, agentCts.Token).ConfigureAwait(false))
                    {
                        // 실시간으로 MessageDelta 이벤트 발행
                        await writer.WriteAsync(new OrchestrationStreamEvent
                        {
                            EventType = OrchestrationEventType.MessageDelta,
                            AgentName = agent.Name,
                            StreamingResponse = chunk
                        }, cancellationToken).ConfigureAwait(false);

                        // 텍스트 컨텐츠 수집 (다음 에이전트 입력용)
                        switch (chunk)
                        {
                            case StreamingContentDeltaResponse delta:
                                if (delta.Delta is TextDeltaContent textDelta)
                                {
                                    textBuilder.Append(textDelta.Value);
                                }
                                break;
                            case StreamingMessageDoneResponse done:
                                doneResponse = done;
                                break;
                        }
                    }

                    agentStopwatch.Stop();

                    // 성공 시 AgentStepResult 구성
                    var responseMessage = new AssistantMessage
                    {
                        Content = [new TextMessageContent { Value = textBuilder.ToString() }]
                    };

                    var response = new MessageResponse
                    {
                        Id = doneResponse?.Id ?? Guid.NewGuid().ToString(),
                        DoneReason = doneResponse?.DoneReason,
                        Message = responseMessage,
                        TokenUsage = doneResponse?.TokenUsage,
                        Timestamp = doneResponse?.Timestamp ?? DateTime.UtcNow
                    };

                    var stepResult = new AgentStepResult
                    {
                        AgentName = agent.Name,
                        Input = input.ToList(),
                        Response = response,
                        Duration = agentStopwatch.Elapsed,
                        IsSuccess = true
                    };
                    steps.Add(stepResult);

                    await writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.AgentCompleted,
                        AgentName = agent.Name,
                        CompletedResponse = response
                    }, cancellationToken).ConfigureAwait(false);

                    // 다음 에이전트를 위한 입력 준비
                    if (Options.PassOutputAsInput)
                    {
                        if (Options.AccumulateHistory)
                        {
                            accumulatedMessages.Add(responseMessage);
                        }
                        else
                        {
                            currentMessages = [responseMessage];
                        }
                    }
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && !cts.Token.IsCancellationRequested)
                {
                    // 개별 에이전트 타임아웃
                    agentStopwatch.Stop();
                    var errorMessage = $"Agent '{agent.Name}' timed out after {Options.AgentTimeout.TotalSeconds}s";

                    steps.Add(new AgentStepResult
                    {
                        AgentName = agent.Name,
                        Input = input.ToList(),
                        Duration = agentStopwatch.Elapsed,
                        IsSuccess = false,
                        Error = errorMessage
                    });

                    await writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.AgentFailed,
                        AgentName = agent.Name,
                        Error = errorMessage
                    }, cancellationToken).ConfigureAwait(false);

                    if (Options.StopOnAgentFailure)
                    {
                        break;
                    }
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // 전체 오케스트레이션 타임아웃
                    agentStopwatch.Stop();
                    timedOut = true;

                    steps.Add(new AgentStepResult
                    {
                        AgentName = agent.Name,
                        Input = input.ToList(),
                        Duration = agentStopwatch.Elapsed,
                        IsSuccess = false,
                        Error = $"Orchestration timed out after {Options.Timeout.TotalSeconds}s"
                    });

                    await writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.AgentFailed,
                        AgentName = agent.Name,
                        Error = $"Orchestration timed out after {Options.Timeout.TotalSeconds}s"
                    }, cancellationToken).ConfigureAwait(false);
                    break;
                }
                catch (Exception ex)
                {
                    agentStopwatch.Stop();
                    var errorMessage = $"Agent '{agent.Name}' failed: {ex.Message}";

                    steps.Add(new AgentStepResult
                    {
                        AgentName = agent.Name,
                        Input = input.ToList(),
                        Duration = agentStopwatch.Elapsed,
                        IsSuccess = false,
                        Error = errorMessage
                    });

                    await writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.AgentFailed,
                        AgentName = agent.Name,
                        Error = errorMessage
                    }, cancellationToken).ConfigureAwait(false);

                    if (Options.StopOnAgentFailure)
                    {
                        break;
                    }
                }
            }

            stopwatch.Stop();

            if (timedOut)
            {
                await writer.WriteAsync(new OrchestrationStreamEvent
                {
                    EventType = OrchestrationEventType.Failed,
                    Error = $"Orchestration timed out after {Options.Timeout.TotalSeconds}s",
                    Result = OrchestrationResult.Failure(
                        $"Orchestration timed out after {Options.Timeout.TotalSeconds}s",
                        steps,
                        stopwatch.Elapsed)
                }, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var lastSuccess = steps.LastOrDefault(s => s.IsSuccess);
                var finalOutput = ExtractMessage(lastSuccess?.Response);

                if (finalOutput == null)
                {
                    await writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.Failed,
                        Error = "No successful agent output",
                        Result = OrchestrationResult.Failure("No successful agent output", steps, stopwatch.Elapsed)
                    }, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.Completed,
                        Result = OrchestrationResult.Success(
                            finalOutput,
                            steps,
                            stopwatch.Elapsed,
                            AggregateTokenUsage(steps))
                    }, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            writer.Complete();
        }
    }
}

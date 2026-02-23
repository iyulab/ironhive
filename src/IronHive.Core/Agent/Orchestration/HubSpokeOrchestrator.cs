using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// Hub-Spoke 패턴 오케스트레이터입니다.
/// 중앙 Hub 에이전트가 작업을 분배하고 Spoke 에이전트들의 결과를 조율합니다.
/// </summary>
public class HubSpokeOrchestrator : OrchestratorBase
{
    private static readonly JsonSerializerOptions s_caseInsensitiveJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private IAgent? _hubAgent;
    private readonly List<IAgent> _spokeAgents = [];

    private new HubSpokeOrchestratorOptions Options => (HubSpokeOrchestratorOptions)base.Options;

    public HubSpokeOrchestrator(HubSpokeOrchestratorOptions? options = null)
        : base(options ?? new HubSpokeOrchestratorOptions())
    {
    }

    /// <summary>
    /// Hub(조정자) 에이전트를 설정합니다.
    /// </summary>
    public void SetHubAgent(IAgent agent)
    {
        ArgumentNullException.ThrowIfNull(agent);
        _hubAgent = agent;
    }

    /// <summary>
    /// Spoke(작업자) 에이전트를 추가합니다.
    /// </summary>
    public void AddSpokeAgent(IAgent agent)
    {
        ArgumentNullException.ThrowIfNull(agent);
        _spokeAgents.Add(agent);
        AddAgent(agent);
    }

    /// <inheritdoc />
    public override async Task<OrchestrationResult> ExecuteAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default)
    {
        if (_hubAgent == null)
        {
            return OrchestrationResult.Failure("Hub agent is not set. Call SetHubAgent first.");
        }

        if (_spokeAgents.Count == 0)
        {
            return OrchestrationResult.Failure("No spoke agents registered. Call AddSpokeAgent first.");
        }

        var stopwatch = Stopwatch.StartNew();
        var steps = new List<AgentStepResult>();
        var currentMessages = messages.ToList();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(Options.Timeout);

        try
        {
            // 체크포인트에서 재개
            var checkpoint = await LoadCheckpointAsync(cts.Token).ConfigureAwait(false);
            var completedSteps = checkpoint?.CompletedSteps.ToList() ?? [];
            steps.AddRange(completedSteps);

            // 이전 완료 라운드 수 계산 (Hub 에이전트 호출 횟수 = 라운드 수)
            var completedRounds = completedSteps.Count(s => s.AgentName == _hubAgent!.Name);

            // 체크포인트에서 currentMessages 복원
            if (checkpoint?.CurrentMessages.Count > 0)
            {
                currentMessages = checkpoint.CurrentMessages.ToList();
            }

            for (var round = completedRounds; round < Options.MaxRounds; round++)
            {
                // Hub 승인 체크
                var previousStep = steps.LastOrDefault();
                if (!await CheckApprovalAsync(_hubAgent!, previousStep, cts.Token).ConfigureAwait(false))
                {
                    await SaveCheckpointAsync(steps, currentMessages, cts.Token).ConfigureAwait(false);
                    stopwatch.Stop();
                    return OrchestrationResult.Failure(
                        $"Approval denied for hub agent '{_hubAgent!.Name}'",
                        steps,
                        stopwatch.Elapsed);
                }

                // 1. Hub에게 현재 상황을 보내고 지시 받기
                var hubInstruction = await GetHubInstructionAsync(
                    currentMessages, steps, round, cts.Token).ConfigureAwait(false);

                steps.Add(hubInstruction);

                if (!hubInstruction.IsSuccess)
                {
                    await SaveCheckpointAsync(steps, currentMessages, cts.Token).ConfigureAwait(false);
                    return OrchestrationResult.Failure(
                        hubInstruction.Error ?? "Hub agent failed",
                        steps,
                        stopwatch.Elapsed);
                }

                // Hub 응답 파싱
                var hubResponse = ParseHubResponse(hubInstruction.Response);

                if (hubResponse.IsComplete)
                {
                    // Hub가 완료를 선언
                    stopwatch.Stop();
                    var finalMessage = ExtractMessage(hubInstruction.Response);
                    await DeleteCheckpointAsync(cancellationToken).ConfigureAwait(false);
                    return OrchestrationResult.Success(
                        finalMessage ?? new AssistantMessage { Content = [] },
                        steps,
                        stopwatch.Elapsed,
                        AggregateTokenUsage(steps));
                }

                // Spoke 승인 체크
                foreach (var task in hubResponse.Tasks)
                {
                    var spokeAgent = _spokeAgents.FirstOrDefault(a =>
                        a.Name.Equals(task.AgentName, StringComparison.OrdinalIgnoreCase));
                    if (spokeAgent != null)
                    {
                        if (!await CheckApprovalAsync(spokeAgent, steps.LastOrDefault(), cts.Token).ConfigureAwait(false))
                        {
                            await SaveCheckpointAsync(steps, currentMessages, cts.Token).ConfigureAwait(false);
                            stopwatch.Stop();
                            return OrchestrationResult.Failure(
                                $"Approval denied for spoke agent '{spokeAgent.Name}'",
                                steps,
                                stopwatch.Elapsed);
                        }
                    }
                }

                // 2. Spoke 에이전트들 실행
                var spokeResults = await ExecuteSpokesAsync(
                    hubResponse.Tasks, cts.Token).ConfigureAwait(false);

                steps.AddRange(spokeResults);

                // 3. Spoke 결과를 현재 메시지에 추가
                currentMessages = BuildNextRoundMessages(currentMessages, spokeResults);

                // 라운드 완료 후 체크포인트 저장
                await SaveCheckpointAsync(steps, currentMessages, cts.Token).ConfigureAwait(false);
            }

            // 최대 라운드 도달
            stopwatch.Stop();
            var lastHubStep = steps.LastOrDefault(s => s.AgentName == _hubAgent!.Name && s.IsSuccess);
            var output = ExtractMessage(lastHubStep?.Response);

            await DeleteCheckpointAsync(cancellationToken).ConfigureAwait(false);

            return OrchestrationResult.Success(
                output ?? new AssistantMessage { Content = [] },
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

    private async Task<AgentStepResult> GetHubInstructionAsync(
        List<Message> messages,
        List<AgentStepResult> previousSteps,
        int round,
        CancellationToken cancellationToken)
    {
        // Hub에게 보낼 컨텍스트 구성
        var hubMessages = new List<Message>(messages);

        if (round > 0 && previousSteps.Count > 0)
        {
            // 이전 라운드 결과 요약 추가
            var summaryContent = BuildPreviousRoundSummary(previousSteps, round);
            hubMessages.Add(new UserMessage
            {
                Content = [new TextMessageContent { Value = summaryContent }]
            });
        }

        // 사용 가능한 Spoke 에이전트 정보 추가
        var spokeInfo = BuildSpokeAgentInfo();
        hubMessages.Add(new UserMessage
        {
            Content = [new TextMessageContent
            {
                Value = $"Available agents for delegation:\n{spokeInfo}\n\n" +
                        "Respond with JSON: {{\"complete\": true/false, \"tasks\": [{{\"agent\": \"name\", \"instruction\": \"...\"}}]}}"
            }]
        });

        return await ExecuteAgentAsync(_hubAgent!, hubMessages, cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<AgentStepResult>> ExecuteSpokesAsync(
        List<SpokeTask> tasks,
        CancellationToken cancellationToken)
    {
        var results = new List<AgentStepResult>();

        if (tasks.Count == 0)
        {
            return results;
        }

        if (Options.ParallelSpokes)
        {
            var semaphore = Options.MaxConcurrentSpokes.HasValue
                ? new SemaphoreSlim(Options.MaxConcurrentSpokes.Value)
                : null;

            var spokeTasks = tasks.Select(async task =>
            {
                if (semaphore != null)
                {
                    await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                }

                try
                {
                    var agent = _spokeAgents.FirstOrDefault(a =>
                        a.Name.Equals(task.AgentName, StringComparison.OrdinalIgnoreCase));

                    if (agent == null)
                    {
                        return new AgentStepResult
                        {
                            AgentName = task.AgentName,
                            Input = [],
                            IsSuccess = false,
                            Error = $"Agent '{task.AgentName}' not found"
                        };
                    }

                    var messages = new List<Message>
                    {
                        new UserMessage
                        {
                            Content = [new TextMessageContent { Value = task.Instruction }]
                        }
                    };

                    return await ExecuteAgentAsync(agent, messages, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    semaphore?.Release();
                }
            });

            var completedResults = await Task.WhenAll(spokeTasks).ConfigureAwait(false);
            results.AddRange(completedResults);
        }
        else
        {
            foreach (var task in tasks)
            {
                var agent = _spokeAgents.FirstOrDefault(a =>
                    a.Name.Equals(task.AgentName, StringComparison.OrdinalIgnoreCase));

                if (agent == null)
                {
                    results.Add(new AgentStepResult
                    {
                        AgentName = task.AgentName,
                        Input = [],
                        IsSuccess = false,
                        Error = $"Agent '{task.AgentName}' not found"
                    });
                    continue;
                }

                var messages = new List<Message>
                {
                    new UserMessage
                    {
                        Content = [new TextMessageContent { Value = task.Instruction }]
                    }
                };

                var result = await ExecuteAgentAsync(agent, messages, cancellationToken).ConfigureAwait(false);
                results.Add(result);

                if (!result.IsSuccess && Options.StopOnAgentFailure)
                {
                    break;
                }
            }
        }

        return results;
    }

    private string BuildSpokeAgentInfo()
    {
        var sb = new StringBuilder();
        foreach (var agent in _spokeAgents)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"- {agent.Name}: {agent.Description}");
        }
        return sb.ToString();
    }

    private static string BuildPreviousRoundSummary(List<AgentStepResult> steps, int round)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"=== Previous Round {round} Results ===");

        foreach (var step in steps.TakeLast(steps.Count / round))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"Agent: {step.AgentName}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Success: {step.IsSuccess}");

            if (step.IsSuccess)
            {
                var msg = ExtractMessage(step.Response);
                if (msg != null)
                {
                    foreach (var content in GetMessageContent(msg))
                    {
                        if (content is TextMessageContent textContent)
                        {
                            sb.AppendLine(CultureInfo.InvariantCulture, $"Output: {textContent.Value}");
                        }
                    }
                }
            }
            else
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"Error: {step.Error}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static List<Message> BuildNextRoundMessages(
        List<Message> originalMessages,
        List<AgentStepResult> spokeResults)
    {
        var messages = new List<Message>(originalMessages);

        var sb = new StringBuilder();
        sb.AppendLine("Spoke agent results:");

        foreach (var result in spokeResults)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"[{result.AgentName}]:");
            if (result.IsSuccess)
            {
                var msg = ExtractMessage(result.Response);
                if (msg != null)
                {
                    foreach (var content in GetMessageContent(msg))
                    {
                        if (content is TextMessageContent textContent)
                        {
                            sb.AppendLine(textContent.Value);
                        }
                    }
                }
            }
            else
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"Failed: {result.Error}");
            }
            sb.AppendLine();
        }

        messages.Add(new UserMessage
        {
            Content = [new TextMessageContent { Value = sb.ToString() }]
        });

        return messages;
    }

    private static HubResponseParsed ParseHubResponse(MessageResponse? response)
    {
        if (response?.Message == null)
        {
            return new HubResponseParsed { IsComplete = true };
        }

        var content = GetMessageContent(response.Message)
            .OfType<TextMessageContent>()
            .FirstOrDefault()?.Value;

        if (string.IsNullOrWhiteSpace(content))
        {
            return new HubResponseParsed { IsComplete = true };
        }

        try
        {
            // JSON 블록 추출 시도
            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var parsed = JsonSerializer.Deserialize<HubResponseJson>(json, s_caseInsensitiveJsonOptions);

                if (parsed != null)
                {
                    return new HubResponseParsed
                    {
                        IsComplete = parsed.Complete,
                        Tasks = parsed.Tasks?.Select(t => new SpokeTask
                        {
                            AgentName = t.Agent ?? "",
                            Instruction = t.Instruction ?? ""
                        }).ToList() ?? []
                    };
                }
            }
        }
        catch (JsonException)
        {
            // JSON 파싱 실패 시 완료로 간주
        }

        return new HubResponseParsed { IsComplete = true };
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        IEnumerable<Message> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Hub-Spoke 패턴은 복잡한 상호작용이 필요하므로
        // 스트리밍 버전은 각 단계의 시작/완료 이벤트만 제공
        var result = await ExecuteAsync(messages, cancellationToken).ConfigureAwait(false);

        yield return new OrchestrationStreamEvent { EventType = OrchestrationEventType.Started };

        foreach (var step in result.Steps)
        {
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

    private sealed class HubResponseParsed
    {
        public bool IsComplete { get; init; }
        public List<SpokeTask> Tasks { get; init; } = [];
    }

    private sealed class SpokeTask
    {
        public required string AgentName { get; init; }
        public required string Instruction { get; init; }
    }

    private sealed class HubResponseJson
    {
        public bool Complete { get; set; }
        public List<TaskJson>? Tasks { get; set; }
    }

    private sealed class TaskJson
    {
        public string? Agent { get; set; }
        public string? Instruction { get; set; }
    }
}

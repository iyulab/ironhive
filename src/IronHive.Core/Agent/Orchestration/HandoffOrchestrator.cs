using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 에이전트 간 직접 제어권 위임(handoff)을 지원하는 오케스트레이터입니다.
/// 에이전트 응답에서 JSON 형태의 handoff 지시를 파싱하여 대상 에이전트로 전환합니다.
/// </summary>
public partial class HandoffOrchestrator : OrchestratorBase
{
    private readonly Dictionary<string, IAgent> _agentMap = [];
    private readonly Dictionary<string, List<HandoffTarget>> _handoffMap = [];

    private new HandoffOrchestratorOptions Options => (HandoffOrchestratorOptions)base.Options;

    internal HandoffOrchestrator(
        HandoffOrchestratorOptions options,
        Dictionary<string, IAgent> agentMap,
        Dictionary<string, List<HandoffTarget>> handoffMap)
        : base(options)
    {
        _agentMap = agentMap;
        _handoffMap = handoffMap;

        // OrchestratorBase의 Agents 목록에도 추가
        AddAgents(agentMap.Values);
    }

    /// <inheritdoc />
    public override async Task<OrchestrationResult> ExecuteAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default)
    {
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

            // 완료된 단계에서 핸드오프 체인 추적하여 재개 지점 결정
            var currentAgentName = Options.InitialAgentName;
            var transitionCount = 0;

            foreach (var completedStep in completedSteps)
            {
                var text = GetResponseText(completedStep.Response);
                var h = ParseHandoff(text);
                if (h != null)
                {
                    currentAgentName = h.Value.TargetAgent;
                    transitionCount++;
                }
            }

            while (transitionCount <= Options.MaxTransitions)
            {
                if (!_agentMap.TryGetValue(currentAgentName, out var currentAgent))
                {
                    return OrchestrationResult.Failure(
                        $"Agent '{currentAgentName}' not found.",
                        steps, stopwatch.Elapsed);
                }

                // 승인 체크
                if (!await CheckApprovalAsync(currentAgent, steps.LastOrDefault(), cts.Token).ConfigureAwait(false))
                {
                    await SaveCheckpointAsync(steps, conversationHistory, cts.Token).ConfigureAwait(false);
                    stopwatch.Stop();
                    return OrchestrationResult.Failure(
                        $"Approval denied for agent '{currentAgentName}'",
                        steps,
                        stopwatch.Elapsed);
                }

                // 시스템 프롬프트에 handoff 정보 주입
                var originalPrompt = currentAgent.Instructions;
                InjectHandoffInstructions(currentAgent, currentAgentName);

                try
                {
                    var step = await ExecuteAgentAsync(currentAgent, conversationHistory, cts.Token)
                        .ConfigureAwait(false);
                    steps.Add(step);

                    if (!step.IsSuccess)
                    {
                        if (Options.StopOnAgentFailure)
                        {
                            await SaveCheckpointAsync(steps, conversationHistory, cts.Token).ConfigureAwait(false);
                            return OrchestrationResult.Failure(
                                step.Error ?? $"Agent '{currentAgentName}' failed.",
                                steps, stopwatch.Elapsed);
                        }
                        break;
                    }

                    // 응답을 대화 히스토리에 추가
                    var responseMessage = ExtractMessage(step.Response);
                    if (responseMessage != null)
                    {
                        conversationHistory.Add(responseMessage);
                    }

                    // handoff 감지
                    var responseText = GetResponseText(step.Response);
                    var handoff = ParseHandoff(responseText);

                    if (handoff != null)
                    {
                        // 유효한 handoff 대상인지 확인
                        if (!_agentMap.ContainsKey(handoff.Value.TargetAgent))
                        {
                            return OrchestrationResult.Failure(
                                $"Handoff target '{handoff.Value.TargetAgent}' not found.",
                                steps, stopwatch.Elapsed);
                        }

                        // 컨텍스트가 있으면 대화 히스토리에 추가
                        if (!string.IsNullOrEmpty(handoff.Value.Context))
                        {
                            conversationHistory.Add(new UserMessage
                            {
                                Content = [new TextMessageContent { Value = handoff.Value.Context }]
                            });
                        }

                        currentAgentName = handoff.Value.TargetAgent;
                        transitionCount++;

                        // 전환 후 체크포인트 저장
                        await SaveCheckpointAsync(steps, conversationHistory, cts.Token).ConfigureAwait(false);
                        continue;
                    }

                    // handoff 없음
                    if (Options.NoHandoffHandler != null)
                    {
                        var followUp = await Options.NoHandoffHandler(currentAgentName, step)
                            .ConfigureAwait(false);
                        if (followUp != null)
                        {
                            conversationHistory.Add(followUp);
                            continue;
                        }
                    }

                    // 종료
                    break;
                }
                finally
                {
                    currentAgent.Instructions = originalPrompt;
                }
            }

            stopwatch.Stop();

            var lastStep = steps.LastOrDefault();
            var finalMessage = lastStep?.Response?.Message as Message
                ?? new AssistantMessage
                {
                    Content = [new TextMessageContent { Value = "Orchestration completed." }]
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
                $"Orchestration timed out after {Options.Timeout.TotalSeconds}s.",
                steps, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return OrchestrationResult.Failure(
                $"Orchestration failed: {ex.Message}",
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

            // handoff 이벤트 감지
            if (step.IsSuccess)
            {
                var responseText = GetResponseText(step.Response);
                var handoff = ParseHandoff(responseText);
                if (handoff != null)
                {
                    yield return new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.Handoff,
                        AgentName = handoff.Value.TargetAgent
                    };
                }
            }
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

    private void InjectHandoffInstructions(IAgent agent, string agentName)
    {
        if (!_handoffMap.TryGetValue(agentName, out var targets) || targets.Count == 0)
            return;

        var handoffInfo = string.Join("\n", targets.Select(t =>
            $"- \"{t.AgentName}\": {t.Description ?? "No description"}"));

        var instruction = $$"""

            [HANDOFF INSTRUCTIONS - IMPORTANT]
            When you need another agent's expertise, you MUST hand off by responding with ONLY this JSON format:
            {"handoff_to": "agent_name", "context": "brief context for the next agent"}

            Available agents for handoff:
            {{handoffInfo}}

            RULES:
            1. If you can fully answer the user's question yourself, respond normally WITHOUT JSON.
            2. If the question requires another agent's expertise, respond ONLY with the JSON handoff object.
            3. Do NOT include any other text when handing off - ONLY the JSON object.

            Example handoff response (when math expertise is needed):
            {"handoff_to": "math-expert", "context": "User asked about calculus derivatives"}
            """;

        agent.Instructions = (agent.Instructions ?? "") + instruction;
    }

    private static string GetResponseText(MessageResponse? response)
    {
        if (response?.Message?.Content == null) return string.Empty;
        return string.Join("", response.Message.Content
            .OfType<TextMessageContent>()
            .Select(c => c.Value));
    }

    private static HandoffInfo? ParseHandoff(string responseText)
    {
        if (string.IsNullOrWhiteSpace(responseText))
            return null;

        // 코드 블록 내용 추출 (```json ... ``` 형태)
        var codeBlockMatch = CodeBlockRegex().Match(responseText);
        var textToSearch = codeBlockMatch.Success
            ? codeBlockMatch.Groups[1].Value
            : responseText;

        // 여러 패턴 시도: handoff_to, transfer_to, delegate_to
        var match = HandoffJsonRegex().Match(textToSearch);
        if (!match.Success)
            match = TransferJsonRegex().Match(textToSearch);
        if (!match.Success)
            match = DelegateJsonRegex().Match(textToSearch);
        if (!match.Success)
            return null;

        try
        {
            using var doc = JsonDocument.Parse(match.Value);
            var root = doc.RootElement;

            // 여러 키 이름 지원
            string? target = null;
            foreach (var key in new[] { "handoff_to", "transfer_to", "delegate_to", "agent" })
            {
                if (root.TryGetProperty(key, out var prop))
                {
                    target = prop.GetString();
                    if (!string.IsNullOrEmpty(target))
                        break;
                }
            }

            if (string.IsNullOrEmpty(target))
                return null;

            // context 또는 message 키 지원
            string? context = null;
            foreach (var key in new[] { "context", "message", "reason" })
            {
                if (root.TryGetProperty(key, out var prop))
                {
                    context = prop.GetString();
                    if (!string.IsNullOrEmpty(context))
                        break;
                }
            }

            return new HandoffInfo(target, context);
        }
        catch (JsonException)
        {
            // JSON 파싱 실패 → handoff 아님
        }

        return null;
    }

    // 코드 블록: ```json ... ``` 또는 ``` ... ```
    [GeneratedRegex(@"```(?:json)?\s*(\{[^`]*\})\s*```", RegexOptions.Singleline)]
    private static partial Regex CodeBlockRegex();

    [GeneratedRegex("""\{[^{}]*"handoff_to"\s*:\s*"[^"]*"[^{}]*\}""", RegexOptions.Singleline)]
    private static partial Regex HandoffJsonRegex();

    [GeneratedRegex("""\{[^{}]*"transfer_to"\s*:\s*"[^"]*"[^{}]*\}""", RegexOptions.Singleline)]
    private static partial Regex TransferJsonRegex();

    [GeneratedRegex("""\{[^{}]*"delegate_to"\s*:\s*"[^"]*"[^{}]*\}""", RegexOptions.Singleline)]
    private static partial Regex DelegateJsonRegex();

    private readonly record struct HandoffInfo(string TargetAgent, string? Context);
}

using System.Globalization;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 라운드 로빈 방식으로 발언자를 순환 선택합니다.
/// </summary>
public class RoundRobinSpeakerSelector : ISpeakerSelector
{
    private int _currentIndex = -1;

    public Task<string?> SelectNextSpeakerAsync(
        IReadOnlyList<AgentStepResult> steps,
        IReadOnlyList<Message> conversationHistory,
        IReadOnlyList<IAgent> agents,
        CancellationToken cancellationToken = default)
    {
        if (agents.Count == 0)
            return Task.FromResult<string?>(null);

        _currentIndex = (_currentIndex + 1) % agents.Count;
        return Task.FromResult<string?>(agents[_currentIndex].Name);
    }
}

/// <summary>
/// 랜덤으로 발언자를 선택합니다.
/// </summary>
public class RandomSpeakerSelector : ISpeakerSelector
{
    private readonly Random _random = new();

    public Task<string?> SelectNextSpeakerAsync(
        IReadOnlyList<AgentStepResult> steps,
        IReadOnlyList<Message> conversationHistory,
        IReadOnlyList<IAgent> agents,
        CancellationToken cancellationToken = default)
    {
        if (agents.Count == 0)
            return Task.FromResult<string?>(null);

        var index = _random.Next(agents.Count);
        return Task.FromResult<string?>(agents[index].Name);
    }
}

/// <summary>
/// LLM 기반 관리자 에이전트가 대화 기록을 보고 다음 발언자를 선택합니다.
/// </summary>
public class LlmSpeakerSelector : ISpeakerSelector
{
    private readonly IAgent _manager;

    public LlmSpeakerSelector(IAgent manager)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    public async Task<string?> SelectNextSpeakerAsync(
        IReadOnlyList<AgentStepResult> steps,
        IReadOnlyList<Message> conversationHistory,
        IReadOnlyList<IAgent> agents,
        CancellationToken cancellationToken = default)
    {
        if (agents.Count == 0)
            return null;

        // 에이전트 목록 생성 (설명 포함)
        var agentDescriptions = string.Join("\n", agents.Select(a =>
            $"- \"{a.Name}\": {a.Description}"));

        // 대화 기록 구성 (초기 메시지 + 에이전트 응답)
        var historyBuilder = new System.Text.StringBuilder();

        // 초기 user 메시지 포함
        foreach (var msg in conversationHistory)
        {
            var (role, content) = msg switch
            {
                UserMessage um => ("User", um.Content),
                AssistantMessage am => (am.Name ?? "Assistant", am.Content),
                _ => ("System", Enumerable.Empty<MessageContent>())
            };
            var text = string.Join("", content.OfType<TextMessageContent>().Select(c => c.Value));
            if (!string.IsNullOrWhiteSpace(text))
            {
                historyBuilder.AppendLine(CultureInfo.InvariantCulture, $"[{role}]: {text}");
            }
        }

        var history = historyBuilder.ToString().Trim();
        var agentNames = string.Join(", ", agents.Select(a => $"\"{a.Name}\""));

        var prompt = $"""
            You are a conversation manager. Based on the conversation so far, select the most appropriate agent to speak next.

            Available agents:
            {agentDescriptions}

            Conversation history:
            {history}

            Which agent should respond next? Consider the topic and each agent's expertise.
            Respond with ONLY the agent name (one of: {agentNames}).
            Do not include any other text, explanation, or punctuation.
            """;

        var messages = new[]
        {
            new UserMessage
            {
                Content = [new TextMessageContent { Value = prompt }]
            }
        };

        var response = await _manager.InvokeAsync(messages, cancellationToken).ConfigureAwait(false);
        var selectedName = response.Message.Content
            .OfType<TextMessageContent>()
            .FirstOrDefault()?.Value?.Trim().Trim('"', '\'', ' ');

        if (string.IsNullOrEmpty(selectedName) || selectedName.Equals("NONE", StringComparison.OrdinalIgnoreCase))
            return null;

        // 에이전트 이름 매칭 (부분 일치도 허용)
        var matched = agents.FirstOrDefault(a =>
            a.Name.Equals(selectedName, StringComparison.OrdinalIgnoreCase));

        // 정확한 매칭 실패 시 포함 여부로 시도
        matched ??= agents.FirstOrDefault(a =>
            selectedName.Contains(a.Name, StringComparison.OrdinalIgnoreCase) ||
            a.Name.Contains(selectedName, StringComparison.OrdinalIgnoreCase));

        return matched?.Name;
    }
}

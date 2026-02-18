using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages.Content;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 최대 라운드 수 초과 시 종료
/// </summary>
public class MaxRoundsTermination : ITerminationCondition
{
    private readonly int _maxRounds;

    public MaxRoundsTermination(int maxRounds)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRounds);
        _maxRounds = maxRounds;
    }

    public Task<bool> ShouldTerminateAsync(
        IReadOnlyList<AgentStepResult> steps,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(steps.Count >= _maxRounds);
    }
}

/// <summary>
/// 응답에 특정 키워드가 포함되면 종료
/// </summary>
public class KeywordTermination : ITerminationCondition
{
    private readonly string _keyword;
    private readonly StringComparison _comparison;

    public KeywordTermination(string keyword, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        _keyword = keyword ?? throw new ArgumentNullException(nameof(keyword));
        _comparison = comparison;
    }

    public Task<bool> ShouldTerminateAsync(
        IReadOnlyList<AgentStepResult> steps,
        CancellationToken cancellationToken = default)
    {
        if (steps.Count == 0)
            return Task.FromResult(false);

        var lastStep = steps[^1];
        var text = lastStep.Response?.Message?.Content
            .OfType<TextMessageContent>()
            .FirstOrDefault()?.Value;

        if (text == null)
            return Task.FromResult(false);

        return Task.FromResult(text.Contains(_keyword, _comparison));
    }
}

/// <summary>
/// 누적 토큰 수 초과 시 종료
/// </summary>
public class TokenBudgetTermination : ITerminationCondition
{
    private readonly int _maxTokens;

    public TokenBudgetTermination(int maxTokens)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTokens);
        _maxTokens = maxTokens;
    }

    public Task<bool> ShouldTerminateAsync(
        IReadOnlyList<AgentStepResult> steps,
        CancellationToken cancellationToken = default)
    {
        var totalTokens = 0;
        foreach (var step in steps)
        {
            if (step.Response?.TokenUsage != null)
            {
                totalTokens += step.Response.TokenUsage.InputTokens
                    + step.Response.TokenUsage.OutputTokens;
            }
        }

        return Task.FromResult(totalTokens >= _maxTokens);
    }
}

/// <summary>
/// 여러 종료 조건을 조합합니다.
/// </summary>
public class CompositeTermination : ITerminationCondition
{
    private readonly IReadOnlyList<ITerminationCondition> _conditions;
    private readonly bool _requireAll;

    /// <summary>
    /// 복합 종료 조건을 생성합니다.
    /// </summary>
    /// <param name="requireAll">true이면 모든 조건 충족 시 종료, false이면 하나라도 충족 시 종료</param>
    /// <param name="conditions">종료 조건들</param>
    public CompositeTermination(bool requireAll, params ITerminationCondition[] conditions)
    {
        if (conditions.Length == 0)
            throw new ArgumentException("At least one condition is required.", nameof(conditions));
        _conditions = conditions;
        _requireAll = requireAll;
    }

    public async Task<bool> ShouldTerminateAsync(
        IReadOnlyList<AgentStepResult> steps,
        CancellationToken cancellationToken = default)
    {
        foreach (var condition in _conditions)
        {
            var result = await condition.ShouldTerminateAsync(steps, cancellationToken)
                .ConfigureAwait(false);

            if (_requireAll && !result)
                return false; // all 모드에서 하나라도 false면 종료하지 않음

            if (!_requireAll && result)
                return true; // any 모드에서 하나라도 true면 종료
        }

        return _requireAll; // all 모드: 모두 true → true, any 모드: 모두 false → false
    }
}

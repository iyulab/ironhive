using FluentAssertions;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Core.Agent.Orchestration;

namespace IronHive.Tests.Agent.Orchestration;

public class TerminationConditionTests
{
    #region MaxRoundsTermination

    [Fact]
    public async Task MaxRounds_BelowMax_DoesNotTerminate()
    {
        var condition = new MaxRoundsTermination(5);
        var steps = MakeSteps(3);

        var result = await condition.ShouldTerminateAsync(steps);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task MaxRounds_AtMax_Terminates()
    {
        var condition = new MaxRoundsTermination(3);
        var steps = MakeSteps(3);

        var result = await condition.ShouldTerminateAsync(steps);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task MaxRounds_AboveMax_Terminates()
    {
        var condition = new MaxRoundsTermination(3);
        var steps = MakeSteps(5);

        var result = await condition.ShouldTerminateAsync(steps);

        result.Should().BeTrue();
    }

    [Fact]
    public void MaxRounds_ZeroOrNegative_Throws()
    {
        var act0 = () => new MaxRoundsTermination(0);
        var actNeg = () => new MaxRoundsTermination(-1);

        act0.Should().Throw<ArgumentOutOfRangeException>();
        actNeg.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task MaxRounds_EmptySteps_DoesNotTerminate()
    {
        var condition = new MaxRoundsTermination(5);

        var result = await condition.ShouldTerminateAsync(Array.Empty<AgentStepResult>());

        result.Should().BeFalse();
    }

    #endregion

    #region KeywordTermination

    [Fact]
    public async Task Keyword_Found_Terminates()
    {
        var condition = new KeywordTermination("DONE");
        var steps = MakeStepsWithText("still working", "DONE");

        var result = await condition.ShouldTerminateAsync(steps);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Keyword_NotFound_DoesNotTerminate()
    {
        var condition = new KeywordTermination("DONE");
        var steps = MakeStepsWithText("still working", "not finished");

        var result = await condition.ShouldTerminateAsync(steps);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Keyword_EmptySteps_DoesNotTerminate()
    {
        var condition = new KeywordTermination("DONE");

        var result = await condition.ShouldTerminateAsync(Array.Empty<AgentStepResult>());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Keyword_CaseInsensitive_Terminates()
    {
        var condition = new KeywordTermination("done", StringComparison.OrdinalIgnoreCase);
        var steps = MakeStepsWithText("DONE");

        var result = await condition.ShouldTerminateAsync(steps);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Keyword_CaseSensitive_DoesNotTerminate()
    {
        var condition = new KeywordTermination("DONE", StringComparison.Ordinal);
        var steps = MakeStepsWithText("done");

        var result = await condition.ShouldTerminateAsync(steps);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Keyword_ChecksLastStepOnly()
    {
        var condition = new KeywordTermination("DONE");
        var steps = MakeStepsWithText("DONE", "still working");

        var result = await condition.ShouldTerminateAsync(steps);

        result.Should().BeFalse(); // last step doesn't contain keyword
    }

    [Fact]
    public void Keyword_NullKeyword_Throws()
    {
        var act = () => new KeywordTermination(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region TokenBudgetTermination

    [Fact]
    public async Task TokenBudget_BelowBudget_DoesNotTerminate()
    {
        var condition = new TokenBudgetTermination(100);
        var steps = MakeStepsWithTokens((10, 5), (10, 5)); // total = 30

        var result = await condition.ShouldTerminateAsync(steps);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task TokenBudget_AtBudget_Terminates()
    {
        var condition = new TokenBudgetTermination(30);
        var steps = MakeStepsWithTokens((10, 5), (10, 5)); // total = 30

        var result = await condition.ShouldTerminateAsync(steps);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task TokenBudget_AboveBudget_Terminates()
    {
        var condition = new TokenBudgetTermination(20);
        var steps = MakeStepsWithTokens((10, 5), (10, 5)); // total = 30

        var result = await condition.ShouldTerminateAsync(steps);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task TokenBudget_EmptySteps_DoesNotTerminate()
    {
        var condition = new TokenBudgetTermination(100);

        var result = await condition.ShouldTerminateAsync(Array.Empty<AgentStepResult>());

        result.Should().BeFalse();
    }

    [Fact]
    public void TokenBudget_ZeroOrNegative_Throws()
    {
        var act0 = () => new TokenBudgetTermination(0);
        var actNeg = () => new TokenBudgetTermination(-1);

        act0.Should().Throw<ArgumentOutOfRangeException>();
        actNeg.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task TokenBudget_NullTokenUsage_Ignored()
    {
        var condition = new TokenBudgetTermination(100);
        var steps = new List<AgentStepResult>
        {
            new() { AgentName = "a", IsSuccess = true, Response = null }
        };

        var result = await condition.ShouldTerminateAsync(steps);

        result.Should().BeFalse();
    }

    #endregion

    #region CompositeTermination

    [Fact]
    public async Task Composite_RequireAll_AllTrue_Terminates()
    {
        var composite = new CompositeTermination(
            requireAll: true,
            new MaxRoundsTermination(2),
            new KeywordTermination("DONE"));

        var steps = MakeStepsWithText("working", "DONE"); // 2 steps, last has DONE

        var result = await composite.ShouldTerminateAsync(steps);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Composite_RequireAll_SomeFalse_DoesNotTerminate()
    {
        var composite = new CompositeTermination(
            requireAll: true,
            new MaxRoundsTermination(5), // 2 < 5 → false
            new KeywordTermination("DONE")); // "DONE" found → true

        var steps = MakeStepsWithText("working", "DONE");

        var result = await composite.ShouldTerminateAsync(steps);

        result.Should().BeFalse(); // not all conditions met
    }

    [Fact]
    public async Task Composite_RequireAny_OneTrue_Terminates()
    {
        var composite = new CompositeTermination(
            requireAll: false,
            new MaxRoundsTermination(5), // 2 < 5 → false
            new KeywordTermination("DONE")); // "DONE" found → true

        var steps = MakeStepsWithText("working", "DONE");

        var result = await composite.ShouldTerminateAsync(steps);

        result.Should().BeTrue(); // one condition met
    }

    [Fact]
    public async Task Composite_RequireAny_AllFalse_DoesNotTerminate()
    {
        var composite = new CompositeTermination(
            requireAll: false,
            new MaxRoundsTermination(5), // 2 < 5 → false
            new KeywordTermination("DONE")); // "working" → false

        var steps = MakeStepsWithText("working", "still working");

        var result = await composite.ShouldTerminateAsync(steps);

        result.Should().BeFalse();
    }

    [Fact]
    public void Composite_NoConditions_Throws()
    {
        var act = () => new CompositeTermination(true);
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Helpers

    private static AgentStepResult[] MakeSteps(int count)
    {
        return Enumerable.Range(0, count).Select(i => new AgentStepResult
        {
            AgentName = $"agent-{i}",
            IsSuccess = true,
            Response = new MessageResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Content = [new TextMessageContent { Value = $"response-{i}" }]
                },
                TokenUsage = new MessageTokenUsage { InputTokens = 10, OutputTokens = 5 }
            }
        }).ToArray();
    }

    private static AgentStepResult[] MakeStepsWithText(params string[] texts)
    {
        return texts.Select((text, i) => new AgentStepResult
        {
            AgentName = $"agent-{i}",
            IsSuccess = true,
            Response = new MessageResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Content = [new TextMessageContent { Value = text }]
                },
                TokenUsage = new MessageTokenUsage { InputTokens = 10, OutputTokens = text.Length }
            }
        }).ToArray();
    }

    private static AgentStepResult[] MakeStepsWithTokens(params (int input, int output)[] tokens)
    {
        return tokens.Select((t, i) => new AgentStepResult
        {
            AgentName = $"agent-{i}",
            IsSuccess = true,
            Response = new MessageResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Content = [new TextMessageContent { Value = "text" }]
                },
                TokenUsage = new MessageTokenUsage { InputTokens = t.input, OutputTokens = t.output }
            }
        }).ToArray();
    }

    #endregion
}

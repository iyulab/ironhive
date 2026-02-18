using FluentAssertions;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Tests.Agent.Orchestration;

public class OrchestrationResultTests
{
    [Fact]
    public void Success_ShouldSetCorrectProperties()
    {
        var message = new AssistantMessage { Content = [] };
        var steps = new List<AgentStepResult>
        {
            new() { AgentName = "agent-1", IsSuccess = true }
        };
        var duration = TimeSpan.FromSeconds(5);
        var tokenUsage = new TokenUsageSummary
        {
            TotalInputTokens = 100,
            TotalOutputTokens = 50
        };

        var result = OrchestrationResult.Success(message, steps, duration, tokenUsage);

        result.IsSuccess.Should().BeTrue();
        result.FinalOutput.Should().Be(message);
        result.Steps.Should().HaveCount(1);
        result.TotalDuration.Should().Be(duration);
        result.TokenUsage.Should().Be(tokenUsage);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Success_WithoutTokenUsage_ShouldHaveNullTokenUsage()
    {
        var message = new AssistantMessage { Content = [] };
        var steps = Array.Empty<AgentStepResult>();

        var result = OrchestrationResult.Success(message, steps, TimeSpan.Zero);

        result.IsSuccess.Should().BeTrue();
        result.TokenUsage.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldSetCorrectProperties()
    {
        var result = OrchestrationResult.Failure("Something went wrong");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Something went wrong");
        result.Steps.Should().BeEmpty();
        result.TotalDuration.Should().Be(TimeSpan.Zero);
        result.FinalOutput.Should().BeNull();
    }

    [Fact]
    public void Failure_WithStepsAndDuration_ShouldSetAll()
    {
        var steps = new List<AgentStepResult>
        {
            new() { AgentName = "agent-1", IsSuccess = true },
            new() { AgentName = "agent-2", IsSuccess = false, Error = "Failed" }
        };
        var duration = TimeSpan.FromSeconds(10);

        var result = OrchestrationResult.Failure("Error occurred", steps, duration);

        result.IsSuccess.Should().BeFalse();
        result.Steps.Should().HaveCount(2);
        result.TotalDuration.Should().Be(duration);
    }

    [Fact]
    public void DefaultSteps_ShouldBeEmpty()
    {
        var result = new OrchestrationResult();

        result.Steps.Should().BeEmpty();
    }
}

public class AgentStepResultTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var step = new AgentStepResult { AgentName = "test-agent" };

        step.AgentName.Should().Be("test-agent");
        step.Input.Should().BeEmpty();
        step.Response.Should().BeNull();
        step.Duration.Should().Be(TimeSpan.Zero);
        step.IsSuccess.Should().BeFalse();
        step.Error.Should().BeNull();
    }

    [Fact]
    public void ShouldInitialize_WithAllFields()
    {
        var step = new AgentStepResult
        {
            AgentName = "agent-1",
            IsSuccess = true,
            Duration = TimeSpan.FromMilliseconds(500),
            Error = null
        };

        step.IsSuccess.Should().BeTrue();
        step.Duration.TotalMilliseconds.Should().Be(500);
    }
}

public class TokenUsageSummaryTests
{
    [Fact]
    public void TotalTokens_ShouldSumInputAndOutput()
    {
        var summary = new TokenUsageSummary
        {
            TotalInputTokens = 1000,
            TotalOutputTokens = 500
        };

        summary.TotalTokens.Should().Be(1500);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(100, 0, 100)]
    [InlineData(0, 200, 200)]
    [InlineData(5000, 3000, 8000)]
    public void TotalTokens_ShouldComputeCorrectly(int input, int output, int expected)
    {
        var summary = new TokenUsageSummary
        {
            TotalInputTokens = input,
            TotalOutputTokens = output
        };

        summary.TotalTokens.Should().Be(expected);
    }

    [Fact]
    public void DefaultValues_ShouldBeZero()
    {
        var summary = new TokenUsageSummary();

        summary.TotalInputTokens.Should().Be(0);
        summary.TotalOutputTokens.Should().Be(0);
        summary.TotalTokens.Should().Be(0);
    }
}

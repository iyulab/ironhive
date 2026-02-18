using FluentAssertions;
using IronHive.Abstractions.Workflow;

namespace IronHive.Tests.Workflow;

public class TaskStepResultTests
{
    [Fact]
    public void Success_ShouldSetIsErrorFalse()
    {
        var result = TaskStepResult.Success();

        result.IsError.Should().BeFalse();
        result.Message.Should().BeNull();
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void Success_WithMessage_ShouldSetMessage()
    {
        var result = TaskStepResult.Success("completed");

        result.IsError.Should().BeFalse();
        result.Message.Should().Be("completed");
    }

    [Fact]
    public void Fail_ShouldSetIsErrorTrue()
    {
        var ex = new InvalidOperationException("something broke");

        var result = TaskStepResult.Fail(ex);

        result.IsError.Should().BeTrue();
        result.Exception.Should().BeSameAs(ex);
        result.Message.Should().Be("something broke");
    }

    [Fact]
    public void Fail_MessageShouldMatchExceptionMessage()
    {
        var ex = new ArgumentException("bad argument");

        var result = TaskStepResult.Fail(ex);

        result.Message.Should().Be(ex.Message);
    }

    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var result = new TaskStepResult();

        result.IsError.Should().BeFalse();
        result.Message.Should().BeNull();
        result.Exception.Should().BeNull();
    }
}

public class ConditionStepResultTests
{
    [Fact]
    public void Select_ShouldSetKey()
    {
        var result = ConditionStepResult.Select("branch-a");

        result.Key.Should().Be("branch-a");
        result.Message.Should().BeNull();
    }

    [Fact]
    public void Select_WithMessage_ShouldSetBoth()
    {
        var result = ConditionStepResult.Select("yes", "condition met");

        result.Key.Should().Be("yes");
        result.Message.Should().Be("condition met");
    }
}

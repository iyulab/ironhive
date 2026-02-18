using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Core.Tools;
using NSubstitute;
using IronHive.Abstractions.Tools;

namespace IronHive.Tests.Messages;

public class ToolLimitValidationResultTests
{
    [Fact]
    public void ExceededBy_WhenValid_ShouldReturnZero()
    {
        var result = new ToolLimitValidationResult
        {
            IsValid = true,
            ActualCount = 5,
            MaxAllowed = 10
        };

        result.ExceededBy.Should().Be(0);
    }

    [Fact]
    public void ExceededBy_WhenInvalid_ShouldReturnDifference()
    {
        var result = new ToolLimitValidationResult
        {
            IsValid = false,
            ActualCount = 15,
            MaxAllowed = 10
        };

        result.ExceededBy.Should().Be(5);
    }

    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var result = new ToolLimitValidationResult();

        result.IsValid.Should().BeFalse();
        result.ActualCount.Should().Be(0);
        result.MaxAllowed.Should().Be(0);
        result.Message.Should().BeNull();
    }
}

public class ToolLimitExceededExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var ex = new ToolLimitExceededException(25, 20);

        ex.ActualCount.Should().Be(25);
        ex.MaxAllowed.Should().Be(20);
        ex.Message.Should().Contain("25");
        ex.Message.Should().Contain("20");
    }

    [Fact]
    public void ShouldBeInvalidOperationException()
    {
        var ex = new ToolLimitExceededException(10, 5);

        ex.Should().BeAssignableTo<InvalidOperationException>();
    }
}

public class ToolLimitValidationExtensionsTests
{
    private static MessageGenerationRequest CreateRequest(
        int toolCount = 0,
        int maxTools = 20,
        ToolLimitBehavior behavior = ToolLimitBehavior.Throw)
    {
        var tools = new ToolCollection();
        for (var i = 0; i < toolCount; i++)
        {
            var tool = Substitute.For<ITool>();
            tool.UniqueName.Returns($"tool-{i}");
            tools.Add(tool);
        }

        return new MessageGenerationRequest
        {
            Model = "test-model",
            Tools = tools,
            MaxTools = maxTools,
            ToolLimitBehavior = behavior
        };
    }

    [Fact]
    public void ValidateToolLimit_WithMaxToolsZero_ShouldReturnValid()
    {
        var request = CreateRequest(toolCount: 50, maxTools: 0);

        var result = request.ValidateToolLimit();

        result.IsValid.Should().BeTrue();
        result.ActualCount.Should().Be(50);
        result.MaxAllowed.Should().Be(int.MaxValue);
        result.Message.Should().BeNull();
    }

    [Fact]
    public void ValidateToolLimit_WithNegativeMaxTools_ShouldReturnValid()
    {
        var request = CreateRequest(toolCount: 10, maxTools: -1);

        var result = request.ValidateToolLimit();

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateToolLimit_WithinLimit_ShouldReturnValid()
    {
        var request = CreateRequest(toolCount: 5, maxTools: 10);

        var result = request.ValidateToolLimit();

        result.IsValid.Should().BeTrue();
        result.ActualCount.Should().Be(5);
        result.MaxAllowed.Should().Be(10);
        result.Message.Should().BeNull();
    }

    [Fact]
    public void ValidateToolLimit_AtExactLimit_ShouldReturnValid()
    {
        var request = CreateRequest(toolCount: 10, maxTools: 10);

        var result = request.ValidateToolLimit();

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateToolLimit_ExceedingLimit_ShouldReturnInvalid()
    {
        var request = CreateRequest(toolCount: 15, maxTools: 10);

        var result = request.ValidateToolLimit();

        result.IsValid.Should().BeFalse();
        result.ActualCount.Should().Be(15);
        result.MaxAllowed.Should().Be(10);
        result.Message.Should().Contain("15");
        result.Message.Should().Contain("10");
    }

    [Fact]
    public void ValidateToolLimit_WithNullTools_ShouldReturnValid()
    {
        var request = new MessageGenerationRequest
        {
            Model = "test-model",
            Tools = null,
            MaxTools = 10
        };

        var result = request.ValidateToolLimit();

        result.IsValid.Should().BeTrue();
        result.ActualCount.Should().Be(0);
    }

    [Fact]
    public void ValidateToolLimit_NullRequest_ShouldThrow()
    {
        MessageGenerationRequest request = null!;

        var act = () => request.ValidateToolLimit();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ApplyToolLimitPolicy_WhenValid_ShouldReturnTrue()
    {
        var request = CreateRequest(toolCount: 5, maxTools: 10, behavior: ToolLimitBehavior.Throw);

        var result = request.ApplyToolLimitPolicy();

        result.Should().BeTrue();
    }

    [Fact]
    public void ApplyToolLimitPolicy_Ignore_ShouldReturnTrue()
    {
        var request = CreateRequest(toolCount: 15, maxTools: 10, behavior: ToolLimitBehavior.Ignore);

        var result = request.ApplyToolLimitPolicy();

        result.Should().BeTrue();
    }

    [Fact]
    public void ApplyToolLimitPolicy_Warn_ShouldCallOnWarningAndReturnTrue()
    {
        var request = CreateRequest(toolCount: 15, maxTools: 10, behavior: ToolLimitBehavior.Warn);
        string? capturedWarning = null;

        var result = request.ApplyToolLimitPolicy(msg => capturedWarning = msg);

        result.Should().BeTrue();
        capturedWarning.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ApplyToolLimitPolicy_Throw_ShouldThrowException()
    {
        var request = CreateRequest(toolCount: 15, maxTools: 10, behavior: ToolLimitBehavior.Throw);

        var act = () => request.ApplyToolLimitPolicy();

        act.Should().Throw<ToolLimitExceededException>()
            .Where(e => e.ActualCount == 15 && e.MaxAllowed == 10);
    }

    [Fact]
    public void ApplyToolLimitPolicy_Truncate_ShouldReturnTrueAndWarn()
    {
        var request = CreateRequest(toolCount: 15, maxTools: 10, behavior: ToolLimitBehavior.Truncate);
        string? capturedWarning = null;

        var result = request.ApplyToolLimitPolicy(msg => capturedWarning = msg);

        result.Should().BeTrue();
        capturedWarning.Should().Contain("truncate");
    }
}

public class ToolLimitBehaviorEnumTests
{
    [Fact]
    public void ShouldHaveFourValues()
    {
        Enum.GetValues<ToolLimitBehavior>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(ToolLimitBehavior.Ignore, 0)]
    [InlineData(ToolLimitBehavior.Warn, 1)]
    [InlineData(ToolLimitBehavior.Throw, 2)]
    [InlineData(ToolLimitBehavior.Truncate, 3)]
    public void ShouldHaveExpectedIntValues(ToolLimitBehavior behavior, int expected)
    {
        ((int)behavior).Should().Be(expected);
    }
}

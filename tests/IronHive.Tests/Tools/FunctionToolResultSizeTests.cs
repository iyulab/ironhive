using FluentAssertions;
using IronHive.Core.Tools;
using IronHive.Abstractions.Tools;

namespace IronHive.Tests.Tools;

/// <summary>
/// Tests to verify FunctionTool result size limit configuration.
/// Issue #9: Tool result size limit is hardcoded at 30,000 characters.
/// </summary>
public class FunctionToolResultSizeTests
{
    [Fact]
    public void MaxResultSize_ShouldDefaultTo30000()
    {
        // Arrange
        var tool = new FunctionTool(new Func<string>(() => "test"))
        {
            Name = "test_tool",
            Description = "Test tool",
            Parameters = null,
            RequiresApproval = false
        };

        // Assert
        tool.MaxOutput.Should().Be(30_000);
    }

    [Fact]
    public void MaxResultSize_ShouldBeConfigurable()
    {
        // Arrange & Act
        var tool = new FunctionTool(new Func<string>(() => "test"))
        {
            Name = "test_tool",
            Description = "Test tool",
            Parameters = null,
            RequiresApproval = false,
            MaxOutput = 100_000
        };

        // Assert
        tool.MaxOutput.Should().Be(100_000);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSucceed_WhenResultUnderLimit()
    {
        // Arrange
        var tool = new FunctionTool(new Func<string>(() => "short result"))
        {
            Name = "test_tool",
            Description = "Test tool",
            Parameters = null,
            RequiresApproval = false,
            MaxOutput = 100
        };

        // Act
        var result = await tool.InvokeAsync(new ToolInput());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Contain("short result");
    }

    [Fact]
    public async Task InvokeAsync_ShouldFail_WhenResultExceedsLimit()
    {
        // Arrange
        var largeResult = new string('x', 100);
        var tool = new FunctionTool(new Func<string>(() => largeResult))
        {
            Name = "test_tool",
            Description = "Test tool",
            Parameters = null,
            RequiresApproval = false,
            MaxOutput = 50 // Very small limit
        };

        // Act
        var result = await tool.InvokeAsync(new ToolInput());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().Contain("too large");
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotEnforceLimit_WhenMaxResultSizeIsZero()
    {
        // Arrange
        var largeResult = new string('x', 100_000);
        var tool = new FunctionTool(new Func<string>(() => largeResult))
        {
            Name = "test_tool",
            Description = "Test tool",
            Parameters = null,
            RequiresApproval = false,
            MaxOutput = 0 // Unlimited
        };

        // Act
        var result = await tool.InvokeAsync(new ToolInput());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Timeout_ShouldBeConfigurableIndependently()
    {
        // Arrange & Act
        var tool = new FunctionTool(new Func<string>(() => "test"))
        {
            Name = "test_tool",
            Description = "Test tool",
            Parameters = null,
            RequiresApproval = false,
            Timeout = 120,
            MaxOutput = 50_000
        };

        // Assert
        tool.Timeout.Should().Be(120);
        tool.MaxOutput.Should().Be(50_000);
    }
}

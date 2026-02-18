using FluentAssertions;
using IronHive.Abstractions.Tools;

namespace IronHive.Tests.Tools;

public class ToolOutputTests
{
    [Fact]
    public void Success_ShouldSetIsSuccessTrue()
    {
        var output = ToolOutput.Success("result data");

        output.IsSuccess.Should().BeTrue();
        output.Result.Should().Be("result data");
    }

    [Fact]
    public void Success_WithNull_ShouldSetNullResult()
    {
        var output = ToolOutput.Success(null);

        output.IsSuccess.Should().BeTrue();
        output.Result.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldSetIsSuccessFalse()
    {
        var output = ToolOutput.Failure("error message");

        output.IsSuccess.Should().BeFalse();
        output.Result.Should().Be("error message");
    }

    [Fact]
    public void Failure_WithNull_ShouldSetNullResult()
    {
        var output = ToolOutput.Failure(null);

        output.IsSuccess.Should().BeFalse();
        output.Result.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithParameters_ShouldSetProperties()
    {
        var output = new ToolOutput(true, "test");

        output.IsSuccess.Should().BeTrue();
        output.Result.Should().Be("test");
    }

    [Fact]
    public void DefaultConstructor_ShouldHaveDefaultValues()
    {
        var output = new ToolOutput();

        output.IsSuccess.Should().BeFalse();
        output.Result.Should().BeNull();
    }
}

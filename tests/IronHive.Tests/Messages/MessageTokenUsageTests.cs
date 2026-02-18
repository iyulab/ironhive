using FluentAssertions;
using IronHive.Abstractions.Messages;

namespace IronHive.Tests.Messages;

public class MessageTokenUsageTests
{
    [Fact]
    public void TotalTokens_ShouldSumInputAndOutput()
    {
        var usage = new MessageTokenUsage
        {
            InputTokens = 100,
            OutputTokens = 200
        };

        usage.TotalTokens.Should().Be(300);
    }

    [Fact]
    public void TotalTokens_WithZeros_ShouldReturnZero()
    {
        var usage = new MessageTokenUsage();

        usage.TotalTokens.Should().Be(0);
    }

    [Fact]
    public void DefaultValues_ShouldBeZero()
    {
        var usage = new MessageTokenUsage();

        usage.InputTokens.Should().Be(0);
        usage.OutputTokens.Should().Be(0);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(500, 0, 500)]
    [InlineData(0, 300, 300)]
    [InlineData(1000, 2000, 3000)]
    public void TotalTokens_ShouldComputeCorrectly(int input, int output, int expected)
    {
        var usage = new MessageTokenUsage
        {
            InputTokens = input,
            OutputTokens = output
        };

        usage.TotalTokens.Should().Be(expected);
    }
}

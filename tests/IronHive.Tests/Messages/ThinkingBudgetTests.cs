using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using Xunit;

namespace IronHive.Tests.Messages;

/// <summary>
/// Reasoning and answer share MaxTokens. A budget the cap cannot hold produced an empty answer every time (Medium 1,024
/// with MaxTokens 256: 6 of 6 answers empty on a llama.cpp server), so every provider fits the budget with one rule.
/// </summary>
public class ThinkingBudgetTests
{
    [Theory]
    [InlineData(1_024, null, 1_024)]   // no cap: the effort's budget
    [InlineData(1_024, 256, 0)]        // the measured case: no room to think, so no thinking
    [InlineData(1_024, 300, 44)]       // 256 kept for the answer
    [InlineData(1_024, 1_100, 825)]    // a quarter kept for the answer (275)
    [InlineData(1_024, 2_000, 1_024)]  // room for both: unchanged
    [InlineData(4_000, 4_100, 3_075)]  // just above the budget used to leave 100 tokens to answer in
    [InlineData(0, 2_000, 0)]          // effort None stays off
    public void FitWithin_LeavesTheAnswerItsShareOfTheCap(int budget, int? maxTokens, int expected) =>
        ThinkingBudget.FitWithin(budget, maxTokens).Should().Be(expected);

    [Fact]
    public void FitWithin_NeverExceedsTheCapMinusTheAnswersShare()
    {
        foreach (var max in new[] { 1, 255, 256, 257, 512, 1_000, 4_096, 32_000 })
        {
            var fitted = ThinkingBudget.FitWithin(100_000, max);
            fitted.Should().BeInRange(0, Math.Max(0, max - ThinkingBudget.MinimumAnswerTokens), $"cap {max}");
            (max - fitted).Should().BeGreaterThanOrEqualTo(Math.Min(max, max / 4), $"cap {max}");
        }
    }
}

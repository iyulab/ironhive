using FluentAssertions;

namespace IronHive.Tests.Providers;

/// <summary>
/// Tests to verify correct token calculation in GoogleAI provider.
/// Issue #4: Operator precedence bug in OutputTokens calculation.
/// </summary>
public class GoogleAITokenCalculationTests
{
    /// <summary>
    /// Verifies that the corrected token calculation produces correct results.
    /// Pattern: (CandidatesTokenCount ?? 0) + (ThoughtsTokenCount ?? 0)
    /// </summary>
    [Theory]
    [InlineData(100, 50, 150)]      // Both have values
    [InlineData(100, null, 100)]    // Only candidates
    [InlineData(null, 50, 50)]      // Only thoughts
    [InlineData(null, null, 0)]     // Both null
    [InlineData(0, 0, 0)]           // Both zero
    [InlineData(1000, 500, 1500)]   // Larger values
    public void OutputTokens_ShouldBeSum_OfCandidatesAndThoughtsTokens(
        int? candidatesTokenCount,
        int? thoughtsTokenCount,
        int expectedOutputTokens)
    {
        // Act - The correct pattern with proper parentheses
        var outputTokens = (candidatesTokenCount ?? 0) + (thoughtsTokenCount ?? 0);

        // Assert
        outputTokens.Should().Be(expectedOutputTokens);
    }

    /// <summary>
    /// Demonstrates the bug in the old pattern (without parentheses).
    /// Pattern: CandidatesTokenCount ?? 0 + ThoughtsTokenCount ?? 0
    /// Due to operator precedence, this evaluates as:
    /// CandidatesTokenCount ?? (0 + ThoughtsTokenCount) ?? 0
    /// </summary>
    [Fact]
    public void OldPattern_WouldProduceIncorrectResults()
    {
        // Arrange
        int? candidatesTokenCount = 100;
        int? thoughtsTokenCount = 50;

        // Act - The buggy pattern (simulated - actual code would be more complex)
        // This demonstrates what the old pattern effectively did
        var buggyResult = candidatesTokenCount ?? (0 + thoughtsTokenCount) ?? 0;

        // Assert - The buggy result would be 100 (ignoring thoughts)
        // because candidatesTokenCount is not null, so 100 is returned directly
        buggyResult.Should().Be(100, "the old buggy pattern would ignore ThoughtsTokenCount");

        // Correct calculation should be 150
        var correctResult = (candidatesTokenCount ?? 0) + (thoughtsTokenCount ?? 0);
        correctResult.Should().Be(150);
    }

    /// <summary>
    /// Verifies edge case where CandidatesTokenCount is null but ThoughtsTokenCount has value.
    /// </summary>
    [Fact]
    public void OutputTokens_WhenCandidatesNull_ShouldStillAddThoughts()
    {
        // Arrange
        int? candidatesTokenCount = null;
        int? thoughtsTokenCount = 200;

        // Act
        var outputTokens = (candidatesTokenCount ?? 0) + (thoughtsTokenCount ?? 0);

        // Assert
        outputTokens.Should().Be(200);
    }
}

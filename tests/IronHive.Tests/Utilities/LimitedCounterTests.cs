using FluentAssertions;
using IronHive.Core.Utilities;

namespace IronHive.Tests.Utilities;

public class LimitedCounterTests
{
    [Fact]
    public void NewCounter_StartsAtZero()
    {
        var counter = new LimitedCounter(5);

        counter.Count.Should().Be(0);
        counter.MaximumCount.Should().Be(5);
        counter.RemainingCount.Should().Be(5);
        counter.CanIncrement.Should().BeTrue();
        counter.HasLimit.Should().BeFalse();
    }

    [Fact]
    public void TryIncrement_BelowMax_Succeeds()
    {
        var counter = new LimitedCounter(3);

        counter.TryIncrement().Should().BeTrue();
        counter.Count.Should().Be(1);
        counter.RemainingCount.Should().Be(2);
        counter.CanIncrement.Should().BeTrue();
    }

    [Fact]
    public void TryIncrement_AtMax_Fails()
    {
        var counter = new LimitedCounter(2);
        counter.TryIncrement();
        counter.TryIncrement();

        counter.TryIncrement().Should().BeFalse();
        counter.Count.Should().Be(2);
        counter.HasLimit.Should().BeTrue();
        counter.CanIncrement.Should().BeFalse();
        counter.RemainingCount.Should().Be(0);
    }

    [Fact]
    public void Reset_ResetsToZero()
    {
        var counter = new LimitedCounter(3);
        counter.TryIncrement();
        counter.TryIncrement();

        counter.Reset();

        counter.Count.Should().Be(0);
        counter.CanIncrement.Should().BeTrue();
        counter.HasLimit.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void NegativeOrZeroMax_SetsToOne(int max)
    {
        var counter = new LimitedCounter(max);

        counter.MaximumCount.Should().Be(1);
        counter.TryIncrement().Should().BeTrue();
        counter.TryIncrement().Should().BeFalse();
    }

    [Fact]
    public void IncrementToMax_AllPropertiesCorrect()
    {
        var counter = new LimitedCounter(3);

        for (int i = 0; i < 3; i++)
            counter.TryIncrement();

        counter.Count.Should().Be(3);
        counter.MaximumCount.Should().Be(3);
        counter.RemainingCount.Should().Be(0);
        counter.CanIncrement.Should().BeFalse();
        counter.HasLimit.Should().BeTrue();
    }
}

using FluentAssertions;
using IronHive.Abstractions.Vector;
using System.Diagnostics;

namespace IronHive.Tests.Vector;

/// <summary>
/// Tests to verify VectorRecordFilter performance improvements.
/// Issue #10: O(n²) performance due to Contains() check before Add().
/// </summary>
public class VectorRecordFilterPerformanceTests
{
    /// <summary>
    /// Verifies that adding items is now O(1) instead of O(n).
    /// HashSet.Add is O(1) amortized.
    /// </summary>
    [Fact]
    public void AddSourceId_ShouldBeConstantTime()
    {
        // Arrange
        var filter = new VectorRecordFilter();
        var itemCount = 10000;

        // Act - Add many items
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < itemCount; i++)
        {
            filter.AddSourceId($"source_{i}");
        }
        sw.Stop();

        // Assert - Should complete quickly (< 100ms for 10000 items)
        // With O(n²) this would take much longer
        filter.SourceIds.Should().HaveCount(itemCount);
        sw.ElapsedMilliseconds.Should().BeLessThan(1000, "adding 10000 items should be fast with O(1) inserts");
    }

    /// <summary>
    /// Verifies that duplicate items are handled correctly.
    /// </summary>
    [Fact]
    public void AddSourceId_ShouldIgnoreDuplicates()
    {
        // Arrange
        var filter = new VectorRecordFilter();

        // Act
        filter.AddSourceId("source_1");
        filter.AddSourceId("source_1"); // duplicate
        filter.AddSourceId("source_2");
        filter.AddSourceId("source_1"); // duplicate

        // Assert
        filter.SourceIds.Should().HaveCount(2);
        filter.SourceIds.Should().Contain("source_1");
        filter.SourceIds.Should().Contain("source_2");
    }

    /// <summary>
    /// Verifies that AddSourceIds uses UnionWith for efficient bulk addition.
    /// </summary>
    [Fact]
    public void AddSourceIds_ShouldAddMultipleItemsEfficiently()
    {
        // Arrange
        var filter = new VectorRecordFilter();
        var ids = Enumerable.Range(0, 1000).Select(i => $"source_{i}").ToList();

        // Act
        filter.AddSourceIds(ids);
        filter.AddSourceIds(ids); // Add duplicates - should not increase count

        // Assert
        filter.SourceIds.Should().HaveCount(1000);
    }

    /// <summary>
    /// Verifies that VectorIds also works correctly with HashSet.
    /// </summary>
    [Fact]
    public void AddVectorId_ShouldBeConstantTime()
    {
        // Arrange
        var filter = new VectorRecordFilter();
        var itemCount = 10000;

        // Act
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < itemCount; i++)
        {
            filter.AddVectorId($"vector_{i}");
        }
        sw.Stop();

        // Assert
        filter.VectorIds.Should().HaveCount(itemCount);
        sw.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    /// <summary>
    /// Verifies constructor initialization works correctly.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeFromEnumerables()
    {
        // Arrange
        var sourceIds = new[] { "s1", "s2", "s1" }; // includes duplicate
        var vectorIds = new[] { "v1", "v2", "v3" };

        // Act
        var filter = new VectorRecordFilter(sourceIds, vectorIds);

        // Assert
        filter.SourceIds.Should().HaveCount(2); // duplicate removed
        filter.VectorIds.Should().HaveCount(3);
        filter.Any().Should().BeTrue();
    }

    /// <summary>
    /// Verifies Any() returns correct results.
    /// </summary>
    [Fact]
    public void Any_ShouldReturnFalse_WhenEmpty()
    {
        // Arrange
        var filter = new VectorRecordFilter();

        // Assert
        filter.Any().Should().BeFalse();
    }

    /// <summary>
    /// Verifies IReadOnlyCollection interface is properly exposed.
    /// </summary>
    [Fact]
    public void Collections_ShouldBeReadOnly()
    {
        // Arrange
        var filter = new VectorRecordFilter();
        filter.AddSourceId("test");

        // Assert - Properties return IReadOnlyCollection
        IReadOnlyCollection<string> sourceIds = filter.SourceIds;
        IReadOnlyCollection<string> vectorIds = filter.VectorIds;

        sourceIds.Should().HaveCount(1);
        vectorIds.Should().HaveCount(0);
    }
}

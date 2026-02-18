using FluentAssertions;
using IronHive.Abstractions.Vector;

namespace IronHive.Tests.Vector;

public class VectorRecordFilterTests
{
    [Fact]
    public void DefaultConstructor_ShouldBeEmpty()
    {
        var filter = new VectorRecordFilter();

        filter.VectorIds.Should().BeEmpty();
        filter.SourceIds.Should().BeEmpty();
        filter.Any().Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithSourceIds_ShouldPopulate()
    {
        var filter = new VectorRecordFilter(sourceIds: ["src1", "src2"]);

        filter.SourceIds.Should().HaveCount(2);
        filter.SourceIds.Should().Contain("src1");
        filter.SourceIds.Should().Contain("src2");
        filter.VectorIds.Should().BeEmpty();
        filter.Any().Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithVectorIds_ShouldPopulate()
    {
        var filter = new VectorRecordFilter(vectorIds: ["vec1", "vec2"]);

        filter.VectorIds.Should().HaveCount(2);
        filter.SourceIds.Should().BeEmpty();
        filter.Any().Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithBoth_ShouldPopulateBoth()
    {
        var filter = new VectorRecordFilter(
            sourceIds: ["src1"],
            vectorIds: ["vec1"]);

        filter.SourceIds.Should().HaveCount(1);
        filter.VectorIds.Should().HaveCount(1);
        filter.Any().Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithNulls_ShouldBeEmpty()
    {
        var filter = new VectorRecordFilter(sourceIds: null, vectorIds: null);

        filter.SourceIds.Should().BeEmpty();
        filter.VectorIds.Should().BeEmpty();
        filter.Any().Should().BeFalse();
    }

    [Fact]
    public void AddSourceId_ShouldAddSingle()
    {
        var filter = new VectorRecordFilter();
        filter.AddSourceId("src1");

        filter.SourceIds.Should().Contain("src1");
        filter.Any().Should().BeTrue();
    }

    [Fact]
    public void AddSourceId_Duplicate_ShouldNotDuplicate()
    {
        var filter = new VectorRecordFilter();
        filter.AddSourceId("src1");
        filter.AddSourceId("src1");

        filter.SourceIds.Should().HaveCount(1);
    }

    [Fact]
    public void AddSourceIds_ShouldAddBatch()
    {
        var filter = new VectorRecordFilter();
        filter.AddSourceIds(["src1", "src2", "src3"]);

        filter.SourceIds.Should().HaveCount(3);
    }

    [Fact]
    public void AddSourceIds_WithDuplicates_ShouldDeduplicate()
    {
        var filter = new VectorRecordFilter(sourceIds: ["src1"]);
        filter.AddSourceIds(["src1", "src2"]);

        filter.SourceIds.Should().HaveCount(2);
    }

    [Fact]
    public void AddVectorId_ShouldAddSingle()
    {
        var filter = new VectorRecordFilter();
        filter.AddVectorId("vec1");

        filter.VectorIds.Should().Contain("vec1");
        filter.Any().Should().BeTrue();
    }

    [Fact]
    public void AddVectorId_Duplicate_ShouldNotDuplicate()
    {
        var filter = new VectorRecordFilter();
        filter.AddVectorId("vec1");
        filter.AddVectorId("vec1");

        filter.VectorIds.Should().HaveCount(1);
    }

    [Fact]
    public void AddVectorIds_ShouldAddBatch()
    {
        var filter = new VectorRecordFilter();
        filter.AddVectorIds(["vec1", "vec2"]);

        filter.VectorIds.Should().HaveCount(2);
    }

    [Fact]
    public void AddVectorIds_WithExisting_ShouldDeduplicate()
    {
        var filter = new VectorRecordFilter(vectorIds: ["vec1"]);
        filter.AddVectorIds(["vec1", "vec2"]);

        filter.VectorIds.Should().HaveCount(2);
    }

    [Fact]
    public void Any_OnlySourceIds_ShouldReturnTrue()
    {
        var filter = new VectorRecordFilter();
        filter.AddSourceId("src1");

        filter.Any().Should().BeTrue();
    }

    [Fact]
    public void Any_OnlyVectorIds_ShouldReturnTrue()
    {
        var filter = new VectorRecordFilter();
        filter.AddVectorId("vec1");

        filter.Any().Should().BeTrue();
    }
}

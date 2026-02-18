using FluentAssertions;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using NSubstitute;

namespace IronHive.Tests.Tools;

public class ToolCollectionTests
{
    private static ITool CreateMockTool(string name)
    {
        var tool = Substitute.For<ITool>();
        tool.UniqueName.Returns(name);
        return tool;
    }

    #region Construction

    [Fact]
    public void Constructor_Default_Empty()
    {
        var collection = new ToolCollection();

        collection.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithTools_PopulatesCollection()
    {
        var tools = new[]
        {
            CreateMockTool("tool_a"),
            CreateMockTool("tool_b")
        };

        var collection = new ToolCollection(tools);

        collection.Count.Should().Be(2);
    }

    [Fact]
    public void Constructor_WithComparer_UsesComparer()
    {
        var collection = new ToolCollection(StringComparer.OrdinalIgnoreCase);

        collection.Set(CreateMockTool("Tool_A"));
        collection.TryGet("tool_a", out var tool).Should().BeTrue();
        tool!.UniqueName.Should().Be("Tool_A");
    }

    #endregion

    #region FilterBy

    [Fact]
    public void FilterBy_ExistingNames_ReturnsFilteredCollection()
    {
        var collection = new ToolCollection(new[]
        {
            CreateMockTool("tool_a"),
            CreateMockTool("tool_b"),
            CreateMockTool("tool_c")
        });

        var filtered = collection.FilterBy(["tool_a", "tool_c"]);

        filtered.Count.Should().Be(2);
        filtered.TryGet("tool_a", out _).Should().BeTrue();
        filtered.TryGet("tool_c", out _).Should().BeTrue();
        filtered.TryGet("tool_b", out _).Should().BeFalse();
    }

    [Fact]
    public void FilterBy_NonExistentNames_SkipsGracefully()
    {
        var collection = new ToolCollection(new[]
        {
            CreateMockTool("tool_a")
        });

        var filtered = collection.FilterBy(["tool_a", "tool_z"]);

        filtered.Count.Should().Be(1);
    }

    [Fact]
    public void FilterBy_EmptyNames_ReturnsEmpty()
    {
        var collection = new ToolCollection(new[]
        {
            CreateMockTool("tool_a")
        });

        var filtered = collection.FilterBy([]);

        filtered.Count.Should().Be(0);
    }

    [Fact]
    public void FilterBy_DuplicateNames_NoDuplicatesInResult()
    {
        var collection = new ToolCollection(new[]
        {
            CreateMockTool("tool_a")
        });

        var filtered = collection.FilterBy(["tool_a", "tool_a", "TOOL_A"]);

        filtered.Count.Should().Be(1);
    }

    [Fact]
    public void FilterBy_Null_ThrowsArgumentNullException()
    {
        var collection = new ToolCollection();

        var act = () => collection.FilterBy(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FilterBy_WithCaseInsensitiveCollection_MatchesCaseInsensitively()
    {
        var collection = new ToolCollection(
            new[] { CreateMockTool("Tool_A") },
            StringComparer.OrdinalIgnoreCase);

        var filtered = collection.FilterBy(["tool_a"]);

        filtered.Count.Should().Be(1);
    }

    #endregion

    #region IToolCollection Interface

    [Fact]
    public void Set_AddsTool()
    {
        var collection = new ToolCollection();
        var tool = CreateMockTool("tool_x");

        collection.Set(tool);

        collection.Count.Should().Be(1);
        collection.TryGet("tool_x", out var retrieved).Should().BeTrue();
        retrieved.Should().Be(tool);
    }

    [Fact]
    public void Set_SameKey_OverwritesPrevious()
    {
        var collection = new ToolCollection();
        var tool1 = CreateMockTool("tool_x");
        var tool2 = CreateMockTool("tool_x");

        collection.Set(tool1);
        collection.Set(tool2);

        collection.Count.Should().Be(1);
        collection.TryGet("tool_x", out var retrieved).Should().BeTrue();
        retrieved.Should().Be(tool2);
    }

    [Fact]
    public void Remove_ExistingKey_ReturnsTrue()
    {
        var collection = new ToolCollection(new[] { CreateMockTool("tool_a") });

        collection.Remove("tool_a").Should().BeTrue();
        collection.Count.Should().Be(0);
    }

    [Fact]
    public void Remove_NonExistentKey_ReturnsFalse()
    {
        var collection = new ToolCollection();

        collection.Remove("missing").Should().BeFalse();
    }

    #endregion
}

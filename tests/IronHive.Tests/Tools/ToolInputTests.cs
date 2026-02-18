using FluentAssertions;
using IronHive.Abstractions.Tools;
using System.Text.Json;

namespace IronHive.Tests.Tools;

public class ToolInputTests
{
    [Fact]
    public void Constructor_WithNull_ShouldCreateEmptyDictionary()
    {
        var input = new ToolInput(null);

        input.Count.Should().Be(0);
        input.Keys.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldCreateEmptyDictionary()
    {
        var input = new ToolInput("");

        input.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithWhitespaceString_ShouldCreateEmptyDictionary()
    {
        var input = new ToolInput("   ");

        input.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithDictionary_ShouldPopulate()
    {
        var dict = new Dictionary<string, object?> { ["key1"] = "value1", ["key2"] = 42 };
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(dict));
        var input = new ToolInput(jsonElement);

        input.Count.Should().Be(2);
        input.ContainsKey("key1").Should().BeTrue();
        input.ContainsKey("key2").Should().BeTrue();
    }

    [Fact]
    public void Indexer_ExistingKey_ShouldReturnValue()
    {
        var dict = new Dictionary<string, object?> { ["name"] = "test" };
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(dict));
        var input = new ToolInput(jsonElement);

        input["name"].Should().NotBeNull();
    }

    [Fact]
    public void Indexer_NonExistingKey_ShouldReturnNull()
    {
        var input = new ToolInput(null);

        input["missing"].Should().BeNull();
    }

    [Fact]
    public void TryGetValue_ExistingKey_ShouldReturnTrue()
    {
        var dict = new Dictionary<string, object?> { ["count"] = 42 };
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(dict));
        var input = new ToolInput(jsonElement);

        input.TryGetValue("count", out object? value).Should().BeTrue();
        value.Should().NotBeNull();
    }

    [Fact]
    public void TryGetValue_NonExistingKey_ShouldReturnFalse()
    {
        var input = new ToolInput(null);

        input.TryGetValue("missing", out object? value).Should().BeFalse();
    }

    [Fact]
    public void Options_ShouldBeSettable()
    {
        var input = new ToolInput(null, options: "my-options");

        input.Options.Should().Be("my-options");
    }

    [Fact]
    public void Services_ShouldBeSettable()
    {
        var input = new ToolInput(null, services: null);

        input.Services.Should().BeNull();
    }

    [Fact]
    public void ToString_ShouldReturnJson()
    {
        var input = new ToolInput(null);

        var result = input.ToString();

        result.Should().Be("{}");
    }

    [Fact]
    public void GetEnumerator_ShouldEnumerate()
    {
        var dict = new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 };
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(dict));
        var input = new ToolInput(jsonElement);

        var keys = new List<string>();
        foreach (var kvp in input)
        {
            keys.Add(kvp.Key);
        }

        keys.Should().HaveCount(2);
        keys.Should().Contain("a");
        keys.Should().Contain("b");
    }
}

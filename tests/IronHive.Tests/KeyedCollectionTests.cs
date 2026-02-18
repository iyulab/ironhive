using FluentAssertions;
using IronHive.Core;

namespace IronHive.Tests;

public class KeyedCollectionTests
{
    private sealed record TestItem(string Id, string Value);

    private static KeyedCollection<string, TestItem> CreateCollection(params TestItem[] items)
    {
        return new KeyedCollection<string, TestItem>(x => x.Id, items);
    }

    #region Construction

    [Fact]
    public void New_Empty_HasZeroCount()
    {
        var collection = CreateCollection();

        collection.Count.Should().Be(0);
        collection.IsReadOnly.Should().BeFalse();
        collection.Keys.Should().BeEmpty();
    }

    [Fact]
    public void New_WithItems_PopulatesCorrectly()
    {
        var collection = CreateCollection(
            new TestItem("a", "1"),
            new TestItem("b", "2"));

        collection.Count.Should().Be(2);
        collection.ContainsKey("a").Should().BeTrue();
        collection.ContainsKey("b").Should().BeTrue();
    }

    [Fact]
    public void New_DuplicateKeys_Throws()
    {
        var act = () => CreateCollection(
            new TestItem("a", "1"),
            new TestItem("a", "2"));

        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Add / Set

    [Fact]
    public void Add_NewItem_Succeeds()
    {
        var collection = CreateCollection();

        collection.Add(new TestItem("a", "1"));

        collection.Count.Should().Be(1);
        collection.TryGet("a", out var item).Should().BeTrue();
        item!.Value.Should().Be("1");
    }

    [Fact]
    public void Add_DuplicateKey_Throws()
    {
        var collection = CreateCollection(new TestItem("a", "1"));

        var act = () => collection.Add(new TestItem("a", "2"));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddRange_MultipleItems_AddsAll()
    {
        var collection = CreateCollection();

        collection.AddRange([new TestItem("a", "1"), new TestItem("b", "2")]);

        collection.Count.Should().Be(2);
    }

    [Fact]
    public void Set_NewItem_Adds()
    {
        var collection = CreateCollection();

        collection.Set(new TestItem("a", "1"));

        collection.Count.Should().Be(1);
    }

    [Fact]
    public void Set_ExistingKey_Replaces()
    {
        var collection = CreateCollection(new TestItem("a", "original"));

        collection.Set(new TestItem("a", "updated"));

        collection.Count.Should().Be(1);
        collection.TryGet("a", out var item).Should().BeTrue();
        item!.Value.Should().Be("updated");
    }

    [Fact]
    public void SetRange_MultipleItems_SetsAll()
    {
        var collection = CreateCollection(new TestItem("a", "1"));

        collection.SetRange([new TestItem("a", "updated"), new TestItem("b", "new")]);

        collection.Count.Should().Be(2);
        collection.TryGet("a", out var a).Should().BeTrue();
        a!.Value.Should().Be("updated");
    }

    #endregion

    #region TryGet / Contains

    [Fact]
    public void TryGet_Existing_ReturnsTrue()
    {
        var collection = CreateCollection(new TestItem("a", "1"));

        collection.TryGet("a", out var item).Should().BeTrue();
        item!.Value.Should().Be("1");
    }

    [Fact]
    public void TryGet_NonExisting_ReturnsFalse()
    {
        var collection = CreateCollection();

        collection.TryGet("nonexistent", out _).Should().BeFalse();
    }

    [Fact]
    public void Contains_ExistingItem_ReturnsTrue()
    {
        var item = new TestItem("a", "1");
        var collection = CreateCollection(item);

        collection.Contains(item).Should().BeTrue();
    }

    [Fact]
    public void ContainsKey_Existing_ReturnsTrue()
    {
        var collection = CreateCollection(new TestItem("a", "1"));

        collection.ContainsKey("a").Should().BeTrue();
        collection.ContainsKey("b").Should().BeFalse();
    }

    #endregion

    #region Remove

    [Fact]
    public void RemoveByKey_Existing_ReturnsTrue()
    {
        var collection = CreateCollection(new TestItem("a", "1"));

        collection.Remove("a").Should().BeTrue();
        collection.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveByKey_NonExisting_ReturnsFalse()
    {
        var collection = CreateCollection();

        collection.Remove("nonexistent").Should().BeFalse();
    }

    [Fact]
    public void RemoveByItem_Existing_ReturnsTrue()
    {
        var item = new TestItem("a", "1");
        var collection = CreateCollection(item);

        collection.Remove(item).Should().BeTrue();
        collection.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveAll_NoPredicate_RemovesAll()
    {
        var collection = CreateCollection(
            new TestItem("a", "1"),
            new TestItem("b", "2"),
            new TestItem("c", "3"));

        var removed = collection.RemoveAll();

        removed.Should().Be(3);
        collection.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveAll_WithPredicate_RemovesMatching()
    {
        var collection = CreateCollection(
            new TestItem("a", "keep"),
            new TestItem("b", "remove"),
            new TestItem("c", "remove"));

        var removed = collection.RemoveAll(i => i.Value == "remove");

        removed.Should().Be(2);
        collection.Count.Should().Be(1);
        collection.ContainsKey("a").Should().BeTrue();
    }

    [Fact]
    public void Clear_RemovesAll()
    {
        var collection = CreateCollection(
            new TestItem("a", "1"),
            new TestItem("b", "2"));

        collection.Clear();

        collection.Count.Should().Be(0);
    }

    #endregion

    #region Enumeration / CopyTo

    [Fact]
    public void GetEnumerator_ReturnsAllItems()
    {
        var collection = CreateCollection(
            new TestItem("a", "1"),
            new TestItem("b", "2"));

        var items = collection.ToList();

        items.Should().HaveCount(2);
    }

    [Fact]
    public void CopyTo_CopiesItems()
    {
        var collection = CreateCollection(
            new TestItem("a", "1"),
            new TestItem("b", "2"));

        var array = new TestItem[3];
        collection.CopyTo(array, 1);

        array[0].Should().BeNull();
        array[1].Should().NotBeNull();
        array[2].Should().NotBeNull();
    }

    [Fact]
    public void CopyTo_InsufficientSpace_Throws()
    {
        var collection = CreateCollection(
            new TestItem("a", "1"),
            new TestItem("b", "2"));

        var array = new TestItem[1];
        var act = () => collection.CopyTo(array, 0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Keys_ReturnsAllKeys()
    {
        var collection = CreateCollection(
            new TestItem("x", "1"),
            new TestItem("y", "2"));

        collection.Keys.Should().Contain("x").And.Contain("y");
    }

    #endregion

    #region Custom Comparer

    [Fact]
    public void CaseInsensitiveComparer_MatchesDifferentCase()
    {
        var collection = new KeyedCollection<string, TestItem>(
            x => x.Id,
            comparer: StringComparer.OrdinalIgnoreCase);

        collection.Add(new TestItem("Key", "1"));

        collection.ContainsKey("key").Should().BeTrue();
        collection.ContainsKey("KEY").Should().BeTrue();
        collection.TryGet("key", out var item).Should().BeTrue();
        item!.Value.Should().Be("1");
    }

    #endregion
}

using FluentAssertions;
using IronHive.Core;

namespace IronHive.Tests.Core;

// Test service hierarchy for KeyedServiceRegistry
public interface ITestService
{
    string Name { get; }
}

public sealed class ServiceAlpha : ITestService
{
    public string Name { get; init; } = "alpha";
}

public sealed class ServiceBeta : ITestService
{
    public string Name { get; init; } = "beta";
}

public class KeyedServiceRegistryTests
{
    private static KeyedServiceRegistry<string, ITestService> CreateRegistry()
        => new(StringComparer.OrdinalIgnoreCase);

    // --- Count ---

    [Fact]
    public void Count_EmptyRegistry_ReturnsZero()
    {
        var registry = CreateRegistry();

        registry.Count<ServiceAlpha>().Should().Be(0);
    }

    [Fact]
    public void Count_AfterAdd_ReturnsCorrect()
    {
        var registry = CreateRegistry();
        registry.TryAdd<ServiceAlpha>("key1", new ServiceAlpha());
        registry.TryAdd<ServiceAlpha>("key2", new ServiceAlpha());

        registry.Count<ServiceAlpha>().Should().Be(2);
    }

    [Fact]
    public void Count_DifferentBuckets_IndependentCounts()
    {
        var registry = CreateRegistry();
        registry.TryAdd<ServiceAlpha>("key1", new ServiceAlpha());
        registry.TryAdd<ServiceBeta>("key1", new ServiceBeta());

        registry.Count<ServiceAlpha>().Should().Be(1);
        registry.Count<ServiceBeta>().Should().Be(1);
    }

    // --- TryAdd ---

    [Fact]
    public void TryAdd_NewKey_ReturnsTrue()
    {
        var registry = CreateRegistry();

        registry.TryAdd<ServiceAlpha>("key1", new ServiceAlpha()).Should().BeTrue();
    }

    [Fact]
    public void TryAdd_DuplicateKey_ReturnsFalse()
    {
        var registry = CreateRegistry();
        registry.TryAdd<ServiceAlpha>("key1", new ServiceAlpha());

        registry.TryAdd<ServiceAlpha>("key1", new ServiceAlpha()).Should().BeFalse();
    }

    [Fact]
    public void TryAdd_TypeMismatch_ReturnsFalse()
    {
        var registry = CreateRegistry();
        // Trying to add a ServiceBeta as ServiceAlpha
        registry.TryAdd<ServiceAlpha>("key1", new ServiceBeta()).Should().BeFalse();
    }

    [Fact]
    public void TryAdd_NullItem_ThrowsArgumentNullException()
    {
        var registry = CreateRegistry();

        var act = () => registry.TryAdd<ServiceAlpha>("key1", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryAdd_SameKeyDifferentBucket_BothSucceed()
    {
        var registry = CreateRegistry();

        registry.TryAdd<ServiceAlpha>("shared", new ServiceAlpha()).Should().BeTrue();
        registry.TryAdd<ServiceBeta>("shared", new ServiceBeta()).Should().BeTrue();
    }

    // --- Contains ---

    [Fact]
    public void Contains_ExistingKey_ReturnsTrue()
    {
        var registry = CreateRegistry();
        registry.TryAdd<ServiceAlpha>("key1", new ServiceAlpha());

        registry.Contains<ServiceAlpha>("key1").Should().BeTrue();
    }

    [Fact]
    public void Contains_NonExistentKey_ReturnsFalse()
    {
        var registry = CreateRegistry();

        registry.Contains<ServiceAlpha>("missing").Should().BeFalse();
    }

    [Fact]
    public void Contains_CaseInsensitive_Works()
    {
        var registry = CreateRegistry();
        registry.TryAdd<ServiceAlpha>("MyKey", new ServiceAlpha());

        registry.Contains<ServiceAlpha>("mykey").Should().BeTrue();
        registry.Contains<ServiceAlpha>("MYKEY").Should().BeTrue();
    }

    // --- TryGet ---

    [Fact]
    public void TryGet_ExistingKey_ReturnsTrueWithValue()
    {
        var registry = CreateRegistry();
        var item = new ServiceAlpha { Name = "test" };
        registry.TryAdd<ServiceAlpha>("key1", item);

        registry.TryGet<ServiceAlpha>("key1", out var result).Should().BeTrue();
        result!.Name.Should().Be("test");
    }

    [Fact]
    public void TryGet_NonExistentKey_ReturnsFalse()
    {
        var registry = CreateRegistry();

        registry.TryGet<ServiceAlpha>("missing", out var result).Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void TryGet_WrongBucket_ReturnsFalse()
    {
        var registry = CreateRegistry();
        registry.TryAdd<ServiceAlpha>("key1", new ServiceAlpha());

        registry.TryGet<ServiceBeta>("key1", out _).Should().BeFalse();
    }

    // --- Set ---

    [Fact]
    public void Set_NewKey_AddsItem()
    {
        var registry = CreateRegistry();
        registry.Set<ServiceAlpha>("key1", new ServiceAlpha { Name = "first" });

        registry.TryGet<ServiceAlpha>("key1", out var result).Should().BeTrue();
        result!.Name.Should().Be("first");
    }

    [Fact]
    public void Set_ExistingKey_OverwritesItem()
    {
        var registry = CreateRegistry();
        registry.Set<ServiceAlpha>("key1", new ServiceAlpha { Name = "first" });
        registry.Set<ServiceAlpha>("key1", new ServiceAlpha { Name = "second" });

        registry.TryGet<ServiceAlpha>("key1", out var result).Should().BeTrue();
        result!.Name.Should().Be("second");
        registry.Count<ServiceAlpha>().Should().Be(1);
    }

    [Fact]
    public void Set_TypeMismatch_ThrowsArgumentException()
    {
        var registry = CreateRegistry();

        var act = () => registry.Set<ServiceAlpha>("key1", new ServiceBeta());

        act.Should().Throw<ArgumentException>();
    }

    // --- Remove ---

    [Fact]
    public void Remove_ExistingKey_ReturnsTrue()
    {
        var registry = CreateRegistry();
        registry.TryAdd<ServiceAlpha>("key1", new ServiceAlpha());

        registry.Remove<ServiceAlpha>("key1").Should().BeTrue();
        registry.Count<ServiceAlpha>().Should().Be(0);
    }

    [Fact]
    public void Remove_NonExistentKey_ReturnsFalse()
    {
        var registry = CreateRegistry();

        registry.Remove<ServiceAlpha>("missing").Should().BeFalse();
    }

    [Fact]
    public void Remove_WrongBucket_ReturnsFalse()
    {
        var registry = CreateRegistry();
        registry.TryAdd<ServiceAlpha>("key1", new ServiceAlpha());

        registry.Remove<ServiceBeta>("key1").Should().BeFalse();
    }

    // --- RemoveAll ---

    [Fact]
    public void RemoveAll_KeyInMultipleBuckets_RemovesAll()
    {
        var registry = CreateRegistry();
        registry.TryAdd<ServiceAlpha>("shared", new ServiceAlpha());
        registry.TryAdd<ServiceBeta>("shared", new ServiceBeta());

        registry.RemoveAll("shared").Should().Be(2);
        registry.Count<ServiceAlpha>().Should().Be(0);
        registry.Count<ServiceBeta>().Should().Be(0);
    }

    [Fact]
    public void RemoveAll_KeyNotFound_ReturnsZero()
    {
        var registry = CreateRegistry();

        registry.RemoveAll("missing").Should().Be(0);
    }

    // --- Entries ---

    [Fact]
    public void Entries_EmptyBucket_ReturnsEmpty()
    {
        var registry = CreateRegistry();

        registry.Entries<ServiceAlpha>().Should().BeEmpty();
    }

    [Fact]
    public void Entries_WithItems_ReturnsAllPairs()
    {
        var registry = CreateRegistry();
        registry.TryAdd<ServiceAlpha>("a", new ServiceAlpha { Name = "one" });
        registry.TryAdd<ServiceAlpha>("b", new ServiceAlpha { Name = "two" });

        var entries = registry.Entries<ServiceAlpha>().ToList();

        entries.Should().HaveCount(2);
        entries.Select(e => e.Key).Should().Contain(["a", "b"]);
    }

    [Fact]
    public void Entries_DifferentBucket_ReturnsOnlyMatchingType()
    {
        var registry = CreateRegistry();
        registry.TryAdd<ServiceAlpha>("a", new ServiceAlpha());
        registry.TryAdd<ServiceBeta>("b", new ServiceBeta());

        registry.Entries<ServiceAlpha>().Should().HaveCount(1);
        registry.Entries<ServiceBeta>().Should().HaveCount(1);
    }
}

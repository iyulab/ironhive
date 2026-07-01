using FluentAssertions;
using IronHive.Abstractions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Vector;
using IronHive.Abstractions.Workflow;
using IronHive.Core;
using NSubstitute;

namespace IronHive.Tests.Core;

/// <summary>
/// Tests for HiveServiceBuilder registration behavior.
/// In the new design, AddX methods use dictionary assignment (last write wins),
/// so there is no fail-fast on duplicate names.
/// </summary>
public class HiveServiceBuilderRegistrationTests
{
    // --- AddMessageGenerator (dict-based, last-write-wins) ---

    [Fact]
    public void AddMessageGenerator_DuplicateName_OverwritesPrevious()
    {
        // In the new design AddMessageGenerator overwrites on duplicate name
        var builder = new HiveServiceBuilder();
        var gen1 = Substitute.For<IMessageGenerator>();
        var gen2 = Substitute.For<IMessageGenerator>();

        builder.AddMessageGenerator("openai", gen1);

        // No throw — dictionary assignment
        var act = () => builder.AddMessageGenerator("openai", gen2);

        act.Should().NotThrow();
    }

    [Fact]
    public void AddMessageGenerator_DifferentNames_BothSucceed()
    {
        var builder = new HiveServiceBuilder();
        var gen1 = Substitute.For<IMessageGenerator>();
        var gen2 = Substitute.For<IMessageGenerator>();

        var act = () =>
        {
            builder.AddMessageGenerator("openai", gen1);
            builder.AddMessageGenerator("anthropic", gen2);
        };

        act.Should().NotThrow();
    }

    // --- AddEmbeddingGenerator (dict-based, last-write-wins) ---

    [Fact]
    public void AddEmbeddingGenerator_DuplicateName_OverwritesPrevious()
    {
        var builder = new HiveServiceBuilder();
        var gen1 = Substitute.For<IEmbeddingGenerator>();
        var gen2 = Substitute.For<IEmbeddingGenerator>();

        builder.AddEmbeddingGenerator("openai", gen1);

        var act = () => builder.AddEmbeddingGenerator("openai", gen2);

        act.Should().NotThrow();
    }

    // --- AddVectorStorage (dict-based, last-write-wins) ---

    [Fact]
    public void AddVectorStorage_DuplicateName_OverwritesPrevious()
    {
        var builder = new HiveServiceBuilder();
        var storage1 = Substitute.For<IVectorStorage>();
        var storage2 = Substitute.For<IVectorStorage>();

        builder.AddVectorStorage("main", storage1);

        var act = () => builder.AddVectorStorage("main", storage2);

        act.Should().NotThrow();
    }

    // --- HiveService DisposeAsync ---

    [Fact]
    public async Task HiveService_DisposeAsync_DoesNotThrow()
    {
        var builder = new HiveServiceBuilder();
        var service = builder.Build();

        var act = async () => await ((IAsyncDisposable)service).DisposeAsync();

        await act.Should().NotThrowAsync();
    }
}

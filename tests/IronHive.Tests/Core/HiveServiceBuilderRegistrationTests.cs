using FluentAssertions;
using IronHive.Abstractions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Workflow;
using IronHive.Core;
using NSubstitute;

namespace IronHive.Tests.Core;

/// <summary>
/// Tests for HiveServiceBuilder AddX fail-fast and SetX upsert behavior.
/// </summary>
public class HiveServiceBuilderRegistrationTests
{
    // --- AddMessageGenerator fail-fast ---

    [Fact]
    public void AddMessageGenerator_DuplicateName_ThrowsInvalidOperationException()
    {
        var builder = new HiveServiceBuilder();
        var gen1 = Substitute.For<IMessageGenerator>();
        var gen2 = Substitute.For<IMessageGenerator>();

        builder.AddMessageGenerator("openai", gen1);

        var act = () => builder.AddMessageGenerator("openai", gen2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*openai*")
            .WithMessage("*SetMessageGenerator*");
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

    // --- SetMessageGenerator upsert ---

    [Fact]
    public void SetMessageGenerator_DuplicateName_Replaces()
    {
        var builder = new HiveServiceBuilder();
        var gen1 = Substitute.For<IMessageGenerator>();
        var gen2 = Substitute.For<IMessageGenerator>();

        builder.AddMessageGenerator("openai", gen1);
        builder.SetMessageGenerator("openai", gen2);

        var service = builder.Build();
        service.Providers.TryGet<IMessageGenerator>("openai", out var resolved).Should().BeTrue();
        resolved.Should().BeSameAs(gen2);
    }

    [Fact]
    public void SetMessageGenerator_NewName_AddsEntry()
    {
        var builder = new HiveServiceBuilder();
        var gen = Substitute.For<IMessageGenerator>();

        builder.SetMessageGenerator("openai", gen);

        var service = builder.Build();
        service.Providers.TryGet<IMessageGenerator>("openai", out var resolved).Should().BeTrue();
        resolved.Should().BeSameAs(gen);
    }

    // --- AddEmbeddingGenerator fail-fast ---

    [Fact]
    public void AddEmbeddingGenerator_DuplicateName_ThrowsInvalidOperationException()
    {
        var builder = new HiveServiceBuilder();
        var gen1 = Substitute.For<IEmbeddingGenerator>();
        var gen2 = Substitute.For<IEmbeddingGenerator>();

        builder.AddEmbeddingGenerator("openai", gen1);

        var act = () => builder.AddEmbeddingGenerator("openai", gen2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*openai*")
            .WithMessage("*SetEmbeddingGenerator*");
    }

    // --- SetEmbeddingGenerator upsert ---

    [Fact]
    public void SetEmbeddingGenerator_DuplicateName_Replaces()
    {
        var builder = new HiveServiceBuilder();
        var gen1 = Substitute.For<IEmbeddingGenerator>();
        var gen2 = Substitute.For<IEmbeddingGenerator>();

        builder.AddEmbeddingGenerator("openai", gen1);
        builder.SetEmbeddingGenerator("openai", gen2);

        var service = builder.Build();
        service.Providers.TryGet<IEmbeddingGenerator>("openai", out var resolved).Should().BeTrue();
        resolved.Should().BeSameAs(gen2);
    }

    // --- AddVectorStorage fail-fast ---

    [Fact]
    public void AddVectorStorage_DuplicateName_ThrowsInvalidOperationException()
    {
        var builder = new HiveServiceBuilder();
        var storage1 = Substitute.For<IronHive.Abstractions.Vector.IVectorStorage>();
        var storage2 = Substitute.For<IronHive.Abstractions.Vector.IVectorStorage>();

        builder.AddVectorStorage("main", storage1);

        var act = () => builder.AddVectorStorage("main", storage2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*main*")
            .WithMessage("*SetVectorStorage*");
    }

    // --- SetVectorStorage upsert ---

    [Fact]
    public void SetVectorStorage_DuplicateName_Replaces()
    {
        var builder = new HiveServiceBuilder();
        var storage1 = Substitute.For<IronHive.Abstractions.Vector.IVectorStorage>();
        var storage2 = Substitute.For<IronHive.Abstractions.Vector.IVectorStorage>();

        builder.AddVectorStorage("main", storage1);
        builder.SetVectorStorage("main", storage2);

        var service = builder.Build();
        service.Storages.TryGet<IronHive.Abstractions.Vector.IVectorStorage>("main", out var resolved).Should().BeTrue();
        resolved.Should().BeSameAs(storage2);
    }

    // --- Workflow surface ---

    [Fact]
    public void Build_ShouldExpose_WorkflowFactory()
    {
        var builder = new HiveServiceBuilder();
        var service = builder.Build();

        service.Workflows.Should().NotBeNull();
        service.Workflows.Should().BeAssignableTo<IWorkflowFactory>();
    }

    [Fact]
    public void Workflows_CreateFrom_WithRegisteredStep_ReturnsWorkflow()
    {
        var builder = new HiveServiceBuilder();
        var step = Substitute.For<IWorkflowTask<object>>();
        builder.AddWorkflowStep<IWorkflowTask<object>>("my-step", (IWorkflowTask<object>)step);
        var service = builder.Build();

        var definition = new WorkflowDefinition
        {
            Name = "test-workflow",
            Steps = [new TaskNode { Id = "n1", Step = "my-step" }]
        };

        var workflow = service.Workflows.CreateFrom<object>(definition);
        workflow.Should().NotBeNull();
        workflow.Name.Should().Be("test-workflow");
    }

    // --- KeyedServiceRegistry IAsyncDisposable teardown ---

    [Fact]
    public async Task KeyedServiceRegistry_DisposeAsync_DisposesAsyncDisposableItems()
    {
        var registry = new KeyedServiceRegistry<string, TestRegistryBase>();
        var asyncItem = new AsyncDisposableItem();
        registry.TryAdd<AsyncDisposableItem>("key1", asyncItem);

        await registry.DisposeAsync();

        asyncItem.Disposed.Should().BeTrue();
    }

    [Fact]
    public async Task KeyedServiceRegistry_DisposeAsync_DisposesDisposableItems()
    {
        var registry = new KeyedServiceRegistry<string, TestRegistryBase>();
        var syncItem = new SyncDisposableItem();
        registry.TryAdd<SyncDisposableItem>("key1", syncItem);

        await registry.DisposeAsync();

        syncItem.Disposed.Should().BeTrue();
    }

    [Fact]
    public async Task HiveService_DisposeAsync_DoesNotThrow()
    {
        var builder = new HiveServiceBuilder();
        var service = builder.Build();

        var act = async () => await ((IAsyncDisposable)service).DisposeAsync();

        await act.Should().NotThrowAsync();
    }
}

// --- Test helpers ---

public abstract class TestRegistryBase : IDisposable
{
    public bool Disposed { get; protected set; }

    public virtual void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}

public sealed class AsyncDisposableItem : TestRegistryBase, IAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}

public sealed class SyncDisposableItem : TestRegistryBase
{
    public override void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}

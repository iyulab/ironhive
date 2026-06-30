using FluentAssertions;
using IronHive.Abstractions;
using IronHive.Abstractions.Models;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Workflow;
using IronHive.Core;

namespace IronHive.Tests.Core;

/// <summary>
/// Tests for HiveService core functionality.
/// </summary>
public class HiveServiceTests
{
    [Fact]
    public void Build_ShouldCreateHiveService_WithAllRequiredDependencies()
    {
        // Arrange
        var builder = new HiveServiceBuilder();

        // Act
        var service = builder.Build();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IHiveService>();
    }

    [Fact]
    public void Build_ShouldResolve_CatalogService()
    {
        // Arrange
        var builder = new HiveServiceBuilder();

        // Act
        var service = builder.Build();

        // Assert
        service.Models.Should().NotBeNull();
        service.Models.Should().BeAssignableTo<IModelService>();
    }

    [Fact]
    public void Build_ShouldResolve_MessageService()
    {
        // Arrange
        var builder = new HiveServiceBuilder();

        // Act
        var service = builder.Build();

        // Assert
        service.Messages.Should().NotBeNull();
        service.Messages.Should().BeAssignableTo<IMessageService>();
    }

    [Fact]
    public void Build_ShouldResolve_EmbeddingService()
    {
        // Arrange
        var builder = new HiveServiceBuilder();

        // Act
        var service = builder.Build();

        // Assert
        service.Embeddings.Should().NotBeNull();
        service.Embeddings.Should().BeAssignableTo<IEmbeddingService>();
    }

    [Fact]
    public void Build_ShouldResolve_MemoryService()
    {
        // Arrange
        var builder = new HiveServiceBuilder();

        // Act
        var service = builder.Build();

        // Assert
        service.Memory.Should().NotBeNull();
        service.Memory.Should().BeAssignableTo<IMemoryService>();
    }

    [Fact]
    public void Build_ShouldResolve_WorkflowFactory()
    {
        // Arrange
        var builder = new HiveServiceBuilder();

        // Act
        var service = builder.Build();

        // Assert
        service.Workflows.Should().NotBeNull();
        service.Workflows.Should().BeAssignableTo<IWorkflowFactory>();
    }

    [Fact]
    public void Build_MultipleTimes_ShouldCreateSeparateInstances()
    {
        // Arrange
        var builder1 = new HiveServiceBuilder();
        var builder2 = new HiveServiceBuilder();

        // Act
        var service1 = builder1.Build();
        var service2 = builder2.Build();

        // Assert
        service1.Should().NotBeSameAs(service2);
        service1.Messages.Should().NotBeSameAs(service2.Messages);
    }

    [Fact]
    public void Build_ShouldImplement_IAsyncDisposable()
    {
        // Arrange
        var builder = new HiveServiceBuilder();
        var service = builder.Build();

        // Assert
        service.Should().BeAssignableTo<IAsyncDisposable>();
    }
}

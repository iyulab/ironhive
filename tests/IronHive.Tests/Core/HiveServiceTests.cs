using FluentAssertions;
using IronHive.Abstractions;
using IronHive.Abstractions.Models;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Workflow;
using IronHive.Core;
using NSubstitute;

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
        service.Should().BeAssignableTo<IDisposable>();
    }

    [Fact]
    public void GetMessageGenerator_ShouldThrow_WhenNoneRegistered()
    {
        // Arrange
        var service = new HiveServiceBuilder().Build();

        // Act
        var act = () => service.GetMessageGenerator();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*No message generators*");
    }

    [Fact]
    public void GetMessageGenerator_ShouldAutoSelect_WhenExactlyOneRegistered()
    {
        // Arrange
        var generator = Substitute.For<IMessageGenerator>();
        var service = new HiveServiceBuilder()
            .AddMessageGenerator("openai", generator)
            .Build();

        // Act
        var result = service.GetMessageGenerator();

        // Assert
        result.Should().BeSameAs(generator);
    }

    [Fact]
    public void GetMessageGenerator_ShouldThrow_WhenMultipleRegisteredAndProviderUnspecified()
    {
        // Arrange
        var service = new HiveServiceBuilder()
            .AddMessageGenerator("openai", Substitute.For<IMessageGenerator>())
            .AddMessageGenerator("anthropic", Substitute.For<IMessageGenerator>())
            .Build();

        // Act
        var act = () => service.GetMessageGenerator();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*Multiple message generators*");
    }

    [Fact]
    public void GetMessageGenerator_ShouldReturnNamedProvider_WhenSpecified()
    {
        // Arrange
        var openai = Substitute.For<IMessageGenerator>();
        var anthropic = Substitute.For<IMessageGenerator>();
        var service = new HiveServiceBuilder()
            .AddMessageGenerator("openai", openai)
            .AddMessageGenerator("anthropic", anthropic)
            .Build();

        // Act
        var result = service.GetMessageGenerator("anthropic");

        // Assert
        result.Should().BeSameAs(anthropic);
    }

    [Fact]
    public void GetMessageGenerator_ShouldThrow_WhenProviderNotRegistered()
    {
        // Arrange
        var service = new HiveServiceBuilder()
            .AddMessageGenerator("openai", Substitute.For<IMessageGenerator>())
            .Build();

        // Act
        var act = () => service.GetMessageGenerator("nonexistent");

        // Assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("*nonexistent*");
    }

    [Fact]
    public void GetEmbeddingGenerator_ShouldThrow_WhenNoneRegistered()
    {
        // Arrange
        var service = new HiveServiceBuilder().Build();

        // Act
        var act = () => service.GetEmbeddingGenerator();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*No embedding generators*");
    }

    [Fact]
    public void GetEmbeddingGenerator_ShouldAutoSelect_WhenExactlyOneRegistered()
    {
        // Arrange
        var generator = Substitute.For<IEmbeddingGenerator>();
        var service = new HiveServiceBuilder()
            .AddEmbeddingGenerator("openai", generator)
            .Build();

        // Act
        var result = service.GetEmbeddingGenerator();

        // Assert
        result.Should().BeSameAs(generator);
    }

    [Fact]
    public void GetEmbeddingGenerator_ShouldReturnNamedProvider_WhenSpecified()
    {
        // Arrange
        var openai = Substitute.For<IEmbeddingGenerator>();
        var cohere = Substitute.For<IEmbeddingGenerator>();
        var service = new HiveServiceBuilder()
            .AddEmbeddingGenerator("openai", openai)
            .AddEmbeddingGenerator("cohere", cohere)
            .Build();

        // Act
        var result = service.GetEmbeddingGenerator("cohere");

        // Assert
        result.Should().BeSameAs(cohere);
    }

    [Fact]
    public void GetEmbeddingGenerator_ShouldThrow_WhenProviderNotRegistered()
    {
        // Arrange
        var service = new HiveServiceBuilder()
            .AddEmbeddingGenerator("openai", Substitute.For<IEmbeddingGenerator>())
            .Build();

        // Act
        var act = () => service.GetEmbeddingGenerator("nonexistent");

        // Assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("*nonexistent*");
    }
}

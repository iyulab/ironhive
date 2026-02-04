using FluentAssertions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Registries;
using IronHive.Core.Services;
using Moq;

namespace IronHive.Tests.Services;

/// <summary>
/// Tests for EmbeddingService.
/// P0-1.3: Embedding generation service tests with mocked dependencies.
/// </summary>
public class EmbeddingServiceTests
{
    private readonly Mock<IProviderRegistry> _mockProviderRegistry;
    private readonly EmbeddingService _service;

    public EmbeddingServiceTests()
    {
        _mockProviderRegistry = new Mock<IProviderRegistry>();
        _service = new EmbeddingService(_mockProviderRegistry.Object);
    }

    #region EmbedAsync Tests

    [Fact]
    public async Task EmbedAsync_ShouldThrow_WhenProviderNotFound()
    {
        // Arrange
        IEmbeddingGenerator? generator = null;
        _mockProviderRegistry
            .Setup(r => r.TryGet("nonexistent", out generator))
            .Returns(false);

        // Act
        var act = async () => await _service.EmbedAsync("nonexistent", "model", "input");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*nonexistent*");
    }

    [Fact]
    public async Task EmbedAsync_ShouldDelegateToGenerator()
    {
        // Arrange
        var expectedEmbedding = new float[] { 0.1f, 0.2f, 0.3f };
        var mockGenerator = new Mock<IEmbeddingGenerator>();
        mockGenerator
            .Setup(g => g.EmbedAsync("text-embedding-3-small", "Hello world", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmbedding);

        IEmbeddingGenerator? generator = mockGenerator.Object;
        _mockProviderRegistry
            .Setup(r => r.TryGet("openai", out generator))
            .Returns(true);

        // Act
        var result = await _service.EmbedAsync("openai", "text-embedding-3-small", "Hello world");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedEmbedding);
        mockGenerator.Verify(
            g => g.EmbedAsync("text-embedding-3-small", "Hello world", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EmbedAsync_ShouldReturnCorrectDimensions()
    {
        // Arrange
        var embedding1536d = Enumerable.Range(0, 1536).Select(i => (float)i / 1536).ToArray();
        var mockGenerator = new Mock<IEmbeddingGenerator>();
        mockGenerator
            .Setup(g => g.EmbedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding1536d);

        IEmbeddingGenerator? generator = mockGenerator.Object;
        _mockProviderRegistry
            .Setup(r => r.TryGet("openai", out generator))
            .Returns(true);

        // Act
        var result = await _service.EmbedAsync("openai", "text-embedding-ada-002", "test");

        // Assert
        result.Should().HaveCount(1536);
    }

    #endregion

    #region EmbedBatchAsync Tests

    [Fact]
    public async Task EmbedBatchAsync_ShouldThrow_WhenProviderNotFound()
    {
        // Arrange
        IEmbeddingGenerator? generator = null;
        _mockProviderRegistry
            .Setup(r => r.TryGet("nonexistent", out generator))
            .Returns(false);

        var inputs = new[] { "text1", "text2" };

        // Act
        var act = async () => await _service.EmbedBatchAsync("nonexistent", "model", inputs);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*nonexistent*");
    }

    [Fact]
    public async Task EmbedBatchAsync_ShouldDelegateToGenerator()
    {
        // Arrange
        var expectedResults = new List<EmbeddingResult>
        {
            new() { Index = 0, Embedding = [0.1f, 0.2f] },
            new() { Index = 1, Embedding = [0.3f, 0.4f] }
        };

        var mockGenerator = new Mock<IEmbeddingGenerator>();
        mockGenerator
            .Setup(g => g.EmbedBatchAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        IEmbeddingGenerator? generator = mockGenerator.Object;
        _mockProviderRegistry
            .Setup(r => r.TryGet("openai", out generator))
            .Returns(true);

        var inputs = new[] { "text1", "text2" };

        // Act
        var result = await _service.EmbedBatchAsync("openai", "text-embedding-3-small", inputs);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        mockGenerator.Verify(
            g => g.EmbedBatchAsync("text-embedding-3-small", inputs, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EmbedBatchAsync_ShouldPreserveOrder()
    {
        // Arrange
        var expectedResults = new List<EmbeddingResult>
        {
            new() { Index = 0, Embedding = [1.0f] },
            new() { Index = 1, Embedding = [2.0f] },
            new() { Index = 2, Embedding = [3.0f] }
        };

        var mockGenerator = new Mock<IEmbeddingGenerator>();
        mockGenerator
            .Setup(g => g.EmbedBatchAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        IEmbeddingGenerator? generator = mockGenerator.Object;
        _mockProviderRegistry
            .Setup(r => r.TryGet("openai", out generator))
            .Returns(true);

        var inputs = new[] { "first", "second", "third" };

        // Act
        var result = (await _service.EmbedBatchAsync("openai", "model", inputs)).ToList();

        // Assert
        result[0].Index.Should().Be(0);
        result[1].Index.Should().Be(1);
        result[2].Index.Should().Be(2);
    }

    #endregion

    #region CountTokensAsync Tests

    [Fact]
    public async Task CountTokensAsync_ShouldThrow_WhenProviderNotFound()
    {
        // Arrange
        IEmbeddingGenerator? generator = null;
        _mockProviderRegistry
            .Setup(r => r.TryGet("nonexistent", out generator))
            .Returns(false);

        // Act
        var act = async () => await _service.CountTokensAsync("nonexistent", "model", "input");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*nonexistent*");
    }

    [Fact]
    public async Task CountTokensAsync_ShouldDelegateToGenerator()
    {
        // Arrange
        var mockGenerator = new Mock<IEmbeddingGenerator>();
        mockGenerator
            .Setup(g => g.CountTokensAsync("text-embedding-3-small", "Hello world", It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        IEmbeddingGenerator? generator = mockGenerator.Object;
        _mockProviderRegistry
            .Setup(r => r.TryGet("openai", out generator))
            .Returns(true);

        // Act
        var result = await _service.CountTokensAsync("openai", "text-embedding-3-small", "Hello world");

        // Assert
        result.Should().Be(2);
        mockGenerator.Verify(
            g => g.CountTokensAsync("text-embedding-3-small", "Hello world", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CountTokensAsync_ShouldReturnZero_ForEmptyInput()
    {
        // Arrange
        var mockGenerator = new Mock<IEmbeddingGenerator>();
        mockGenerator
            .Setup(g => g.CountTokensAsync(It.IsAny<string>(), "", It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        IEmbeddingGenerator? generator = mockGenerator.Object;
        _mockProviderRegistry
            .Setup(r => r.TryGet("openai", out generator))
            .Returns(true);

        // Act
        var result = await _service.CountTokensAsync("openai", "model", "");

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region CountTokensBatchAsync Tests

    [Fact]
    public async Task CountTokensBatchAsync_ShouldThrow_WhenProviderNotFound()
    {
        // Arrange
        IEmbeddingGenerator? generator = null;
        _mockProviderRegistry
            .Setup(r => r.TryGet("nonexistent", out generator))
            .Returns(false);

        var inputs = new[] { "text1", "text2" };

        // Act
        var act = async () => await _service.CountTokensBatchAsync("nonexistent", "model", inputs);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*nonexistent*");
    }

    [Fact]
    public async Task CountTokensBatchAsync_ShouldDelegateToGenerator()
    {
        // Arrange
        var expectedResults = new List<EmbeddingTokens>
        {
            new() { Index = 0, Text = "Hello", TokenCount = 2 },
            new() { Index = 1, Text = "Hello world", TokenCount = 3 }
        };

        var mockGenerator = new Mock<IEmbeddingGenerator>();
        mockGenerator
            .Setup(g => g.CountTokensBatchAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        IEmbeddingGenerator? generator = mockGenerator.Object;
        _mockProviderRegistry
            .Setup(r => r.TryGet("openai", out generator))
            .Returns(true);

        var inputs = new[] { "Hello", "Hello world" };

        // Act
        var result = await _service.CountTokensBatchAsync("openai", "model", inputs);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        mockGenerator.Verify(
            g => g.CountTokensBatchAsync("model", inputs, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldNotThrow_WithValidDependency()
    {
        // Act
        var act = () => new EmbeddingService(_mockProviderRegistry.Object);

        // Assert
        act.Should().NotThrow();
    }

    #endregion
}

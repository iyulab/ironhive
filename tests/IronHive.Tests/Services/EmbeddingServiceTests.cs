using FluentAssertions;
using IronHive.Abstractions.Embedding;
using IronHive.Core.Services;
using NSubstitute;

namespace IronHive.Tests.Services;

/// <summary>
/// Tests for EmbeddingService.
/// </summary>
public class EmbeddingServiceTests
{
    private readonly IEmbeddingGenerator _mockGenerator;
    private readonly Dictionary<string, IEmbeddingGenerator> _generators;
    private readonly EmbeddingService _service;

    public EmbeddingServiceTests()
    {
        _mockGenerator = Substitute.For<IEmbeddingGenerator>();
        _generators = new Dictionary<string, IEmbeddingGenerator>();
        _service = new EmbeddingService(_generators);
    }

    #region EmbedAsync Tests

    [Fact]
    public async Task EmbedAsync_ShouldThrow_WhenProviderNotFound()
    {
        // Arrange — generators dict is empty

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
        _mockGenerator
            .EmbedAsync("text-embedding-3-small", "Hello world", Arg.Any<CancellationToken>())
            .Returns(expectedEmbedding);
        _generators["openai"] = _mockGenerator;

        // Act
        var result = await _service.EmbedAsync("openai", "text-embedding-3-small", "Hello world");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedEmbedding);
        await _mockGenerator.Received(1)
            .EmbedAsync("text-embedding-3-small", "Hello world", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EmbedAsync_ShouldReturnCorrectDimensions()
    {
        // Arrange
        var embedding1536d = Enumerable.Range(0, 1536).Select(i => (float)i / 1536).ToArray();
        _mockGenerator
            .EmbedAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(embedding1536d);
        _generators["openai"] = _mockGenerator;

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
        // Arrange — generators dict is empty
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
        _mockGenerator
            .EmbedBatchAsync(Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(expectedResults);
        _generators["openai"] = _mockGenerator;

        var inputs = new[] { "text1", "text2" };

        // Act
        var result = await _service.EmbedBatchAsync("openai", "text-embedding-3-small", inputs);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        await _mockGenerator.Received(1)
            .EmbedBatchAsync("text-embedding-3-small", inputs, Arg.Any<CancellationToken>());
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
        _mockGenerator
            .EmbedBatchAsync(Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(expectedResults);
        _generators["openai"] = _mockGenerator;

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
        // Arrange — generators dict is empty

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
        _mockGenerator
            .CountTokensAsync("text-embedding-3-small", "Hello world", Arg.Any<CancellationToken>())
            .Returns(2);
        _generators["openai"] = _mockGenerator;

        // Act
        var result = await _service.CountTokensAsync("openai", "text-embedding-3-small", "Hello world");

        // Assert
        result.Should().Be(2);
        await _mockGenerator.Received(1)
            .CountTokensAsync("text-embedding-3-small", "Hello world", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CountTokensAsync_ShouldReturnZero_ForEmptyInput()
    {
        // Arrange
        _mockGenerator
            .CountTokensAsync(Arg.Any<string>(), "", Arg.Any<CancellationToken>())
            .Returns(0);
        _generators["openai"] = _mockGenerator;

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
        // Arrange — generators dict is empty
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
        _mockGenerator
            .CountTokensBatchAsync(Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(expectedResults);
        _generators["openai"] = _mockGenerator;

        var inputs = new[] { "Hello", "Hello world" };

        // Act
        var result = await _service.CountTokensBatchAsync("openai", "model", inputs);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        await _mockGenerator.Received(1)
            .CountTokensBatchAsync("model", inputs, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldNotThrow_WithValidDependency()
    {
        // Act
        var act = () => new EmbeddingService(new Dictionary<string, IEmbeddingGenerator>());

        // Assert
        act.Should().NotThrow();
    }

    #endregion
}

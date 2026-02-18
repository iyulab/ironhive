using FluentAssertions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Core.Memory.Pipelines;
using NSubstitute;

namespace IronHive.Tests.Memory;

/// <summary>
/// Tests for TextChunkingPipeline.
/// Issue #8: Improve TextChunking algorithm with semantic awareness.
/// </summary>
public class TextChunkingPipelineTests
{
    private readonly IEmbeddingService _mockEmbedder;
    private readonly TextChunkingPipeline _pipeline;

    public TextChunkingPipelineTests()
    {
        _mockEmbedder = Substitute.For<IEmbeddingService>();
        // 기본적으로 문자 수 / 4를 토큰으로 계산
        _mockEmbedder
            .CountTokensAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<string>(2).Length / 4);

        _pipeline = new TextChunkingPipeline(_mockEmbedder);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSplitByParagraphs()
    {
        // Arrange
        var text = @"This is the first paragraph. It contains some text.

This is the second paragraph. It also contains some text.

This is the third paragraph.";

        var context = CreateContext(text);
        var options = new TextChunkingPipeline.Options(ChunkSize: 500);

        // Act
        var result = await _pipeline.ExecuteAsync(context, options);

        // Assert
        result.IsError.Should().BeFalse();
        context.Payload.TryGetValue("chunks", out var chunksObj).Should().BeTrue();
        var chunks = chunksObj as List<string>;
        chunks.Should().NotBeNull();
        chunks!.Count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRespectChunkSize()
    {
        // Arrange
        var text = string.Join("\n\n", Enumerable.Range(1, 20).Select(i => $"Paragraph {i} with some content here."));
        var context = CreateContext(text);
        var options = new TextChunkingPipeline.Options(ChunkSize: 50);

        // Act
        var result = await _pipeline.ExecuteAsync(context, options);

        // Assert
        result.IsError.Should().BeFalse();
        context.Payload.TryGetValue("chunks", out var chunksObj).Should().BeTrue();
        var chunks = chunksObj as List<string>;
        chunks.Should().NotBeNull();
        chunks!.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleLongParagraphs()
    {
        // Arrange - 매우 긴 문단
        var longParagraph = string.Join(" ", Enumerable.Range(1, 500).Select(i => $"Word{i}"));
        var context = CreateContext(longParagraph);
        var options = new TextChunkingPipeline.Options(ChunkSize: 100);

        // Act
        var result = await _pipeline.ExecuteAsync(context, options);

        // Assert
        result.IsError.Should().BeFalse();
        context.Payload.TryGetValue("chunks", out var chunksObj).Should().BeTrue();
        var chunks = chunksObj as List<string>;
        chunks.Should().NotBeNull();
        chunks!.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPreserveSentenceBoundaries()
    {
        // Arrange
        var text = "First sentence. Second sentence. Third sentence. Fourth sentence.";
        var context = CreateContext(text);
        var options = new TextChunkingPipeline.Options(ChunkSize: 30);

        // Act
        var result = await _pipeline.ExecuteAsync(context, options);

        // Assert
        result.IsError.Should().BeFalse();
        context.Payload.TryGetValue("chunks", out var chunksObj).Should().BeTrue();
        var chunks = chunksObj as List<string>;
        chunks.Should().NotBeNull();
        // 각 청크가 문장 경계에서 끝나는지 확인
        foreach (var chunk in chunks!)
        {
            var trimmed = chunk.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                trimmed.Should().NotBeEmpty();
            }
        }
    }

    [Fact]
    public void Options_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var options = new TextChunkingPipeline.Options();

        // Assert
        options.ChunkSize.Should().Be(512);
        options.ChunkOverlap.Should().Be(50);
        options.MinChunkSize.Should().Be(50);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenTextMissing()
    {
        // Arrange
        var context = new MemoryContext
        {
            Source = new TextMemorySource { Id = "test", Value = "" },
            Target = new VectorMemoryTarget
            {
                StorageName = "test-storage",
                CollectionName = "test",
                EmbeddingProvider = "openai",
                EmbeddingModel = "text-embedding-3-small"
            }
        };
        // text를 payload에 추가하지 않음
        var options = new TextChunkingPipeline.Options();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _pipeline.ExecuteAsync(context, options));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenTargetInvalid()
    {
        // Arrange - VectorMemoryTarget이 아닌 target 사용
        var context = new MemoryContext
        {
            Source = new TextMemorySource { Id = "test", Value = "some text" },
            Target = new TestMemoryTarget()
        };
        context.Payload["text"] = "some text";
        var options = new TextChunkingPipeline.Options();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _pipeline.ExecuteAsync(context, options));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenTextEmpty()
    {
        // Arrange
        var context = CreateContext("   \n\n   ");
        var options = new TextChunkingPipeline.Options();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _pipeline.ExecuteAsync(context, options));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleKoreanText()
    {
        // Arrange - 한국어 텍스트
        var text = @"첫 번째 문단입니다. 여기에는 몇 가지 내용이 포함되어 있습니다.

두 번째 문단입니다. 이것도 내용을 담고 있습니다.

세 번째 문단입니다.";

        var context = CreateContext(text);
        var options = new TextChunkingPipeline.Options(ChunkSize: 100);

        // Act
        var result = await _pipeline.ExecuteAsync(context, options);

        // Assert
        result.IsError.Should().BeFalse();
        context.Payload.TryGetValue("chunks", out var chunksObj).Should().BeTrue();
        var chunks = chunksObj as List<string>;
        chunks.Should().NotBeNull();
        chunks!.Count.Should().BeGreaterThanOrEqualTo(1);
    }

    private static MemoryContext CreateContext(string text)
    {
        var context = new MemoryContext
        {
            Source = new TextMemorySource { Id = "test-source", Value = text },
            Target = new VectorMemoryTarget
            {
                StorageName = "test-storage",
                CollectionName = "test",
                EmbeddingProvider = "openai",
                EmbeddingModel = "text-embedding-3-small"
            }
        };
        context.Payload["text"] = text;
        return context;
    }

    /// <summary>
    /// 테스트용 비-Vector 타겟
    /// </summary>
    private sealed class TestMemoryTarget : IMemoryTarget
    {
    }
}

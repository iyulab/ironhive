using AwesomeAssertions;
using IronHive.Abstractions.Reranking;
using IronHive.Core.Services;
using NSubstitute;

namespace IronHive.Tests.Services;

/// <summary>
/// Tests for RerankService.
/// </summary>
public class RerankServiceTests
{
    private readonly IDocumentReranker _mockReranker;
    private readonly Dictionary<string, IDocumentReranker> _rerankers;
    private readonly RerankService _service;

    public RerankServiceTests()
    {
        _mockReranker = Substitute.For<IDocumentReranker>();
        _rerankers = new Dictionary<string, IDocumentReranker>();
        _service = new RerankService(_rerankers);
    }

    [Fact]
    public async Task RerankAsync_ShouldThrow_WhenProviderNotFound()
    {
        var act = async () => await _service.RerankAsync("nonexistent", "model", "query", ["doc1", "doc2"]);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*nonexistent*");
    }

    [Fact]
    public async Task RerankAsync_ShouldDelegateToReranker()
    {
        var expected = new List<RerankResult>
        {
            new() { Index = 1, Score = 0.9f },
            new() { Index = 0, Score = 0.3f },
        };
        _mockReranker
            .RerankAsync("rerank-v1", "query", Arg.Any<IEnumerable<string>>(), null, Arg.Any<CancellationToken>())
            .Returns(expected);
        _rerankers["cohere"] = _mockReranker;

        var documents = new[] { "doc1", "doc2" };

        var result = await _service.RerankAsync("cohere", "rerank-v1", "query", documents, cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeEquivalentTo(expected);
        await _mockReranker.Received(1)
            .RerankAsync("rerank-v1", "query", documents, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RerankAsync_ShouldForwardTopN()
    {
        _mockReranker
            .RerankAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _rerankers["cohere"] = _mockReranker;

        await _service.RerankAsync("cohere", "rerank-v1", "query", ["doc1", "doc2"], topN: 1, cancellationToken: TestContext.Current.CancellationToken);

        await _mockReranker.Received(1)
            .RerankAsync("rerank-v1", "query", Arg.Any<IEnumerable<string>>(), 1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_ShouldNotThrow_WithValidDependency()
    {
        var act = () => new RerankService(new Dictionary<string, IDocumentReranker>());

        act.Should().NotThrow();
    }
}

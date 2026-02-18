using FluentAssertions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Vector;
using IronHive.Core.Memory;
using NSubstitute;

namespace IronHive.Tests.Memory;

public class MemoryCollectionTests
{
    private const string StorageName = "test-storage";
    private const string CollectionName = "test-collection";
    private const string EmbeddingProvider = "test-provider";
    private const string EmbeddingModel = "test-model";

    private static readonly float[] s_testEmbeddings = [0.1f, 0.2f, 0.3f];

    private readonly IStorageRegistry _storages;
    private readonly IEmbeddingService _embedder;

    public MemoryCollectionTests()
    {
        _storages = Substitute.For<IStorageRegistry>();
        _embedder = Substitute.For<IEmbeddingService>();
    }

    private MemoryCollection CreateSut()
    {
        return new MemoryCollection(_storages, _embedder)
        {
            StorageName = StorageName,
            CollectionName = CollectionName,
            EmbeddingProvider = EmbeddingProvider,
            EmbeddingModel = EmbeddingModel,
        };
    }

    private void SetupQueueStorage(string queueName, IQueueStorage queue)
    {
        IQueueStorage outQueue = null!;
        _storages.TryGet(Arg.Is(queueName), out outQueue).Returns(callInfo =>
        {
            callInfo[1] = queue;
            return true;
        });
    }

    private void SetupQueueStorageNotFound()
    {
        IQueueStorage outQueue = null!;
        _storages.TryGet(Arg.Any<string>(), out outQueue).Returns(callInfo =>
        {
            callInfo[1] = null;
            return false;
        });
    }

    private void SetupVectorStorage(IVectorStorage vectorStorage)
    {
        IVectorStorage outStorage = null!;
        _storages.TryGet(Arg.Is(StorageName), out outStorage).Returns(callInfo =>
        {
            callInfo[1] = vectorStorage;
            return true;
        });
    }

    private void SetupVectorStorageNotFound()
    {
        IVectorStorage outStorage = null!;
        _storages.TryGet(Arg.Any<string>(), out outStorage).Returns(callInfo =>
        {
            callInfo[1] = null;
            return false;
        });
    }

    #region IndexSourceAsync

    [Fact]
    public async Task IndexSourceAsync_QueueRegistered_EnqueuesMemoryContext()
    {
        // Arrange
        var queue = Substitute.For<IQueueStorage>();
        SetupQueueStorage("my-queue", queue);

        var source = Substitute.For<IMemorySource>();
        source.Id.Returns("src-1");
        var sut = CreateSut();

        // Act
        await sut.IndexSourceAsync("my-queue", source);

        // Assert
        await queue.Received(1).EnqueueAsync(
            Arg.Any<MemoryContext>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IndexSourceAsync_EnqueuesCorrectTarget()
    {
        // Arrange
        var queue = Substitute.For<IQueueStorage>();
        MemoryContext? captured = null;
        queue.When(q => q.EnqueueAsync(Arg.Any<MemoryContext>(), Arg.Any<CancellationToken>()))
            .Do(ci => captured = ci.Arg<MemoryContext>());
        SetupQueueStorage("q1", queue);

        var source = Substitute.For<IMemorySource>();
        var sut = CreateSut();

        // Act
        await sut.IndexSourceAsync("q1", source);

        // Assert
        captured.Should().NotBeNull();
        captured!.Source.Should().BeSameAs(source);
        var target = captured.Target.Should().BeOfType<VectorMemoryTarget>().Subject;
        target.StorageName.Should().Be(StorageName);
        target.CollectionName.Should().Be(CollectionName);
        target.EmbeddingProvider.Should().Be(EmbeddingProvider);
        target.EmbeddingModel.Should().Be(EmbeddingModel);
    }

    [Fact]
    public async Task IndexSourceAsync_QueueNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupQueueStorageNotFound();
        var source = Substitute.For<IMemorySource>();
        var sut = CreateSut();

        // Act
        var act = async () => await sut.IndexSourceAsync("missing-queue", source);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*missing-queue*");
    }

    #endregion

    #region DeindexSourceAsync

    [Fact]
    public async Task DeindexSourceAsync_StorageRegistered_DeletesVectorsWithFilter()
    {
        // Arrange
        var vectorStorage = Substitute.For<IVectorStorage>();
        SetupVectorStorage(vectorStorage);
        var sut = CreateSut();

        // Act
        await sut.DeindexSourceAsync("source-42");

        // Assert
        await vectorStorage.Received(1).DeleteVectorsAsync(
            Arg.Is(CollectionName),
            Arg.Is<VectorRecordFilter>(f => f != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeindexSourceAsync_StorageNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupVectorStorageNotFound();
        var sut = CreateSut();

        // Act
        var act = async () => await sut.DeindexSourceAsync("source-1");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{StorageName}*");
    }

    #endregion

    #region SemanticSearchAsync

    [Fact]
    public async Task SemanticSearchAsync_StorageRegistered_ReturnsSearchResult()
    {
        // Arrange
        var vectorStorage = Substitute.For<IVectorStorage>();
        SetupVectorStorage(vectorStorage);

        _embedder.EmbedAsync(EmbeddingProvider, EmbeddingModel, "test query", Arg.Any<CancellationToken>())
            .Returns(s_testEmbeddings);

        var scoredRecords = new List<ScoredVectorRecord>();
        vectorStorage.SearchVectorsAsync(
                CollectionName, s_testEmbeddings, 0f, 5, null, Arg.Any<CancellationToken>())
            .Returns(scoredRecords);
        var sut = CreateSut();

        // Act
        var result = await sut.SemanticSearchAsync("test query");

        // Assert
        result.CollectionName.Should().Be(CollectionName);
        result.Query.Should().Be("test query");
        result.Results.Should().BeSameAs(scoredRecords);
    }

    [Fact]
    public async Task SemanticSearchAsync_WithOptions_PassesMinScoreAndLimit()
    {
        // Arrange
        var vectorStorage = Substitute.For<IVectorStorage>();
        SetupVectorStorage(vectorStorage);

        _embedder.EmbedAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(s_testEmbeddings);

        vectorStorage.SearchVectorsAsync(
                Arg.Any<string>(), Arg.Any<float[]>(), Arg.Any<float>(), Arg.Any<int>(),
                Arg.Any<VectorRecordFilter?>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<ScoredVectorRecord>());

        var options = new SearchOptions { MinScore = 0.5f, Limit = 10 };
        var sut = CreateSut();

        // Act
        await sut.SemanticSearchAsync("query", options);

        // Assert
        await vectorStorage.Received(1).SearchVectorsAsync(
            CollectionName,
            Arg.Any<float[]>(),
            0.5f,
            10,
            Arg.Any<VectorRecordFilter?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SemanticSearchAsync_WithSourceIds_PassesFilter()
    {
        // Arrange
        var vectorStorage = Substitute.For<IVectorStorage>();
        SetupVectorStorage(vectorStorage);

        _embedder.EmbedAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(s_testEmbeddings);

        vectorStorage.SearchVectorsAsync(
                Arg.Any<string>(), Arg.Any<float[]>(), Arg.Any<float>(), Arg.Any<int>(),
                Arg.Any<VectorRecordFilter?>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<ScoredVectorRecord>());

        var options = new SearchOptions { SourceIds = ["src-1", "src-2"] };
        var sut = CreateSut();

        // Act
        await sut.SemanticSearchAsync("query", options);

        // Assert
        await vectorStorage.Received(1).SearchVectorsAsync(
            CollectionName,
            Arg.Any<float[]>(),
            Arg.Any<float>(),
            Arg.Any<int>(),
            Arg.Is<VectorRecordFilter?>(f => f != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SemanticSearchAsync_NullOptions_UsesDefaults()
    {
        // Arrange
        var vectorStorage = Substitute.For<IVectorStorage>();
        SetupVectorStorage(vectorStorage);

        _embedder.EmbedAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(s_testEmbeddings);

        vectorStorage.SearchVectorsAsync(
                Arg.Any<string>(), Arg.Any<float[]>(), Arg.Any<float>(), Arg.Any<int>(),
                Arg.Any<VectorRecordFilter?>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<ScoredVectorRecord>());

        var sut = CreateSut();

        // Act
        await sut.SemanticSearchAsync("query");

        // Assert: default options = MinScore 0, Limit 5, no filter
        await vectorStorage.Received(1).SearchVectorsAsync(
            CollectionName,
            Arg.Any<float[]>(),
            0f,
            5,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SemanticSearchAsync_StorageNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupVectorStorageNotFound();
        var sut = CreateSut();

        // Act
        var act = async () => await sut.SemanticSearchAsync("query");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{StorageName}*");
    }

    #endregion
}

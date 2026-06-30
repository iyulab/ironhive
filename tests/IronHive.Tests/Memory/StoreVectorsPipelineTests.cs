using FluentAssertions;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Vector;
using IronHive.Core.Memory.Pipelines;
using NSubstitute;

namespace IronHive.Tests.Memory;

public class StoreVectorsPipelineTests
{
    private readonly Dictionary<string, IVectorStorage> _storages = new();
    private readonly IVectorStorage _mockVectorStorage;
    private readonly StoreVectorsPipeline _pipeline;

    public StoreVectorsPipelineTests()
    {
        _mockVectorStorage = Substitute.For<IVectorStorage>();
        _pipeline = new StoreVectorsPipeline(_storages);
    }

    #region Success Path

    [Fact]
    public async Task ExecuteAsync_ValidContext_ReturnsSuccess()
    {
        var context = CreateContext();
        _storages["test-storage"] = _mockVectorStorage;

        var result = await _pipeline.ExecuteAsync(context);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_UpsertsCalled_WithCorrectCollectionName()
    {
        var context = CreateContext();
        _storages["test-storage"] = _mockVectorStorage;

        await _pipeline.ExecuteAsync(context);

        await _mockVectorStorage.Received(1).UpsertVectorsAsync(
            "test-collection",
            Arg.Any<IEnumerable<VectorRecord>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesVectorRecords()
    {
        var vectors = new List<VectorRecord>
        {
            new() { VectorId = "v1", SourceId = "s1", Vectors = [0.1f, 0.2f], Payload = new Dictionary<string, object?>() },
            new() { VectorId = "v2", SourceId = "s1", Vectors = [0.3f, 0.4f], Payload = new Dictionary<string, object?>() }
        };
        var context = CreateContext(vectors);
        _storages["test-storage"] = _mockVectorStorage;

        await _pipeline.ExecuteAsync(context);

        await _mockVectorStorage.Received(1).UpsertVectorsAsync(
            "test-collection",
            Arg.Is<IEnumerable<VectorRecord>>(v => v.Count() == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesCancellationToken()
    {
        var context = CreateContext();
        _storages["test-storage"] = _mockVectorStorage;
        using var cts = new CancellationTokenSource();

        await _pipeline.ExecuteAsync(context, cts.Token);

        await _mockVectorStorage.Received(1).UpsertVectorsAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<VectorRecord>>(),
            cts.Token);
    }

    [Fact]
    public async Task ExecuteAsync_LooksUpStorageByName()
    {
        var context = CreateContext();
        _storages["test-storage"] = _mockVectorStorage;

        await _pipeline.ExecuteAsync(context);

        // Verify the mock vector storage was used (meaning the lookup succeeded)
        await _mockVectorStorage.Received(1).UpsertVectorsAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<VectorRecord>>(),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task ExecuteAsync_NonVectorTarget_Throws()
    {
        var context = new MemoryContext
        {
            Source = Substitute.For<IMemorySource>(),
            Target = Substitute.For<IMemoryTarget>()
        };

        var act = () => _pipeline.ExecuteAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*target*");
    }

    [Fact]
    public async Task ExecuteAsync_UnregisteredStorage_Throws()
    {
        var context = CreateContext();
        // _storages is empty — TryGetValue returns false

        var act = () => _pipeline.ExecuteAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*test-storage*");
    }

    [Fact]
    public async Task ExecuteAsync_MissingVectors_Throws()
    {
        var context = new MemoryContext
        {
            Source = Substitute.For<IMemorySource>(),
            Target = new VectorMemoryTarget
            {
                StorageName = "test-storage",
                CollectionName = "test-collection",
                EmbeddingProvider = "p",
                EmbeddingModel = "m"
            }
        };
        // No "vectors" key in payload
        _storages["test-storage"] = _mockVectorStorage;

        var act = () => _pipeline.ExecuteAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*vectors*");
    }

    #endregion

    #region Helpers

    private static MemoryContext CreateContext(List<VectorRecord>? vectors = null)
    {
        vectors ??=
        [
            new() { VectorId = "v1", SourceId = "s1", Vectors = [0.1f], Payload = new Dictionary<string, object?>() }
        ];
        var context = new MemoryContext
        {
            Source = new TextMemorySource { Id = "test-source", Value = "text" },
            Target = new VectorMemoryTarget
            {
                StorageName = "test-storage",
                CollectionName = "test-collection",
                EmbeddingProvider = "test-provider",
                EmbeddingModel = "test-model"
            }
        };
        context.Payload["vectors"] = vectors;
        return context;
    }

    #endregion
}

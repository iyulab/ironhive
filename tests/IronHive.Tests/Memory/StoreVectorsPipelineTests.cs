using FluentAssertions;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Vector;
using IronHive.Core.Memory.Pipelines;
using NSubstitute;

namespace IronHive.Tests.Memory;

public class StoreVectorsPipelineTests
{
    private readonly IStorageRegistry _mockStorages;
    private readonly IVectorStorage _mockVectorStorage;
    private readonly StoreVectorsPipeline _pipeline;

    public StoreVectorsPipelineTests()
    {
        _mockStorages = Substitute.For<IStorageRegistry>();
        _mockVectorStorage = Substitute.For<IVectorStorage>();
        _pipeline = new StoreVectorsPipeline(_mockStorages);
    }

    #region Success Path

    [Fact]
    public async Task ExecuteAsync_ValidContext_ReturnsSuccess()
    {
        var context = CreateContext();
        SetupStorageLookup();

        var result = await _pipeline.ExecuteAsync(context);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_UpsertsCalled_WithCorrectCollectionName()
    {
        var context = CreateContext();
        SetupStorageLookup();

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
        SetupStorageLookup();

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
        SetupStorageLookup();
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
        SetupStorageLookup();

        await _pipeline.ExecuteAsync(context);

        _mockStorages.Received(1).TryGet<IVectorStorage>(
            "test-storage", out Arg.Any<IVectorStorage?>());
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
        // Don't setup storage lookup → TryGet returns false

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
        SetupStorageLookup();

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

    private void SetupStorageLookup()
    {
        _mockStorages.TryGet<IVectorStorage>("test-storage", out Arg.Any<IVectorStorage?>())
            .Returns(x =>
            {
                x[1] = _mockVectorStorage;
                return true;
            });
    }

    #endregion
}

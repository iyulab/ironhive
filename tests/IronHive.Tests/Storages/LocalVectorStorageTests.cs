using FluentAssertions;
using IronHive.Abstractions.Vector;
using IronHive.Core.Storages;

namespace IronHive.Tests.Storages;

/// <summary>
/// Tests for LocalVectorStorage.
/// P0-1.3: SQLite vector storage integration tests.
/// Note: These tests require sqlite-vec module and may be skipped in CI environments.
/// Use [Trait("Category", "Integration")] for filtering.
/// </summary>
[Trait("Category", "Integration")]
public class LocalVectorStorageTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly LocalVectorStorage _storage;

    public LocalVectorStorageTests()
    {
        // Use a temp file for each test run
        _testDbPath = Path.Combine(Path.GetTempPath(), $"ironhive_test_{Guid.NewGuid()}.db");
        var config = new LocalVectorConfig
        {
            DatabasePath = _testDbPath
        };
        _storage = new LocalVectorStorage(config);
    }

    public void Dispose()
    {
        _storage.Dispose();
        // Clean up test database
        if (File.Exists(_testDbPath))
        {
            try
            {
                File.Delete(_testDbPath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        GC.SuppressFinalize(this);
    }

    #region Collection Management Tests

    [Fact]
    public async Task CreateCollectionAsync_ShouldCreateCollection()
    {
        // Arrange
        var collection = new VectorCollectionInfo
        {
            Name = "test_collection",
            Dimensions = 384,
            EmbeddingProvider = "openai",
            EmbeddingModel = "text-embedding-3-small"
        };

        // Act
        await _storage.CreateCollectionAsync(collection);
        var exists = await _storage.CollectionExistsAsync("test_collection");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CollectionExistsAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Act
        var exists = await _storage.CollectionExistsAsync("nonexistent_collection");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetCollectionInfoAsync_ShouldReturnInfo()
    {
        // Arrange
        var collection = new VectorCollectionInfo
        {
            Name = "info_test",
            Dimensions = 1536,
            EmbeddingProvider = "openai",
            EmbeddingModel = "text-embedding-ada-002"
        };
        await _storage.CreateCollectionAsync(collection);

        // Act
        var info = await _storage.GetCollectionInfoAsync("info_test");

        // Assert
        info.Should().NotBeNull();
        info!.Name.Should().Be("info_test");
        info.Dimensions.Should().Be(1536);
        info.EmbeddingProvider.Should().Be("openai");
        info.EmbeddingModel.Should().Be("text-embedding-ada-002");
    }

    [Fact]
    public async Task GetCollectionInfoAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var info = await _storage.GetCollectionInfoAsync("nonexistent");

        // Assert
        info.Should().BeNull();
    }

    [Fact]
    public async Task ListCollectionsAsync_ShouldReturnAllCollections()
    {
        // Arrange
        await _storage.CreateCollectionAsync(new VectorCollectionInfo
        {
            Name = "collection_a",
            Dimensions = 384,
            EmbeddingProvider = "openai",
            EmbeddingModel = "model-a"
        });
        await _storage.CreateCollectionAsync(new VectorCollectionInfo
        {
            Name = "collection_b",
            Dimensions = 768,
            EmbeddingProvider = "anthropic",
            EmbeddingModel = "model-b"
        });

        // Act
        var collections = (await _storage.ListCollectionsAsync()).ToList();

        // Assert
        collections.Should().HaveCount(2);
        collections.Select(c => c.Name).Should().Contain("collection_a");
        collections.Select(c => c.Name).Should().Contain("collection_b");
    }

    [Fact]
    public async Task DeleteCollectionAsync_ShouldRemoveCollection()
    {
        // Arrange
        await _storage.CreateCollectionAsync(new VectorCollectionInfo
        {
            Name = "to_delete",
            Dimensions = 384,
            EmbeddingProvider = "openai",
            EmbeddingModel = "model"
        });

        // Act
        await _storage.DeleteCollectionAsync("to_delete");
        var exists = await _storage.CollectionExistsAsync("to_delete");

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region Collection Name Validation Tests

    [Fact]
    public async Task CreateCollectionAsync_ShouldNormalizeName_ToLowercase()
    {
        // Arrange
        var collection = new VectorCollectionInfo
        {
            Name = "MyCollection",
            Dimensions = 384,
            EmbeddingProvider = "openai",
            EmbeddingModel = "model"
        };

        // Act
        await _storage.CreateCollectionAsync(collection);
        var exists = await _storage.CollectionExistsAsync("mycollection");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCollectionAsync_ShouldThrow_WhenNameIsEmpty()
    {
        // Arrange
        var collection = new VectorCollectionInfo
        {
            Name = "",
            Dimensions = 384,
            EmbeddingProvider = "openai",
            EmbeddingModel = "model"
        };

        // Act
        var act = async () => await _storage.CreateCollectionAsync(collection);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateCollectionAsync_ShouldThrow_WhenNameHasInvalidCharacters()
    {
        // Arrange
        var collection = new VectorCollectionInfo
        {
            Name = "invalid-name-with-dash",
            Dimensions = 384,
            EmbeddingProvider = "openai",
            EmbeddingModel = "model"
        };

        // Act
        var act = async () => await _storage.CreateCollectionAsync(collection);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateCollectionAsync_ShouldThrow_WhenNameStartsWithNumber()
    {
        // Arrange
        var collection = new VectorCollectionInfo
        {
            Name = "123collection",
            Dimensions = 384,
            EmbeddingProvider = "openai",
            EmbeddingModel = "model"
        };

        // Act
        var act = async () => await _storage.CreateCollectionAsync(collection);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region Vector Operations Tests

    [Fact]
    public async Task UpsertVectorsAsync_ShouldInsertVector()
    {
        // Arrange
        await CreateTestCollection("vectors_test", 4);

        var vectors = new List<VectorRecord>
        {
            new()
            {
                VectorId = "vec-1",
                SourceId = "source-1",
                Vectors = [0.1f, 0.2f, 0.3f, 0.4f],
                Payload = new Dictionary<string, object?> { ["text"] = "Hello world" }
            }
        };

        // Act
        await _storage.UpsertVectorsAsync("vectors_test", vectors);
        var found = (await _storage.FindVectorsAsync("vectors_test", filter: new VectorRecordFilter(vectorIds: ["vec-1"]))).ToList();

        // Assert
        found.Should().HaveCount(1);
        found[0].VectorId.Should().Be("vec-1");
        found[0].SourceId.Should().Be("source-1");
    }

    [Fact]
    public async Task UpsertVectorsAsync_ShouldUpdateExistingVector()
    {
        // Arrange
        await CreateTestCollection("upsert_test", 4);

        var initialVector = new VectorRecord
        {
            VectorId = "vec-1",
            SourceId = "source-1",
            Vectors = [0.1f, 0.2f, 0.3f, 0.4f],
            Payload = new Dictionary<string, object?> { ["version"] = 1 }
        };
        await _storage.UpsertVectorsAsync("upsert_test", [initialVector]);

        var updatedVector = new VectorRecord
        {
            VectorId = "vec-1",
            SourceId = "source-updated",
            Vectors = [0.5f, 0.6f, 0.7f, 0.8f],
            Payload = new Dictionary<string, object?> { ["version"] = 2 }
        };

        // Act
        await _storage.UpsertVectorsAsync("upsert_test", [updatedVector]);
        var found = (await _storage.FindVectorsAsync("upsert_test", filter: new VectorRecordFilter(vectorIds: ["vec-1"]))).ToList();

        // Assert
        found.Should().HaveCount(1);
        found[0].SourceId.Should().Be("source-updated");
    }

    [Fact]
    public async Task DeleteVectorsAsync_ShouldRemoveVector()
    {
        // Arrange
        await CreateTestCollection("delete_test", 4);

        var vectors = new List<VectorRecord>
        {
            new()
            {
                VectorId = "vec-to-delete",
                SourceId = "source-1",
                Vectors = [0.1f, 0.2f, 0.3f, 0.4f],
                Payload = new Dictionary<string, object?>()
            }
        };
        await _storage.UpsertVectorsAsync("delete_test", vectors);

        // Act
        await _storage.DeleteVectorsAsync("delete_test", new VectorRecordFilter(vectorIds: ["vec-to-delete"]));

        var found = (await _storage.FindVectorsAsync("delete_test", filter: new VectorRecordFilter(vectorIds: ["vec-to-delete"]))).ToList();

        // Assert
        found.Should().BeEmpty();
    }

    [Fact]
    public async Task FindVectorsAsync_ShouldFilterBySourceId()
    {
        // Arrange
        await CreateTestCollection("find_source_test", 4);

        var vectors = new List<VectorRecord>
        {
            new() { VectorId = "vec-1", SourceId = "doc-1", Vectors = [0.1f, 0.2f, 0.3f, 0.4f], Payload = new Dictionary<string, object?>() },
            new() { VectorId = "vec-2", SourceId = "doc-1", Vectors = [0.2f, 0.3f, 0.4f, 0.5f], Payload = new Dictionary<string, object?>() },
            new() { VectorId = "vec-3", SourceId = "doc-2", Vectors = [0.3f, 0.4f, 0.5f, 0.6f], Payload = new Dictionary<string, object?>() }
        };
        await _storage.UpsertVectorsAsync("find_source_test", vectors);

        // Act
        var found = (await _storage.FindVectorsAsync("find_source_test", filter: new VectorRecordFilter(sourceIds: ["doc-1"]))).ToList();

        // Assert
        found.Should().HaveCount(2);
        found.All(v => v.SourceId == "doc-1").Should().BeTrue();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchVectorsAsync_ShouldReturnScoredResults()
    {
        // Arrange
        await CreateTestCollection("search_test", 4);

        var vectors = new List<VectorRecord>
        {
            new() { VectorId = "vec-1", SourceId = "doc-1", Vectors = [1.0f, 0.0f, 0.0f, 0.0f], Payload = new Dictionary<string, object?>() },
            new() { VectorId = "vec-2", SourceId = "doc-2", Vectors = [0.0f, 1.0f, 0.0f, 0.0f], Payload = new Dictionary<string, object?>() },
            new() { VectorId = "vec-3", SourceId = "doc-3", Vectors = [0.9f, 0.1f, 0.0f, 0.0f], Payload = new Dictionary<string, object?>() }
        };
        await _storage.UpsertVectorsAsync("search_test", vectors);

        var queryVector = new float[] { 1.0f, 0.0f, 0.0f, 0.0f };

        // Act
        var results = (await _storage.SearchVectorsAsync("search_test", queryVector, limit: 3)).ToList();

        // Assert
        results.Should().NotBeEmpty();
        results.All(r => r.Score > 0).Should().BeTrue();
    }

    [Fact]
    public async Task SearchVectorsAsync_ShouldRespectLimit()
    {
        // Arrange
        await CreateTestCollection("limit_test", 4);

        var vectors = Enumerable.Range(1, 10).Select(i => new VectorRecord
        {
            VectorId = $"vec-{i}",
            SourceId = $"doc-{i}",
            Vectors = [i * 0.1f, 0.0f, 0.0f, 0.0f],
            Payload = new Dictionary<string, object?>()
        }).ToList();
        await _storage.UpsertVectorsAsync("limit_test", vectors);

        var queryVector = new float[] { 1.0f, 0.0f, 0.0f, 0.0f };

        // Act
        var results = (await _storage.SearchVectorsAsync("limit_test", queryVector, limit: 3)).ToList();

        // Assert
        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task SearchVectorsAsync_ShouldFilterByMinScore()
    {
        // Arrange
        await CreateTestCollection("minscore_test", 4);

        var vectors = new List<VectorRecord>
        {
            new() { VectorId = "similar", SourceId = "doc-1", Vectors = [0.9f, 0.1f, 0.0f, 0.0f], Payload = new Dictionary<string, object?>() },
            new() { VectorId = "different", SourceId = "doc-2", Vectors = [0.0f, 0.0f, 1.0f, 0.0f], Payload = new Dictionary<string, object?>() }
        };
        await _storage.UpsertVectorsAsync("minscore_test", vectors);

        var queryVector = new float[] { 1.0f, 0.0f, 0.0f, 0.0f };

        // Act
        var results = (await _storage.SearchVectorsAsync("minscore_test", queryVector, minScore: 0.7f)).ToList();

        // Assert
        // Only the similar vector should pass the minScore threshold
        results.All(r => r.Score >= 0.7f).Should().BeTrue();
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var dbPath = Path.Combine(Path.GetTempPath(), $"dispose_test_{Guid.NewGuid()}.db");
        var storage = new LocalVectorStorage(new LocalVectorConfig { DatabasePath = dbPath });

        // Act
        var act = () => storage.Dispose();

        // Assert
        act.Should().NotThrow();

        // Cleanup
        if (File.Exists(dbPath))
        {
            try { File.Delete(dbPath); }
            catch { }
        }
    }

    [Fact]
    public void Dispose_MultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var dbPath = Path.Combine(Path.GetTempPath(), $"multi_dispose_test_{Guid.NewGuid()}.db");
        var storage = new LocalVectorStorage(new LocalVectorConfig { DatabasePath = dbPath });

        // Act
        var act = () =>
        {
            storage.Dispose();
            storage.Dispose();
        };

        // Assert
        act.Should().NotThrow();

        // Cleanup
        if (File.Exists(dbPath))
        {
            try { File.Delete(dbPath); }
            catch { }
        }
    }

    #endregion

    private async Task CreateTestCollection(string name, int dimensions)
    {
        await _storage.CreateCollectionAsync(new VectorCollectionInfo
        {
            Name = name,
            Dimensions = dimensions,
            EmbeddingProvider = "test",
            EmbeddingModel = "test-model"
        });
    }
}

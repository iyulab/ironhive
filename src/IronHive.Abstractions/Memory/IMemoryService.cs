using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Storages;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// Interface for Hive Memory Service
/// </summary>
public interface IMemoryService
{
    /// <summary>
    /// Lists all vector collections in the storage.
    /// </summary>
    Task<IEnumerable<string>> ListCollectionsAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a vector collection exists in the storage.
    /// </summary>
    Task<bool> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new vector collection with the specified name and dimensions.
    /// </summary>
    Task CreateCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a vector collection by its name.
    /// </summary>
    Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds vector records in a collection based on the specified filter.
    /// </summary>
    Task<IEnumerable<VectorRecord>> FindVectorsAsync(
        string collectionName,
        string sourceId,
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates vector records in a collection.
    /// </summary>
    Task UpdateVectorContentAsync(
        string collectionName,
        string vectorId,
        object? content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes vector records from a collection by their source ID.
    /// </summary>
    Task DeleteVectorsAsync(
        string collectionName,
        string sourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules vectorization of a memory source with specified steps and options.
    /// </summary>
    Task ScheduleVectorizationAsync(
        string collectionName,
        IMemorySource source,
        IEnumerable<string> steps,
        IDictionary<string, object?>? handlerOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for vectors in a collection based on a query string and optional filters.
    /// </summary>
    Task<VectorSearchResult> SearchVectorsAsync(
        string collectionName,
        string query,
        float minScore = 0,
        int limit = 5,
        IEnumerable<string>? sourceIds = null,
        CancellationToken cancellationToken = default);
}

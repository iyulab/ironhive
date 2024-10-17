namespace Raggle.Abstractions.Memory;

public interface IDocumentStorage : IDisposable
{
    /// <summary>
    /// Get all collection names in the storage
    /// </summary>
    Task<IEnumerable<string>> GetAllCollectionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new collection, if it doesn't exist already
    /// </summary>
    /// <param name="collection">collection name</param>
    Task CreateCollectionAsync(
        string collection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a collection, if it exists
    /// </summary>
    /// <param name="collection">collection name</param>
    Task DeleteCollectionAsync(
        string collection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find document records in a collection
    /// </summary>
    /// <param name="collection"></param>
    Task<IEnumerable<DocumentRecord>> FindDocumentRecordsAsync(
        string collection,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Insert or update a document record
    /// </summary>
    Task UpsertDocumentRecordAsync(
        string collection,
        DocumentRecord document,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a document record by document ID
    /// </summary>
    Task DeleteDocumentRecordAsync(
        string collection,
        string documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all document file paths by document ID
    /// </summary>
    Task<IEnumerable<string>> GetDocumentFilesAsync(
        string collection,
        string documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create/Overwrite a file by document ID
    /// </summary>
    /// <param name="collection">collection name</param>
    /// <param name="documentId">Document ID</param>
    /// <param name="filePath">Full path of the file</param>
    /// <param name="Content">File content</param>
    /// <param name="overwrite">Overwrite if file already exists</param>
    Task WriteDocumentFileAsync(
        string collection,
        string documentId,
        string filePath,
        Stream Content,
        bool overwrite = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetch a file by document ID
    /// </summary>
    /// <param name="collection">collection name</param>
    /// <param name="documentId">Document ID</param>
    /// <param name="filePath">Full path of the file</param>
    /// <returns>File content</returns>
    Task<Stream> ReadDocumentFileAsync(
        string collection,
        string documentId,
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a file by document ID
    /// </summary>
    /// <param name="collection">collection name</param>
    /// <param name="documentId">Document ID</param>
    /// <param name="filePath">Full path of the file</param>
    /// <returns>File content</returns>
    Task DeleteDocumentFileAsync(
        string collection,
        string documentId,
        string filePath,
        CancellationToken cancellationToken = default);
}

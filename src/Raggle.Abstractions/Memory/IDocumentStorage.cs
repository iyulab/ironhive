namespace Raggle.Abstractions.Memories;

public interface IDocumentStorage : IDisposable
{
    /// <summary>
    /// Create a new container, if it doesn't exist already
    /// </summary>
    /// <param name="collection">collection name</param>
    Task CreateCollectionAsync(
        string collection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a container
    /// </summary>
    /// <param name="collection">collection name</param>
    Task DeleteCollectionAsync(
        string collection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create/Overwrite a file
    /// </summary>
    /// <param name="collection">collection name</param>
    /// <param name="documentId">Document ID</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="Content">File content</param>
    /// <param name="overwrite">Overwrite if file already exists</param>
    Task WriteFileAsync(
        string collection,
        string documentId,
        string fileName,
        Stream Content,
        bool overwrite = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetch a file from storage
    /// </summary>
    /// <param name="collection">collection name</param>
    /// <param name="documentId">Document ID</param>
    /// <param name="fileName"></param>
    /// <returns>File content</returns>
    Task<Stream> ReadFileAsync(
        string collection,
        string documentId,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a file from storage
    /// </summary>
    /// <param name="collection">collection name</param>
    /// <param name="documentId">Document ID</param>
    /// <param name="fileName"></param>
    /// <returns>File content</returns>
    Task DeleteFileAsync(
        string collection,
        string documentId,
        string fileName,
        CancellationToken cancellationToken = default);
}

namespace Raggle.Abstractions.Memory;

public interface IDocumentStorage : IDisposable
{
    Task<IEnumerable<string>> GetCollectionListAsync(
        CancellationToken cancellationToken = default);

    Task<bool> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    Task CreateCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GetDocumentFilesAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default);

    Task<bool> DocumentFileExistsAsync(
        string collectionName,
        string documentId,
        string filePath,
        CancellationToken cancellationToken = default);

    Task<Stream> ReadDocumentFileAsync(
        string collectionName,
        string documentId,
        string filePath,
        CancellationToken cancellationToken = default);

    Task WriteDocumentFileAsync(
        string collectionName,
        string documentId,
        string filePath,
        Stream content,
        bool overwrite = true,
        CancellationToken cancellationToken = default);

    Task DeleteDocumentFileAsync(
        string collectionName,
        string documentId,
        string filePath,
        CancellationToken cancellationToken = default);
}

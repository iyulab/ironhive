namespace Raggle.Core.Memory.Document;

/// <summary>
/// 지정된 형식의 문서를 읽고 쓰기 위한 공통 인터페이스입니다.
/// </summary>
public interface IDocumentManager
{
    IAsyncEnumerable<T> GetDocumentFilesAsync<T>(
        string collectionName,
        string documentId,
        string suffix,
        CancellationToken cancellationToken = default);

    Task<T> GetDocumentFileAsync<T>(
        string collectionName,
        string documentId,
        string suffix,
        CancellationToken cancellationToken = default);

    Task UpsertDocumentFilesAsync<T>(
        string collectionName,
        string documentId,
        string fileName,
        string suffix,
        IEnumerable<T> values,
        CancellationToken cancellationToken = default);

    Task UpsertDocumentFileAsync<T>(
        string collectionName,
        string documentId,
        string fileName,
        string suffix,
        T value,
        CancellationToken cancellationToken = default);
}

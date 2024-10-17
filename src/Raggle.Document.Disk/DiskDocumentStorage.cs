using Raggle.Abstractions.Memory;

namespace Raggle.Document.Disk;

public class DiskDocumentStorage : IDocumentStorage
{
    public string RootPath { get; }

    public DiskDocumentStorage(string rootPath)
    {
        RootPath = rootPath;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task<IEnumerable<string>> GetAllCollectionsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task CreateCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<DocumentRecord>> FindDocumentRecordsAsync(string collection, MemoryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DocumentDetail> GetDocumentDetailAsync(string collection, string documentId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpsertDocumentRecordAsync(string collection, DocumentRecord document, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteDocumentRecordAsync(string collection, string documentId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UploadFileAsync(string collection, string documentId, string filePath, Stream Content, bool overwrite = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> DownloadFileAsync(string collection, string documentId, string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFileAsync(string collection, string documentId, string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

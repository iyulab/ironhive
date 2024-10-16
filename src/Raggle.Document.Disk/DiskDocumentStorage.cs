using Raggle.Abstractions.Memories;

namespace Raggle.Document.Disk;

public class DiskDocumentStorage : IDocumentStorage
{
    public string RootPath { get; }

    public DiskDocumentStorage(string rootPath)
    {
        RootPath = rootPath;
    }

    public Task CreateCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFileAsync(string collection, string documentId, string fileName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> ReadFileAsync(string collection, string documentId, string fileName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task WriteFileAsync(string collection, string documentId, string fileName, Stream Content, bool overwrite = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

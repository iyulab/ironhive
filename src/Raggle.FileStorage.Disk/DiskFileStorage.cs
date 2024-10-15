using Raggle.Abstractions.Memories;

namespace Raggle.FileStorage.LocalDisk;

public class DiskFileStorage : IFileStorage
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public string TypeName { get; }

    public DiskFileStorage(string typeName)
    {
        TypeName = typeName;
    }

    public async Task<Stream> ReadFileAsync(string fileIdentifier, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync();

        try
        {
            var filePath = fileIdentifier;
            var fileStream = File.OpenRead(filePath);

            return fileStream;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}

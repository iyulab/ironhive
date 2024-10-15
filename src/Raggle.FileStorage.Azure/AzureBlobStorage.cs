namespace Raggle.FileStorage.Azure;

public class AzureBlobStorage
{
    public Task<Stream> DownloadAsync()
    {
        return Task.FromResult<Stream>(new MemoryStream());
    }
}

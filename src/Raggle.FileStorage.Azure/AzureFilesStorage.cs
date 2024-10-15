namespace Raggle.FileStorage.Azure;

public class AzureFilesStorage
{
    public Task<Stream> DownloadAsync()
    {
        return Task.FromResult<Stream>(new MemoryStream());
    }
}

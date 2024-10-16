namespace Raggle.Document.Azure;

public class AzureBlobDocumentStorage
{
    public Task<Stream> DownloadAsync()
    {
        return Task.FromResult<Stream>(new MemoryStream());
    }
}

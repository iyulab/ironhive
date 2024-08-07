using Azure.Storage.Blobs;

namespace Raggle.Abstractions.Sources;

public class AzureBlobSource
{
    private string _connectionString;
    private string _containerName;

    public IEnumerable<string> GetFileNames(string index)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        return blobContainerClient.GetBlobs().Select(blobItem => blobItem.Name);
    }

    public void UpsertFile(string filename, Stream content)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = blobContainerClient.GetBlobClient(filename);
        blobClient.Upload(content, true);
    }

    public void DeleteFile(string filename)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = blobContainerClient.GetBlobClient(filename);
        blobClient.DeleteIfExists();
    }
}

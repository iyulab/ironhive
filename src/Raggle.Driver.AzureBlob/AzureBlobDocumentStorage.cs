using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Raggle.Abstractions.Memory;
using System.Runtime.CompilerServices;

namespace Raggle.Driver.AzureBlob;

public class AzureBlobDocumentStorage : IDocumentStorage
{
    private readonly BlobServiceClient _client;    

    public AzureBlobDocumentStorage(AzureBlobConfig config, BlobClientOptions? options = null)
    {
        _client = GetBlobServiceClient(config, options);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetCollectionListAsync(
        CancellationToken cancellationToken = default)
    {
        var collections = new List<string>();
        await foreach (BlobContainerItem container in _client.GetBlobContainersAsync(cancellationToken: cancellationToken))
        {
            collections.Add(container.Name);
        }
        return collections;
    }

    /// <inheritdoc />
    public async Task<bool> CollectionExistsAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        return await container.ExistsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        await container.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> GetDocumentFilesAsync(
        string collectionName, 
        string documentId, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        string prefix = $"{documentId}/";

        await foreach (BlobItem blob in container.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
        {
            var relativePath = blob.Name.Substring(prefix.Length);
            yield return relativePath;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DocumentFileExistsAsync(
        string collectionName, 
        string documentId, 
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        var blobName = Path.Combine(documentId, filePath).Replace("\\", "/");
        var blobClient = container.GetBlobClient(blobName);
        return await blobClient.ExistsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task WriteDocumentFileAsync(
        string collectionName, 
        string documentId, 
        string filePath, 
        Stream data, 
        bool overwrite = true, 
        CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        var blobName = Path.Combine(documentId, filePath).Replace("\\", "/");
        var blobClient = container.GetBlobClient(blobName);

        if (!overwrite && await blobClient.ExistsAsync(cancellationToken))
            throw new IOException($"파일이 이미 존재합니다: {blobName}");

        // 파일 업로드 또는 덮어쓰기
        await blobClient.UploadAsync(data, overwrite, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Stream> ReadDocumentFileAsync(
        string collectionName, 
        string documentId, 
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        var blobName = Path.Combine(documentId, filePath).Replace("\\", "/");
        var blobClient = container.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync(cancellationToken))
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {filePath}");

        var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <inheritdoc />
    public async Task DeleteDocumentFileAsync(
        string collectionName, 
        string documentId, 
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(collectionName);
        var blobName = Path.Combine(documentId, filePath).Replace("\\", "/");
        var blobClient = container.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    #region Private Methods

    // BlobServiceClient 생성
    private static BlobServiceClient GetBlobServiceClient(AzureBlobConfig config, BlobClientOptions? options)
    {
        var client = config.AuthType switch
        {
            AzureBlobAuthTypes.ConnectionString => new BlobServiceClient(config.ConnectionString, options),
            AzureBlobAuthTypes.AccountKey => new BlobServiceClient(GetBlobStorageUri(config), GetSharedKeyCredential(config), options),
            AzureBlobAuthTypes.SASToken => new BlobServiceClient(GetBlobStorageUri(config), GetSasTokenCredential(config), options),
            AzureBlobAuthTypes.AzureIdentity => new BlobServiceClient(GetBlobStorageUri(config), config.TokenCredential, options),
            _ => throw new ArgumentOutOfRangeException(nameof(config.AuthType), "알 수 없는 Azure Blob 인증 유형입니다.")
        };
        return client;
    }

    // AccountName을 이용한 Uri 생성
    private static Uri GetBlobStorageUri(AzureBlobConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.AccountName))
            throw new ArgumentException("AccountName은 비어 있을 수 없습니다.", nameof(config.AccountName));
        return new Uri($"https://{config.AccountName}.blob.core.windows.net");
    }

    // AccountKey를 이용한 인증 방식
    private static StorageSharedKeyCredential GetSharedKeyCredential(AzureBlobConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.AccountName))
            throw new ArgumentException("AccountName은 비어 있을 수 없습니다.", nameof(config.AccountName));
        return new StorageSharedKeyCredential(config.AccountName, config.AccountKey);
    }

    // SAS Token을 이용한 인증 방식
    private static AzureSasCredential GetSasTokenCredential(AzureBlobConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.SASToken))
            throw new ArgumentException("SASToken은 비어 있을 수 없습니다.", nameof(config.SASToken));
        return new AzureSasCredential(config.SASToken);
    }

    #endregion
}

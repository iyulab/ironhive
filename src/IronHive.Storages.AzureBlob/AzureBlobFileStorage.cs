using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using IronHive.Abstractions.Memory;

namespace IronHive.Storages.AzureBlob;

public class AzureBlobFileStorage : IFileStorage
{
    private readonly BlobContainerClient _client;

    public AzureBlobFileStorage(AzureBlobConfig config)
    {
        _client = CreateClient(config);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> ListAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        var files = new List<string>();
        await foreach (var item in _client.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
        {
            files.Add(item.Name);
        }
        return files;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        string path, 
        CancellationToken cancellationToken = default)
    {
        var blob = _client.GetBlobClient(path);
        return await blob.ExistsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Stream> ReadAsync(
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        var blob = _client.GetBlobClient(filePath);

        if (!await blob.ExistsAsync(cancellationToken))
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {filePath}");

        var memoryStream = new MemoryStream();
        await blob.DownloadToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <inheritdoc />
    public async Task WriteAsync(
        string filePath, 
        Stream data, 
        bool overwrite = true, 
        CancellationToken cancellationToken = default)
    {
        var blob = _client.GetBlobClient(filePath);

        if (!overwrite && await blob.ExistsAsync(cancellationToken))
            throw new IOException($"파일이 이미 존재합니다: {blob}");

        // 파일 업로드 또는 덮어쓰기
        await blob.UploadAsync(data, overwrite, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string path, 
        CancellationToken cancellationToken = default)
    {
        var blob = _client.GetBlobClient(path);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    #region Private Methods

    // BlobContainerClient 생성
    private static BlobContainerClient CreateClient(AzureBlobConfig config)
    {
        var options = new BlobClientOptions();
        var client = config.AuthType switch
        {
            AzureBlobAuthTypes.ConnectionString => new BlobServiceClient(config.ConnectionString, options),
            AzureBlobAuthTypes.AccountKey => new BlobServiceClient(GetBlobStorageUri(config), GetSharedKeyCredential(config), options),
            AzureBlobAuthTypes.SASToken => new BlobServiceClient(GetBlobStorageUri(config), GetSasTokenCredential(config), options),
            AzureBlobAuthTypes.AzureIdentity => new BlobServiceClient(GetBlobStorageUri(config), config.TokenCredential, options),
            _ => throw new ArgumentOutOfRangeException(nameof(config.AuthType), "알 수 없는 Azure Blob 인증 유형입니다.")
        };
        if (string.IsNullOrWhiteSpace(config.ContainerName))
            throw new ArgumentException("ContainerName은 비어 있을 수 없습니다.", nameof(config.ContainerName));

        return client.GetBlobContainerClient(config.ContainerName);
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

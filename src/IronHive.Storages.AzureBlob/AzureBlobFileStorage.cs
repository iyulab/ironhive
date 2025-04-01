using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using IronHive.Abstractions.Files;

namespace IronHive.Storages.AzureBlob;

public class AzureBlobFileStorage : IFileStorage
{
    private readonly BlobContainerClient _client;
    
    public string ContainerName { get; init; }

    public AzureBlobFileStorage(AzureBlobConfig config)
    {
        ContainerName = config.ContainerName;
        _client = CreateClient(config);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> ListAsync(
        string? prefix = null,
        int depth = 1,
        CancellationToken cancellationToken = default)
    {
        prefix ??= string.Empty;
        var result = new List<string>();

        // prefix ~ delimiter 사이의 객체만 가져옵니다.
        await foreach (var item in _client.GetBlobsByHierarchyAsync(
            delimiter: "/",
            prefix: prefix,
            cancellationToken: cancellationToken))
        {
            if (item.IsPrefix)
            {
                if (depth == 1)
                {
                    // depth가 1인 경우는 현재 폴더만 추가
                    result.Add(item.Prefix);
                }
                else
                {
                    // depth가 0 이하이면 무제한, 그렇지 않으면 depth 감소
                    int nextDepth = depth > 1 ? depth - 1 : depth;
                    var subItems = await ListAsync(item.Prefix, nextDepth, cancellationToken);
                    result.AddRange(subItems);
                }
            }
            else if (item.Blob != null)
            {
                // 파일 이름 추가
                result.Add(item.Blob.Name);
            }
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        string path, 
        CancellationToken cancellationToken = default)
    {
        if (IsDirectory(path))
        {
            // 디렉토리 경로인 경우 해당 prefix로 시작하는 blob이 하나라도 있으면 true 반환
            await foreach (var _ in _client.GetBlobsAsync(prefix: path, cancellationToken: cancellationToken))
            {
                // 하나라도 발견되면 디렉토리가 존재하는 것으로 판단
                return true;
            }
            return false;
        }
        else
        {
            var blob = _client.GetBlobClient(path);
            var response = await blob.ExistsAsync(cancellationToken).ConfigureAwait(false);
            return response.Value;
        }
    }

    /// <inheritdoc />
    public async Task<Stream> ReadFileAsync(
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        if (IsDirectory(filePath))
            throw new ArgumentException("디렉터리 경로로 파일을 읽을 수 없습니다.", nameof(filePath));
        if (!await ExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {filePath}");

        var blob = _client.GetBlobClient(filePath);
        var memoryStream = new MemoryStream();
        await blob.DownloadToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <inheritdoc />
    public async Task WriteFileAsync(
        string filePath,
        Stream data,
        bool overwrite = true,
        CancellationToken cancellationToken = default)
    {
        if (IsDirectory(filePath))
            throw new ArgumentException("디렉터리 경로로 파일을 쓸 수 없습니다.", nameof(filePath));
        if (!overwrite && await ExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
            throw new IOException($"파일이 이미 존재합니다: {filePath}");

        // 파일 업로드 또는 덮어쓰기
        var blob = _client.GetBlobClient(filePath);
        await blob.UploadAsync(data, overwrite, cancellationToken).ConfigureAwait(false);
        data.Position = 0;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string path, 
        CancellationToken cancellationToken = default)
    {
        if (IsDirectory(path))
        {
            // 디렉터리 경로인 경우, 해당 prefix로 시작하는 모든 blob 삭제
            await foreach (var blobItem in _client.GetBlobsAsync(prefix: path, cancellationToken: cancellationToken))
            {
                var blobClient = _client.GetBlobClient(blobItem.Name);
                await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            var blob = _client.GetBlobClient(path);
            await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    // azure blob에서 경로끝에 '/'가 있으면 디렉토리로 간주합니다.
    private static bool IsDirectory(string path)
    {
        return path.EndsWith('/');
    }

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
}

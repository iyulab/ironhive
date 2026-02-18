using IronHive.Abstractions.Files;
using Azure;
using Azure.Storage;
using Azure.Storage.Files.Shares;

namespace IronHive.Storages.Azure;

/// <summary>
/// Azure Files Storage를 사용하여 파일 스토리지를 구현한 클래스입니다.
/// </summary>
public class AzureFileShareStorage : IFileStorage
{
    private readonly ShareClient _client;
    private readonly AzureStorageConfig _config;

    public AzureFileShareStorage(AzureStorageConfig config)
    {
        _config = config;
        _client = CreateClient(config);
    }

    /// <inheritdoc />
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

        // 디렉터리 경로에서 '/'를 제거하여 정리
        var directoryPath = prefix.TrimEnd('/');
        
        ShareDirectoryClient directoryClient = string.IsNullOrEmpty(directoryPath) 
            ? _client.GetRootDirectoryClient() 
            : _client.GetDirectoryClient(directoryPath);

        await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync(cancellationToken: cancellationToken))
        {
            var itemPath = string.IsNullOrEmpty(directoryPath) 
                ? item.Name 
                : $"{directoryPath}/{item.Name}";

            if (item.IsDirectory)
            {
                var dirPath = itemPath + "/";
                if (depth == 1)
                {
                    // depth가 1인 경우는 현재 폴더만 추가
                    result.Add(dirPath);
                }
                else
                {
                    // depth가 0 이하이면 무제한, 그렇지 않으면 depth 감소
                    int nextDepth = depth > 1 ? depth - 1 : depth;
                    var subItems = await ListAsync(dirPath, nextDepth, cancellationToken);
                    result.AddRange(subItems);
                }
            }
            else
            {
                // 파일 경로 추가
                result.Add(itemPath);
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsFileAsync(
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        if (IsDirectory(filePath))
            throw new ArgumentException("디렉터리 경로로 파일 존재 여부를 확인할 수 없습니다.", nameof(filePath));
        
        var fileClient = GetFileClient(filePath);
        var response = await fileClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
        return response.Value;
    }

    /// <inheritdoc />
    public async Task<Stream> ReadFileAsync(
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        if (!await ExistsFileAsync(filePath, cancellationToken))
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {filePath}");

        var fileClient = GetFileClient(filePath);
        var download = await fileClient.DownloadAsync(cancellationToken: cancellationToken);
        
        var memoryStream = new MemoryStream();
        await download.Value.Content.CopyToAsync(memoryStream, cancellationToken);
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
        if (!overwrite && await ExistsFileAsync(filePath, cancellationToken).ConfigureAwait(false))
            throw new IOException($"파일이 이미 존재합니다: {filePath}");

        // 상위 디렉터리 생성
        await EnsureDirectoryExistsAsync(filePath, cancellationToken).ConfigureAwait(false);

        var fileClient = GetFileClient(filePath);
        
        // 파일 생성 및 업로드
        await fileClient.CreateAsync(data.Length, cancellationToken: cancellationToken).ConfigureAwait(false);
        await fileClient.UploadAsync(data, cancellationToken: cancellationToken).ConfigureAwait(false);
        data.Position = 0;
    }

    /// <inheritdoc />
    public async Task DeleteFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (IsDirectory(filePath))
            throw new ArgumentException("디렉터리 경로로 파일을 삭제할 수 없습니다.", nameof(filePath));

        var fileClient = GetFileClient(filePath);
        await fileClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteDirectoryAsync(
        string directoryPath, 
        CancellationToken cancellationToken = default)
    {
        if (!IsDirectory(directoryPath))
            throw new ArgumentException("디렉터리 경로는 '/'로 끝나야 합니다.", nameof(directoryPath));

        var cleanPath = directoryPath.TrimEnd('/');
        var directoryClient = _client.GetDirectoryClient(cleanPath);
        
        await DeleteDirectoryRecursiveAsync(directoryClient, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 디렉터리를 재귀적으로 삭제합니다.
    /// </summary>
    private static async Task DeleteDirectoryRecursiveAsync(
        ShareDirectoryClient directoryClient,
        CancellationToken cancellationToken)
    {
        await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync(cancellationToken: cancellationToken))
        {
            if (item.IsDirectory)
            {
                var subDirectoryClient = directoryClient.GetSubdirectoryClient(item.Name);
                await DeleteDirectoryRecursiveAsync(subDirectoryClient, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var fileClient = directoryClient.GetFileClient(item.Name);
                await fileClient.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        await directoryClient.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 파일 경로의 상위 디렉터리가 존재하는지 확인하고, 없으면 생성합니다.
    /// </summary>
    private async Task EnsureDirectoryExistsAsync(string filePath, CancellationToken cancellationToken)
    {
        var directoryPath = Path.GetDirectoryName(filePath)?.Replace('\\', '/');
        if (string.IsNullOrEmpty(directoryPath))
            return;

        var pathParts = directoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var currentPath = string.Empty;

        foreach (var part in pathParts)
        {
            currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";
            var directoryClient = _client.GetDirectoryClient(currentPath);
            await directoryClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 파일 경로에서 ShareFileClient를 가져옵니다.
    /// </summary>
    private ShareFileClient GetFileClient(string filePath)
    {
        var normalizedPath = filePath.Replace('\\', '/').TrimStart('/');
        return _client.GetRootDirectoryClient().GetFileClient(normalizedPath);
    }

    /// <summary>
    /// Azure Files에서 경로 끝에 '/'가 있으면 디렉터리로 간주합니다.
    /// </summary>
    private static bool IsDirectory(string path)
    {
        return path.EndsWith('/');
    }

    /// <summary>
    /// ShareClient 생성
    /// </summary>
    private static ShareClient CreateClient(AzureStorageConfig config)
    {
        var options = new ShareClientOptions();
        var client = config.AuthType switch
        {
            AzureStorageAuthTypes.ConnectionString => new ShareServiceClient(config.ConnectionString, options),
            AzureStorageAuthTypes.AccountKey => new ShareServiceClient(GetFileStorageUri(config), GetSharedKeyCredential(config), options),
            AzureStorageAuthTypes.SASToken => new ShareServiceClient(GetFileStorageUri(config), GetSasTokenCredential(config), options),
            AzureStorageAuthTypes.AzureIdentity => new ShareServiceClient(GetFileStorageUri(config), config.TokenCredential, options),
            _ => throw new ArgumentOutOfRangeException(nameof(config), "알 수 없는 Azure Storage 인증 유형입니다.")
        };
        
        if (string.IsNullOrWhiteSpace(config.StorageName))
            throw new ArgumentException("ShareName은 비어 있을 수 없습니다.", nameof(config));

        var share = client.GetShares(prefix: config.StorageName)
                          .FirstOrDefault(s => s.Name == config.StorageName);

        if (share == null)
            return client.CreateShare(config.StorageName).Value;
        else
            return client.GetShareClient(config.StorageName);
    }

    /// <summary>
    /// AccountName을 이용한 Uri 생성
    /// </summary>
    private static Uri GetFileStorageUri(AzureStorageConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.AccountName))
            throw new ArgumentException("AccountName은 비어 있을 수 없습니다.", nameof(config));
        return new Uri($"https://{config.AccountName}.file.core.windows.net");
    }

    /// <summary>
    /// AccountKey를 이용한 인증 방식
    /// </summary>
    private static StorageSharedKeyCredential GetSharedKeyCredential(AzureStorageConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.AccountName))
            throw new ArgumentException("AccountName은 비어 있을 수 없습니다.", nameof(config));
        return new StorageSharedKeyCredential(config.AccountName, config.AccountKey);
    }

    /// <summary>
    /// SAS Token을 이용한 인증 방식
    /// </summary>
    private static AzureSasCredential GetSasTokenCredential(AzureStorageConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.SASToken))
            throw new ArgumentException("SASToken은 비어 있을 수 없습니다.", nameof(config));
        return new AzureSasCredential(config.SASToken);
    }
}

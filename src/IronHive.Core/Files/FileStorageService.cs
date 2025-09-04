using IronHive.Abstractions.Files;
using IronHive.Abstractions.Registries;

namespace IronHive.Core.Files;

/// <inheritdoc />
public class FileStorageService : IFileStorageService
{
    private readonly IStorageRegistry _storages;

    public FileStorageService(IStorageRegistry storages)
    {
        _storages = storages;
    }

    /// <inheritdoc />
    public Task<IEnumerable<string>> ListAsync(
        string storageName, 
        string? prefix = null, 
        int depth = 1, 
        CancellationToken cancellationToken = default)
        => Get(storageName).ListAsync(prefix, depth, cancellationToken);

    /// <inheritdoc />
    public Task<bool> ExistsFileAsync(
        string storageName, 
        string filePath, 
        CancellationToken cancellationToken = default)
        => Get(storageName).ExistsFileAsync(filePath, cancellationToken);

    /// <inheritdoc />
    public Task<Stream> ReadFileAsync(
        string storageName, 
        string filePath, 
        CancellationToken cancellationToken = default)
        => Get(storageName).ReadFileAsync(filePath, cancellationToken);

    /// <inheritdoc />
    public Task WriteFileAsync(
        string storageName, 
        string filePath, 
        Stream data, 
        bool overwrite = true, 
        CancellationToken cancellationToken = default)
        => Get(storageName).WriteFileAsync(filePath, data, overwrite, cancellationToken);

    /// <inheritdoc />
    public Task DeleteFileAsync(
        string storageName,
        string filePath,
        CancellationToken cancellationToken = default)
        => Get(storageName).DeleteFileAsync(filePath, cancellationToken);

    /// <inheritdoc />
    public Task DeleteDirectoryAsync(
        string storageName, 
        string directoryPath, 
        CancellationToken cancellationToken = default)
        => Get(storageName).DeleteDirectoryAsync(directoryPath, cancellationToken);

    /// <summary>
    /// 지정한 이름의 스토리지를 반환합니다.
    /// </summary>
    private IFileStorage Get(string storageName)
        => _storages.TryGet<IFileStorage>(storageName, out var storage)
            ? storage
            : throw new ArgumentException($"Storage not found: {storageName}", nameof(storageName));
}

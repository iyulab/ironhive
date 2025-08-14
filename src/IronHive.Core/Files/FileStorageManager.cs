using IronHive.Abstractions;
using IronHive.Abstractions.Files;

namespace IronHive.Core.Files;

/// <inheritdoc />
public class FileStorageManager : IFileStorageManager
{
    public FileStorageManager()
        : this(Enumerable.Empty<IFileStorage>())
    { }

    public FileStorageManager(IEnumerable<IFileStorage> storages)
    {
        Storages = new KeyedCollection<IFileStorage>(
            storages,
            storage => storage.StorageName);
    }

    /// <inheritdoc />
    public IKeyedCollection<IFileStorage> Storages { get; }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> ListAsync(
        string storage,
        string? prefix = null,
        int depth = 1,
        CancellationToken cancellationToken = default)
    {
        if (!Storages.TryGet(storage, out var service))
            throw new ArgumentException($"저장소 '{storage}'을(를) 찾을 수 없습니다.", nameof(storage));
        
        var result = await service.ListAsync(prefix, depth, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        string storage,
        string path,
        CancellationToken cancellationToken = default)
    {
        if (!Storages.TryGet(storage, out var service))
            throw new ArgumentException($"저장소 '{storage}'을(를) 찾을 수 없습니다.", nameof(storage));

        var result = await service.ExistsAsync(path, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task<Stream> ReadFileAsync(
        string storage,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (!Storages.TryGet(storage, out var service))
            throw new ArgumentException($"저장소 '{storage}'을(를) 찾을 수 없습니다.", nameof(storage));

        var result = await service.ReadFileAsync(filePath, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task WriteFileAsync(
        string storage,
        string filePath,
        Stream data,
        bool overwrite = true,
        CancellationToken cancellationToken = default)
    {
        if (!Storages.TryGet(storage, out var service))
            throw new ArgumentException($"저장소 '{storage}'을(를) 찾을 수 없습니다.", nameof(storage));

        await service.WriteFileAsync(filePath, data, overwrite, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string storage,
        string path,
        CancellationToken cancellationToken = default)
    {
        if (!Storages.TryGet(storage, out var service))
            throw new ArgumentException($"저장소 '{storage}'을(를) 찾을 수 없습니다.", nameof(storage));

        await service.DeleteAsync(path, cancellationToken);
    }
}

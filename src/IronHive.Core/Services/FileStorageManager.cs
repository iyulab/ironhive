using IronHive.Abstractions;
using IronHive.Abstractions.Files;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core.Services;

public class FileStorageManager : IFileStorageManager
{
    private readonly IServiceProvider _services;
    private readonly IReadOnlyDictionary<string, Func<IServiceProvider, object?, IFileStorage>> _factories;

    public FileStorageManager(IServiceProvider services)
    {
        _services = services;
        _factories = services.GetRequiredService<IHiveServiceStore>().GetFactories<IFileStorage>();
    }

    /// <inheritdoc />
    public IFileStorage CreateFileStorage(string provider, object? providerConfig = null)
    {
        if (_factories.TryGetValue(provider, out var factory))
            return factory(_services, providerConfig);
        else
            throw new InvalidOperationException($"Unknown file storage provider: {provider}");
    }

    /// <inheritdoc />
    public Task<IEnumerable<string>> ListAsync(
        string provider,
        string? prefix = null,
        int depth = 1,
        object? providerConfig = null,
        CancellationToken cancellationToken = default)
        => CreateFileStorage(provider, providerConfig).ListAsync(prefix, depth, cancellationToken);

    /// <inheritdoc />
    public Task<bool> ExistsAsync(
        string provider,
        string path,
        object? providerConfig = null,
        CancellationToken cancellationToken = default)
        => CreateFileStorage(provider, providerConfig).ExistsAsync(path, cancellationToken);

    /// <inheritdoc />
    public Task<Stream> ReadFileAsync(
        string provider,
        string filePath,
        object? providerConfig = null,
        CancellationToken cancellationToken = default)
        => CreateFileStorage(provider, providerConfig).ReadFileAsync(filePath, cancellationToken);

    /// <inheritdoc />
    public Task WriteFileAsync(
        string provider,
        string filePath,
        Stream data,
        bool overwrite = true,
        object? providerConfig = null,
        CancellationToken cancellationToken = default)
        => CreateFileStorage(provider, providerConfig).WriteFileAsync(filePath, data, overwrite, cancellationToken);

    /// <inheritdoc />
    public Task DeleteAsync(
        string provider,
        string path,
        object? providerConfig = null,
        CancellationToken cancellationToken = default)
        => CreateFileStorage(provider, providerConfig).DeleteAsync(path, cancellationToken);
}

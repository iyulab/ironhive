using IronHive.Abstractions;
using IronHive.Abstractions.Files;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core.Services;

public class FileStorageManager : IFileStorageManager
{
    private readonly IServiceProvider _services;
    private readonly IReadOnlyDictionary<string, Func<IServiceProvider, object?, IFileStorage>> _factories;
    private readonly IFileDecoderResolver _resolver;

    public FileStorageManager(IServiceProvider services)
    {
        _services = services;
        _factories = services.GetRequiredService<IHiveServiceStore>().GetFactories<IFileStorage>();
        _resolver = services.GetRequiredService<IFileDecoderResolver>();
    }

    /// <inheritdoc />
    public IEnumerable<string> GetDecodableExtensions()
        => _resolver.GetSupportedExtensions();

    /// <inheritdoc />
    public string? GetMimeType(string fileName)
        => _resolver.GetMimeType(fileName);

    /// <inheritdoc />
    public async Task<string> DecodeFileAsync(
        string provider,
        string filePath,
        object? providerConfig = null,
        CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(filePath);
        if (_resolver.TryGetDecoderByName(fileName, out var decoder))
        {
            await using var stream = await ReadFileAsync(provider, filePath, providerConfig, cancellationToken);
            return await decoder.DecodeAsync(stream, cancellationToken);
        }
        else
        {
            throw new NotSupportedException($"Unsupported file {fileName}");
        }
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

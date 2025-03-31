using IronHive.Abstractions;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;
using Microsoft.AspNetCore.StaticFiles;

namespace IronHive.Core.Files;

public class FileStorageManager : IFileStorageManager
{
    private readonly FileExtensionContentTypeProvider _detector = new();
    private readonly IServiceProvider _provider;
    private readonly IReadOnlyDictionary<string, Func<IServiceProvider, object?, IFileStorage>> _factories;
    private readonly IEnumerable<IFileDecoder> _decoders;

    public FileStorageManager(IServiceProvider provider,IHiveServiceStore store, IEnumerable<IFileDecoder> decoders)
    {
        _provider = provider;
        _factories = store.GetFactories<IFileStorage>();
        _decoders = decoders;
    }

    /// <inheritdoc />
    public IFileStorage CreateStorage(string provider, object? config)
    {
        if (_factories.TryGetValue(provider, out var factory))
        {
            return factory(_provider, config);
        }
        else
        {
            throw new InvalidOperationException($"dont know {provider}");
        }
    }

    /// <inheritdoc />
    public string GetMimeType(
        string fileName, 
        CancellationToken cancellationToken = default)
    {
        if (_detector.TryGetContentType(fileName, out var type))
        {
            return type;
        }
        else
        {
            throw new NotSupportedException("Unsupported file type.");
        }
    }

    /// <inheritdoc />
    public async Task<string> DecodeAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        if (_detector.TryGetContentType(fileName, out var type))
        {
            var decoder = _decoders.FirstOrDefault(x => x.SupportsMimeType(type));
            if (decoder is not null)
            {
                return await decoder.DecodeAsync(data, cancellationToken);
            }
            else
            {
                throw new NotSupportedException("Unsupported file type.");
            }
        }
        else
        {
            var text = new StreamReader(data).ReadToEnd();
            return text;
        }
    }

    /// <inheritdoc />
    public async Task<string> DecodeAsync(
        FileMemorySource source, 
        CancellationToken cancellationToken = default)
    {
        var storage = CreateStorage(source.Provider, source.ProviderConfig);
        var data = await storage.ReadAsync(source.FilePath, cancellationToken);
        return await DecodeAsync(source.FilePath, data, cancellationToken);
    }
}

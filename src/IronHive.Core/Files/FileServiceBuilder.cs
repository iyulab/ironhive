using IronHive.Abstractions.Files;
using IronHive.Abstractions.Storages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IronHive.Core.Files;

/// <inheritdoc />
public class FileServiceBuilder : IFileServiceBuilder
{
    private readonly IServiceCollection _services;
    private readonly HashSet<IFileDecoder> _decoders = new(EqualityComparer<IFileDecoder>.Default);
    private readonly KeyedCollection<IFileStorage> _storages = new(storage => storage.StorageName);

    public FileServiceBuilder(IServiceCollection? services = null)
    {
        _services = services ?? new ServiceCollection();
        _services.TryAddSingleton<IFileService>((sp) =>
        {
            return new FileService(_storages, _decoders);
        });
    }

    /// <inheritdoc />
    public IFileServiceBuilder AddDecoder(IFileDecoder decoder)
    {
        _decoders.Add(decoder);
        return this;
    }

    /// <inheritdoc />
    public IFileServiceBuilder AddStorage(IFileStorage storage)
    {
        _storages.Add(storage);
        return this;
    }

    /// <inheritdoc />
    public IFileService Build()
    {
        var provider = _services.BuildServiceProvider();
        return provider.GetRequiredService<IFileService>();
    }
}

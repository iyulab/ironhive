using IronHive.Abstractions;
using IronHive.Abstractions.Files;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IronHive.Core.Files;

/// <inheritdoc />
public class FileServiceBuilder : IFileServiceBuilder
{
    private readonly IServiceCollection _services;
    private readonly HashSet<IFileDecoder> _decoders = new(EqualityComparer<IFileDecoder>.Default);
    
    public FileServiceBuilder(IServiceCollection? services = null)
    {
        _services = services ?? new ServiceCollection();
        _services.TryAddSingleton<IFileService>((sp) =>
        {
            var storages = sp.GetRequiredService<IKeyedCollectionGroup<IKeyedStorage>>();
            return new FileService(storages, _decoders);
        });
    }

    /// <inheritdoc />
    public IFileServiceBuilder AddDecoder(IFileDecoder decoder)
    {
        _decoders.Add(decoder);
        return this;
    }

    /// <inheritdoc />
    public IFileService Build()
    {
        var provider = _services.BuildServiceProvider();
        return provider.GetRequiredService<IFileService>();
    }
}

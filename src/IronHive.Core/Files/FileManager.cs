using IronHive.Abstractions.Files;
using Microsoft.AspNetCore.StaticFiles;

namespace IronHive.Core.Files;

public class FileManager : IFileManager
{
    private readonly FileExtensionContentTypeProvider _provider = new();
    private readonly IEnumerable<IFileDecoder> _decoders;

    public FileManager(IEnumerable<IFileDecoder> decoders)
    {
        _decoders = decoders;
    }

    /// <inheritdoc />
    public Task<string> DecodeAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        if (_provider.TryGetContentType(fileName, out var type))
        {
            var decoder = _decoders.FirstOrDefault(x => x.SupportsMimeType(type));
            if (decoder is not null)
            {
                return decoder.DecodeAsync(data, cancellationToken);
            }
        }

        throw new NotSupportedException($"The file type of '{fileName}' is not supported.");
    }
}

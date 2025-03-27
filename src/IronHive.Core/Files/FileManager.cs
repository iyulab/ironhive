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
    public async Task<string> DecodeAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        if (_provider.TryGetContentType(fileName, out var type))
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
}

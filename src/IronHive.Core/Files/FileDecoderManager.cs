using IronHive.Abstractions.Files;

namespace IronHive.Core.Files;

/// <inheritdoc />
public class FileDecoderManager : IFileDecoderManager
{
    private readonly IDictionary<string, string> _mapper = new FileContentTypeMapper();
    private readonly IEnumerable<IFileDecoder> _decoders;

    public FileDecoderManager(IEnumerable<IFileDecoder> decoders)
    {
        _decoders = decoders;
    }

    /// <inheritdoc />
    public IEnumerable<string> SupportedExtensions
    {
        get
        {
            return _mapper.Where(kvp => _decoders.Any(d => d.SupportsMimeType(kvp.Value)))
                .Select(kvp => kvp.Key)
                .Distinct();
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> SupportedMimeTypes
    {
        get
        {
            return _mapper.Where(kvp => _decoders.Any(d => d.SupportsMimeType(kvp.Value)))
                .Select(kvp => kvp.Value)
                .Distinct();
        }
    }

    /// <inheritdoc />
    public async Task<string> DecodeAsync(
        string filePath, 
        Stream data, 
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(filePath);
        if (_mapper.TryGetValue(extension, out var mimeType))
        {
            var decoder = _decoders.FirstOrDefault(d => d.SupportsMimeType(mimeType))
                ?? throw new NotSupportedException($"Decoder not found for MIME type: {mimeType}");

            return await decoder.DecodeAsync(data, cancellationToken);
        }
        else
        {
            throw new NotSupportedException($"Not supported file extension: {extension}");
        }
    }
}

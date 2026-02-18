using IronHive.Abstractions.Files;

namespace IronHive.Core.Files;

/// <inheritdoc />
public class FileExtractionService<T> : IFileExtractionService<T>
{
    private readonly IFileMediaTypeDetector _detector;
    private readonly IReadOnlyList<IFileDecoder<T>> _decoders;
    private readonly Lazy<IReadOnlyList<string>> _supportedExts;
    private readonly Lazy<IReadOnlyList<string>> _supportedMimes;

    public FileExtractionService(
        IFileMediaTypeDetector detector,
        IEnumerable<IFileDecoder<T>>? decoders = null)
    {
        _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        _decoders = (decoders ?? Enumerable.Empty<IFileDecoder<T>>()).ToList();

        _supportedExts = new(() => _detector.Extensions
                .Where(ext => _detector.TryDetect(ext, out var mime) && _decoders.Any(d => d.SupportsMimeType(mime)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray());

        _supportedMimes = new(() => _detector.MediaTypes
                .Where(type => _decoders.Any(d => d.SupportsMimeType(type)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray());
    }

    /// <inheritdoc />
    public IEnumerable<string> SupportedExtensions => _supportedExts.Value;

    /// <inheritdoc />
    public IEnumerable<string> SupportedMimeTypes => _supportedMimes.Value;

    /// <inheritdoc />
    public async Task<T> DecodeAsync(
        string fileName, 
        Stream data, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Empty file name.", nameof(fileName));

        if (data.CanSeek) data.Position = 0;

        if (!_detector.TryDetect(fileName, out var mime) || string.IsNullOrWhiteSpace(mime))
            throw new NotSupportedException($"Unknown content type for '{fileName}'.");

        var decoder = _decoders.Where(d => d.SupportsMimeType(mime)).FirstOrDefault()
            ?? throw new NotSupportedException($"No decoder for '{mime}' (file: '{fileName}').");

        return await decoder.DecodeAsync(data, cancellationToken).ConfigureAwait(false);
    }
}

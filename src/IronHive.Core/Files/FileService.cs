using IronHive.Abstractions.Collections;
using IronHive.Abstractions.Files;
using System.Text.RegularExpressions;

namespace IronHive.Core.Files;

/// <inheritdoc />
public partial class FileService : IFileService
{
    private readonly IStorageCollection _storages;
    // 파일 확장자와 MIME 타입 매핑 객체
    private readonly FileContentTypeMapper _mapper = new();

    public FileService(IStorageCollection storages, IEnumerable<IFileDecoder>? decoders = null)
    {
        _storages = storages;
        Decoders = decoders?.ToList() ?? [];
    }

    /// <inheritdoc />
    public ICollection<IFileDecoder> Decoders { get; }

    /// <inheritdoc />
    public IEnumerable<string> SupportedExtensions
    {
        get
        {
            return _mapper.Where(kvp => Decoders.Any(d => d.SupportsMimeType(kvp.Value)))
                .Select(kvp => kvp.Key)
                .Distinct();
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> SupportedMimeTypes
    {         
        get
        {
            return _mapper.Where(kvp => Decoders.Any(d => d.SupportsMimeType(kvp.Value)))
                .Select(kvp => kvp.Value)
                .Distinct();
        }
    }

    // 확장자 검증용 정규식
    [GeneratedRegex(@"^\.[a-zA-Z0-9]+$", RegexOptions.Compiled)]
    private static partial Regex ExtensionRegex();

    /// <inheritdoc />
    public string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (!_mapper.TryGetValue(extension, out var mimeType))
            throw new NotSupportedException($"Not supported file extension: {extension}");
        return mimeType;
    }

    /// <inheritdoc />
    public void SetContentType(string extension, string mimeType)
    {
        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException("확장자는 비어 있을 수 없습니다.", nameof(extension));

        if (!extension.StartsWith('.'))
            extension = "." + extension;

        // 확장자 검증 (예: .jpg, .png, .html 등)
        if (!ExtensionRegex().IsMatch(extension))
            throw new ArgumentException($"잘못된 확장자 형식입니다: {extension}", nameof(extension));

        _mapper[extension] = mimeType;
    }

    /// <inheritdoc />
    public async Task<string> DecodeAsync(
        string fileName, 
        Stream data, 
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        if (_mapper.TryGetValue(extension, out var mimeType))
        {
            var decoder = Decoders.FirstOrDefault(d => d.SupportsMimeType(mimeType))
                ?? throw new NotSupportedException($"Decoder not found for MIME type: {mimeType}");

            return await decoder.DecodeAsync(data, cancellationToken);
        }
        else
        {
            throw new NotSupportedException($"Not supported file extension: {extension}");
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> ListAsync(
        string storage,
        string? prefix = null,
        int depth = 1,
        CancellationToken cancellationToken = default)
    {
        if (!_storages.TryGet<IFileStorage>(storage, out var service))
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
        if (!_storages.TryGet<IFileStorage>(storage, out var service))
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
        if (!_storages.TryGet<IFileStorage>(storage, out var service))
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
        if (!_storages.TryGet<IFileStorage>(storage, out var service))
            throw new ArgumentException($"저장소 '{storage}'을(를) 찾을 수 없습니다.", nameof(storage));

        await service.WriteFileAsync(filePath, data, overwrite, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string storage,
        string path,
        CancellationToken cancellationToken = default)
    {
        if (!_storages.TryGet<IFileStorage>(storage, out var service))
            throw new ArgumentException($"저장소 '{storage}'을(를) 찾을 수 없습니다.", nameof(storage));

        await service.DeleteAsync(path, cancellationToken);
    }
}

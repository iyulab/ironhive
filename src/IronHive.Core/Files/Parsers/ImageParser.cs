using IronHive.Abstractions.Files;

namespace IronHive.Core.Files.Parsers;

/// <summary>
/// 이미지 파일을 파싱합니다. 원본 바이트를 그대로 <see cref="ImageBlock"/>으로 반환합니다.
/// </summary>
public class ImageParser : IFileParser
{
    private static readonly Dictionary<string, string> ExtensionMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"]  = "image/png",
        [".jpg"]  = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"]  = "image/gif",
        [".webp"] = "image/webp",
    };

    private static readonly HashSet<string> SupportedMimeTypes = new(ExtensionMimeTypes.Values, StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool CanParse(string fileName, string? mimeType = null)
        => ExtensionMimeTypes.ContainsKey(Path.GetExtension(fileName))
        || (mimeType is not null && SupportedMimeTypes.Contains(mimeType));

    /// <inheritdoc />
    public async Task<IReadOnlyList<FileBlock>> ParseAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName);
        // 확장자로 MIME을 결정하며, 확장자가 없는 경우(mimeType만 있는 경우)에는 기본값 image/png를 사용합니다.
        var mimeType = ExtensionMimeTypes.GetValueOrDefault(ext, "image/png");

        using var ms = new MemoryStream();
        await data.CopyToAsync(ms, cancellationToken);
        return [new ImageBlock { MimeType = mimeType, Data = ms.ToArray() }];
    }
}

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

    /// <inheritdoc />
    public bool CanParse(string fileName)
        => ExtensionMimeTypes.ContainsKey(Path.GetExtension(fileName));

    /// <inheritdoc />
    public async Task<IReadOnlyList<FileBlock>> ParseAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName);
        // 확장자로 MIME을 결정하며, 알 수 없는 확장자인 경우 기본값 image/png를 사용합니다.
        var mimeType = ExtensionMimeTypes.GetValueOrDefault(ext, "image/png");

        using var ms = new MemoryStream();
        await data.CopyToAsync(ms, cancellationToken);
        return [new ImageBlock { MimeType = mimeType, Data = ms.ToArray() }];
    }
}

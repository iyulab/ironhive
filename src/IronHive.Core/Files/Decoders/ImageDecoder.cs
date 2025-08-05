using IronHive.Abstractions.Files;

namespace IronHive.Core.Files.Decoders;

/// <summary>
/// 이미지 파일 디코더 클래스입니다.
/// </summary>
public class ImageDecoder : IFileDecoder
{
    /// <inheritdoc />
    public bool SupportsMimeType(string mimeType)
    {
        return mimeType switch
        {
            "image/png" => true,
            "image/jpeg" => true,
            "image/gif" => true,
            _ => false
        };
    }

    /// <inheritdoc />
    public async Task<string> DecodeAsync(Stream data, CancellationToken cancellationToken = default)
    {
        // 스트림을 메모리스트림으로 복사 후 바이트 배열로 변환
        using var memoryStream = new MemoryStream();
        await data.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        // 바이트 배열을 base64 문자열로 변환하여 반환
        return Convert.ToBase64String(bytes);
    }
}

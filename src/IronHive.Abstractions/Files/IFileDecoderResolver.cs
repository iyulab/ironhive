using System.Diagnostics.CodeAnalysis;

namespace IronHive.Abstractions.Files;

/// <summary>
/// 파일 확장자 기반의 MimeType 조회 및 지원 디코더 선택 기능을 제공합니다.
/// </summary>
public interface IFileDecoderResolver
{
    /// <summary>
    /// 현재 등록된 디코더들이 지원하는 파일 확장자 리스트를 반환합니다.
    /// </summary>
    IEnumerable<string> GetSupportedExtensions();

    /// <summary>
    /// 파일 확장자(예: ".jpg")에 해당하는 MimeType(예: "image/jpeg")을 반환합니다.
    /// </summary>
    string? GetMimeType(string filePath);

    /// <summary>
    /// 파일 확장자에 해당하는 MimeType을 처리할 수 있는 IFileDecoder 반환을 시도합니다.
    /// </summary>
    bool TryGetDecoderByName(string filePath, [MaybeNullWhen(false)] out IFileDecoder decoder);

    /// <summary>
    /// 파일 확장자에 해당하는 MimeType을 처리할 수 있는 IFileDecoder를 반환합니다.
    /// </summary>
    IFileDecoder? GetDecoderByName(string filePath);
}

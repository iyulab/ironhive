namespace IronHive.Abstractions.Files;

/// <summary>
/// 특정 파일 형식을 파싱하여 <see cref="FileBlock"/> 목록으로 변환하는 파서입니다.
/// </summary>
public interface IFileParser
{
    /// <summary>
    /// 이 파서가 해당 파일을 처리할 수 있는지 확인합니다.
    /// </summary>
    /// <param name="fileName">파일명 또는 경로 (확장자 기반 판별)</param>
    /// <param name="mimeType">MIME 타입 힌트. 확장자가 없거나 불명확할 때 보조로 사용됩니다.</param>
    bool CanParse(string fileName, string? mimeType = null);

    /// <summary>
    /// 파일 스트림을 파싱하여 블록 목록을 반환합니다.
    /// </summary>
    /// <param name="fileName">파일명 (라벨 생성에 사용)</param>
    /// <param name="data">파일 스트림</param>
    Task<IReadOnlyList<FileBlock>> ParseAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default);
}

namespace IronHive.Abstractions.Files;

/// <summary>
/// 파일 스트림을 <see cref="FileBlock"/> 목록으로 파싱하는 서비스입니다.
/// 등록된 <see cref="IFileParser"/> 중 처리 가능한 파서를 선택하고,
/// 일치하는 파서가 없으면 null byte 휴리스틱으로 텍스트/바이너리를 자동 판별합니다.
/// </summary>
public interface IFileParserService
{
    /// <summary>
    /// 파일을 파싱하여 블록 목록을 반환합니다.
    /// </summary>
    /// <param name="fileName">파일명 또는 경로 (파서 선택 및 라벨 생성에 사용)</param>
    /// <param name="data">파일 스트림</param>
    Task<IReadOnlyList<FileBlock>> ParseAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default);
}

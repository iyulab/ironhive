namespace IronHive.Abstractions.Files;

/// <summary>
/// IFileService 빌더 인터페이스입니다.
/// </summary>
public interface IFileServiceBuilder
{
    /// <summary>
    /// 파일 디코더를 추가합니다.
    /// </summary>
    IFileServiceBuilder AddDecoder(IFileDecoder decoder);

    /// <summary>
    /// 서비스를 빌드합니다.
    /// </summary>
    IFileService Build();
}

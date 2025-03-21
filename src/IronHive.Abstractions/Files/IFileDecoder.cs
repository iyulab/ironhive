namespace IronHive.Abstractions.Files;

/// <summary>
/// 파일 디코더 인터페이스입니다.
/// 스트림 데이터에서 텍스트를 추출하고, 지정된 MIME 타입에 대한 지원 여부를 확인합니다.
/// </summary>
public interface IFileDecoder
{
    /// <summary>
    /// 지정된 MIME 타입이 지원되는지 확인합니다.
    /// </summary>
    /// <param name="mimeType">확인할 컨텐츠의 MIME 타입</param>
    /// <returns>지원되는 MIME 타입인 경우 true, 그렇지 않으면 false</returns>
    bool SupportsMimeType(string mimeType);

    /// <summary>
    /// 주어진 스트림 데이터를 기반으로 텍스트를 추출합니다.
    /// </summary>
    /// <param name="data">텍스트 추출 대상 스트림 데이터</param>
    Task<string> DecodeAsync(
        Stream data,
        CancellationToken cancellationToken = default);
}

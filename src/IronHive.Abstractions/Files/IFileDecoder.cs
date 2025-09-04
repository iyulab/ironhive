namespace IronHive.Abstractions.Files;

/// <summary>
/// 스트림 데이터에서 타입 <typeparamref name="T"/>의 객체를 추출합니다.
/// </summary>
public interface IFileDecoder<T>
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
    Task<T> DecodeAsync(
        Stream data,
        CancellationToken cancellationToken = default);
}

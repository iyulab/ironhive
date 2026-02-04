using System.Text.Json;
using System.Text.Json.Nodes;

namespace IronHive.Providers.OpenAI.Compatible.Adapters;

/// <summary>
/// OpenAI 호환 서비스 제공자별 요청/응답 변환을 담당하는 어댑터 인터페이스입니다.
/// </summary>
public interface IProviderAdapter
{
    /// <summary>
    /// 제공자 타입을 가져옵니다.
    /// </summary>
    CompatibleProvider Provider { get; }

    /// <summary>
    /// 기본 URL을 반환합니다.
    /// </summary>
    string GetBaseUrl(CompatibleConfig config);

    /// <summary>
    /// HTTP 요청에 추가할 헤더를 반환합니다.
    /// </summary>
    IDictionary<string, string> GetAdditionalHeaders(CompatibleConfig config);

    /// <summary>
    /// 요청 본문을 제공자 형식으로 변환합니다.
    /// </summary>
    /// <param name="request">원본 OpenAI 형식의 요청 JSON</param>
    /// <param name="config">설정</param>
    /// <returns>변환된 요청 JSON</returns>
    JsonObject TransformRequest(JsonObject request, CompatibleConfig config);

    /// <summary>
    /// 응답 본문을 OpenAI 형식으로 변환합니다.
    /// </summary>
    /// <param name="response">제공자 응답 JSON</param>
    /// <returns>OpenAI 형식으로 변환된 응답 JSON</returns>
    JsonObject TransformResponse(JsonObject response);

    /// <summary>
    /// 스트리밍 응답의 각 청크를 OpenAI 형식으로 변환합니다.
    /// </summary>
    /// <param name="chunk">제공자 스트리밍 청크 JSON</param>
    /// <returns>OpenAI 형식으로 변환된 청크 JSON</returns>
    JsonObject TransformStreamingChunk(JsonObject chunk);

    /// <summary>
    /// 제공자가 지원하지 않는 파라미터를 제거합니다.
    /// </summary>
    /// <param name="request">요청 JSON</param>
    void RemoveUnsupportedParameters(JsonObject request);
}

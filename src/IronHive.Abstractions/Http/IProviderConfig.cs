using System.Text.Json;

namespace IronHive.Abstractions.Http;

/// <summary>
/// AI Provider 설정을 위한 공통 인터페이스
/// </summary>
public interface IProviderConfig
{
    /// <summary>
    /// API 엔드포인트의 기본 URL
    /// </summary>
    string BaseUrl { get; set; }

    /// <summary>
    /// JSON 직렬화/역직렬화 옵션
    /// </summary>
    JsonSerializerOptions JsonOptions { get; set; }

    /// <summary>
    /// Provider의 기본 Base URL을 반환합니다.
    /// </summary>
    string GetDefaultBaseUrl();

    /// <summary>
    /// HttpClient에 Provider별 설정을 적용합니다.
    /// </summary>
    /// <param name="client">설정을 적용할 HttpClient</param>
    void ConfigureHttpClient(HttpClient client);
}

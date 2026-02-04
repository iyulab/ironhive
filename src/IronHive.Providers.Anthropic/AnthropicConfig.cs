using System.Text.Json;
using System.Text.Json.Serialization;
using IronHive.Abstractions.Http;

namespace IronHive.Providers.Anthropic;

/// <summary>
/// Anthropic API에 대한 설정을 나타냅니다.
/// </summary>
public class AnthropicConfig : IProviderConfig
{
    /// <summary>
    /// Anthropic API의 기본 URL입니다.
    /// (Default: "https://api.anthropic.com/v1/")
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API 서비스의 인증 키입니다.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// API 서비스의 버전을 나타냅니다.
    /// (Default: "2023-06-01")
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 요청/응답간의 JSON 직렬화 옵션입니다.
    /// </summary>
    [JsonIgnore]
    public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
        }
    };

    /// <inheritdoc />
    public string GetDefaultBaseUrl() => AnthropicConstants.DefaultBaseUrl;

    /// <inheritdoc />
    public void ConfigureHttpClient(HttpClient client)
    {
        var version = string.IsNullOrWhiteSpace(Version)
            ? AnthropicConstants.VersionHeaderValue
            : Version;
        client.DefaultRequestHeaders.Add(AnthropicConstants.VersionHeaderName, version);

        if (!string.IsNullOrWhiteSpace(ApiKey))
            client.DefaultRequestHeaders.Add(AnthropicConstants.AuthorizationHeaderName, ApiKey);
    }
}
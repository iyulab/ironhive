using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Encodings.Web;
using IronHive.Abstractions.Http;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// OpenAI에 대한 설정 클래스입니다.
/// </summary>
public class OpenAIConfig : IProviderConfig
{
    /// <summary>
    /// OpenAI API의 기본 URL을 가져오거나 설정합니다.
    /// (Default: "https://api.openai.com/v1/")
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI API 키를 가져오거나 설정합니다.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI 계정의 조직 ID를 가져오거나 설정합니다.
    /// 이 옵션은 선택 사항이며 청구할 조직을 지정하는 데 사용할 수 있습니다.
    /// </summary>
    public string Organization { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI 프로젝트 ID를 가져오거나 설정합니다.
    /// 이 옵션은 선택 사항이며 특정 프로젝트에 대한 요청을 지정하는 데 사용할 수 있습니다.
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// 요청/응답의 JSON 직렬화 및 역직렬화에 사용되는 옵션을 가져오거나 설정합니다.
    /// </summary>
    [JsonIgnore]
    public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        //PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        AllowOutOfOrderMetadataProperties = true, // 다형성 역직렬화시 첫번째 속성이 아닌 경우에도 인식(성능 저하 있음)
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
        },
    };

    /// <inheritdoc />
    public string GetDefaultBaseUrl() => OpenAIConstants.DefaultBaseUrl;

    /// <inheritdoc />
    public void ConfigureHttpClient(HttpClient client)
    {
        if (!string.IsNullOrWhiteSpace(ApiKey))
            client.DefaultRequestHeaders.Add(
                OpenAIConstants.AuthorizationHeaderName,
                string.Format(OpenAIConstants.AuthorizationHeaderValue, ApiKey));

        if (!string.IsNullOrWhiteSpace(Organization))
            client.DefaultRequestHeaders.Add(
                OpenAIConstants.OrganizationHeaderName, Organization);

        if (!string.IsNullOrWhiteSpace(Project))
            client.DefaultRequestHeaders.Add(
                OpenAIConstants.ProjectHeaderName, Project);
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Providers.Ollama;

/// <summary>
/// Ollama API에 연결하는 데 필요한 구성 설정을 나타냅니다.
/// </summary>
public class OllamaConfig
{
    /// <summary>  
    /// Ollama API의 기본 URL을 가져오거나 설정합니다.
    /// (Default: "http://localhost:11434/api/")
    /// </summary>  
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>  
    /// Api에 대한 요청에 사용할 기본 요청 헤더를 가져오거나 설정합니다.
    /// </summary>  
    public IDictionary<string, string> DefaultRequestHeaders { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// 요청/응답의 Json 직렬화 옵션을 가져오거나 설정합니다.
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
}
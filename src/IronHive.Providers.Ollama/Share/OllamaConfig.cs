using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Providers.Ollama.Share;

/// <summary>  
/// Represents the configuration settings for the Ollama connector.  
/// </summary>  
public class OllamaConfig
{
    /// <summary>  
    /// Gets or sets the endpoint URL for the Ollama API.
    /// Default endpoint is "http://localhost:11434/api/".
    /// </summary>  
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>  
    /// Gets or sets the default request headers for the Ollama API requests.  
    /// </summary>  
    public IDictionary<string, string> DefaultRequestHeaders { get; set; } = new Dictionary<string, string>();

    /// <summary>  
    /// Gets or sets the JSON serializer options for the Ollama API requests.  
    /// </summary>  
    [JsonIgnore]
    public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };
}

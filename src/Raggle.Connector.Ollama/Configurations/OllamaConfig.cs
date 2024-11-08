using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raggle.Connector.Ollama.Configurations;

/// <summary>  
/// Represents the configuration settings for the Ollama connector.  
/// </summary>  
public class OllamaConfig
{
    /// <summary>  
    /// Gets or sets the endpoint URL for the Ollama API.
    /// Default endpoint is <see cref="OllamaConstants.DefaultEndPoint"/>
    /// </summary>  
    public string EndPoint { get; set; } = string.Empty;

    /// <summary>  
    /// Gets or sets the default request headers for the Ollama API requests.  
    /// </summary>  
    public Dictionary<string, string>? DefaultRequestHeaders { get; set; }

    /// <summary>  
    /// Gets or sets the JSON serializer options for the Ollama API requests.  
    /// </summary>  
    public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };
}

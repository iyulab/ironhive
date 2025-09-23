using IronHive.Providers.OpenAI.ChatCompletion;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses;

internal class ResponsesText
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("format")]
    public ChatResponseFormat? Format {  get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("verbosity")]
    public ChatVerbosity? Verbosity { get; set; }
}

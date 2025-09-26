using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses.Models;

internal class ResponsesText
{
    [JsonPropertyName("format")]
    public ResponsesFormat? Format {  get; set; }

    /// <summary>
    /// 모델 응답의 세부 수준을 제어합니다. (default- medium)
    /// </summary>
    [JsonPropertyName("verbosity")]
    public ResponsesVerbosity? Verbosity { get; set; }
}

using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

/// <summary>
/// OpenAI 플랫폼 대시보드에서 제공하는 프롬프트 기능을 사용합니다.
/// </summary>
internal class ResponsesPrompt
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("variables")]
    public IDictionary<string, object>? Variables { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }
}

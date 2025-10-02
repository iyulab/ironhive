using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.Models;

internal class ModelsListResponse
{
    [JsonPropertyName("models")]
    public required IEnumerable<GoogleAIModel> Models { get; set; }

    /// <summary>
    /// First ID in the data list. Can be used as the before_id for the previous page.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}
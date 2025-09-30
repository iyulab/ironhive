using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Catalog.Models;

public class GoogleAIModelsResponse
{
    /// <summary>
    /// 데이터 목록
    /// </summary>
    [JsonPropertyName("models")]
    public required IEnumerable<GoogleAIModel> Models { get; set; }

    /// <summary>
    /// First ID in the data list. Can be used as the before_id for the previous page.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}

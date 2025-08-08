using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Catalog;

public class AnthropicListModelsResponse
{
    /// <summary>
    /// 데이터 목록
    /// </summary>
    [JsonPropertyName("data")]
    public required IEnumerable<AnthropicModel> Data { get; set; }

    /// <summary>
    /// First ID in the data list. Can be used as the before_id for the previous page.
    /// </summary>
    [JsonPropertyName("first_id")]
    public string? FirstId { get; set; }

    /// <summary>
    /// Indicates if there are more results in the requested page direction.
    /// </summary>
    [JsonPropertyName("has_more")]
    public required bool HasMore { get; set; }

    /// <summary>
    /// Last ID in the data list. Can be used as the after_id for the next page.
    /// </summary>
    [JsonPropertyName("last_id")]
    public string? LastId { get; set; }
}

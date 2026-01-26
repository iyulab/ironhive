using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Models;

public class ListModelsRequest
{
    /// <summary>
    /// ID of the object to use as a cursor for pagination.
    /// </summary>
    [JsonPropertyName("after_id")]
    public string? AfterId { get; set; }

    /// <summary>
    /// ID of the object to use as a cursor for pagination.
    /// </summary>
    [JsonPropertyName("before_id")]
    public string? BeforeId { get; set; }

    /// <summary>
    /// Number of items to return per page.
    /// Defaults to 20. Ranges from 1 to 1000.
    /// </summary>
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }
}

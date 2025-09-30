using IronHive.Providers.GoogleAI.Share.Models;
using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.EmbedContent.Models;

internal class EmbedContentRequest
{
    [JsonPropertyName("content")]
    public required Content Content { get; set; }

    [JsonPropertyName("taskType")]
    public EmbedTaskType? TaskType { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("outputDimensionality")]
    public int? OutputDimensionality { get; set; }
}

internal class BatchEmbedContentRequest
{
    [JsonPropertyName("requests")]
    public ICollection<EmbedContentRequest> Requests { get; set; } = [];
}
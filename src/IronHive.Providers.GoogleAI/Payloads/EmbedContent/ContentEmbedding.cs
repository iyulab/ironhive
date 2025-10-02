using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.EmbedContent;

internal class ContentEmbedding
{
    [JsonPropertyName("values")]
    public float[] Values { get; set; } = [];
}

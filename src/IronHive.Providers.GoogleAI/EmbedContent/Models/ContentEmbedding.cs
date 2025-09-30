using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.EmbedContent.Models;

internal class ContentEmbedding
{
    [JsonPropertyName("values")]
    public float[] Values { get; set; } = [];
}

using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

/// <summary>그라운딩(검색/문서) 관련 데이터.</summary>
internal sealed class GroundingData
{
    [JsonPropertyName("sourceId")]
    public SourceData? Source { get; set; }

    [JsonPropertyName("content")]
    public Content? Content { get; set; }

    /// <summary>
    /// Union 타입: groundingPassage 또는 semanticRetrieverChunk 중 하나.
    /// </summary>
    internal sealed class SourceData
    {
        [JsonPropertyName("groundingPassage")]
        public PassageData? Passage { get; set; }

        [JsonPropertyName("semanticRetrieverChunk")]
        public ChunkData? Chunk { get; set; }
    }

    internal sealed class PassageData
    {
        [JsonPropertyName("passageId")]
        public string? PassageId { get; set; }

        [JsonPropertyName("partIndex")]
        public int? PartIndex { get; set; }
    }

    internal sealed class ChunkData
    {
        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("chunk")]
        public string? Chunk { get; set; }
    }
}
